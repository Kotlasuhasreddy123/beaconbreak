using System;
using System.Collections.Generic;
using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap : MonoBehaviour
    {
        private enum ScreenMode { Menu, Connecting, Fight, Report, Skills, Defeat, Credits, Chronicle, Guide, Profile }
        private enum PlayMode { Local, Host, Client }
        private ScreenMode screen = ScreenMode.Menu;
        private PlayMode playMode = PlayMode.Local;
        private MatchState match;
        private RollbackHost rollback;
        private DirectConnection network;
        private readonly Dictionary<int, Buttons> clientInputs = new Dictionary<int, Buttons>();
        private Buttons lastHostButtons;
        private Buttons lastLocalHostButtons;
        private int clientSentTick;
        private CounterProfile nextProfile;
        private int nextLevel = 1;
        private int skillMask;
        private ScreenMode returnFromSkills = ScreenMode.Menu;
        private bool resultHandled;
        private float accumulator;
        private string status = "";
        private string joinAddress = "127.0.0.1";
        private const int Port = 27551;
        private Texture2D pixel;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private int encounter;
        private int receivedStateTick = -1;
        private bool helloSent, peerVerified, peerReady, localReady;
        private float lastPacketTime, lastPingTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null) return;
            GameObject root = new GameObject("Co-op Fighter Game");
            DontDestroyOnLoad(root);
            root.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply();
            int saved = PlayerPrefs.GetInt("cf_highest_level", 1);
            nextLevel = StoryCampaign.Normalize(saved);
            skillMask = PlayerPrefs.GetInt("cf_skill_mask", 0);
            LoadPersonalRecord();
            string savedProfile = PlayerPrefs.GetString("cf_next_profile", "");
            if (!string.IsNullOrEmpty(savedProfile))
            {
                try { nextProfile = JsonUtility.FromJson<CounterProfile>(savedProfile); }
                catch (Exception) { nextProfile = null; }
            }
        }

        private void Update()
        {
            PollNetwork();
            UpdateSession();
            if (screen == ScreenMode.Report && !CinemaActive && playMode != PlayMode.Client &&
                Input.GetKeyDown(KeyCode.Return)) localReady = true;
            if (screen == ScreenMode.Defeat && playMode != PlayMode.Client &&
                Input.GetKeyDown(KeyCode.R)) localReady = true;
            if (screen != ScreenMode.Fight || match == null) return;

            if (playMode == PlayMode.Host && match.Result != MatchResult.Fighting)
            {
                accumulator = 0f;
                if (!resultHandled && rollback.ResultConfirmed) HandleResult();
                return;
            }

            accumulator += Mathf.Min(0.1f, Time.unscaledDeltaTime);
            int steps = 0;
            while (accumulator >= 1f / Simulation.TicksPerSecond && steps < 5)
            {
                accumulator -= 1f / Simulation.TicksPerSecond;
                if (playMode == PlayMode.Local)
                    Simulation.Step(match, ReadPlayerOne(), ReadPlayerTwo());
                else if (playMode == PlayMode.Host)
                {
                    Buttons input = ReadPlayerOne();
                    lastLocalHostButtons = input;
                    rollback.Step(input);
                    match = rollback.State;
                    if (match.Tick % 3 == 0 || match.Result != MatchResult.Fighting)
                        SendState(input);
                }
                else
                {
                    if (clientSentTick >= receivedStateTick + RollbackHost.MaxPredictionTicks) {
                        accumulator = 0f; break;
                    }
                    int tick = ++clientSentTick;
                    Buttons input = ReadPlayerOne();
                    clientInputs[tick] = input;
                    Send(new NetworkEnvelope { kind = "input", tick = tick, buttons = (int)input });
                    Simulation.Step(match, lastHostButtons, input);
                }
                steps++;
            }
            if (match.Result != MatchResult.Fighting && !resultHandled &&
                (playMode == PlayMode.Local || playMode == PlayMode.Host && rollback.ResultConfirmed))
                HandleResult();
        }

        private void StartLevel(int level)
        {
            encounter++;
            localReady = peerReady = false;
            CounterProfile profile = nextProfile != null && nextProfile.TargetLevel == level ? nextProfile : null;
            match = Simulation.Start(level, profile, 0xABC000 + level * 97, skillMask);
            rollback = playMode == PlayMode.Host ? new RollbackHost(match) : null;
            clientInputs.Clear();
            clientSentTick = 0;
            lastHostButtons = Buttons.None;
            lastLocalHostButtons = Buttons.None;
            resultHandled = false;
            accumulator = 0f;
            status = "";
            screen = ScreenMode.Fight;
            if (playMode == PlayMode.Host) SendState(Buttons.None);
        }

        private void HandleResult()
        {
            resultHandled = true;
            SavePersonalVictory(match);
            if (match.Result == MatchResult.Defeat)
            {
                screen = ScreenMode.Defeat;
                localReady = peerReady = false;
                LogTestResult("defeat", match);
                if (playMode == PlayMode.Host) Send(new NetworkEnvelope { kind = "defeat", state = match.Copy() });
                return;
            }
            if (match.Level == 100)
            {
                if (!automatedTest && !browserBot) { PlayerPrefs.SetInt("cf_highest_level", 100); PlayerPrefs.Save(); }
                screen = ScreenMode.Credits;
                BeginCinema();
                LogTestResult("credits", match);
                if (playMode == PlayMode.Host) Send(new NetworkEnvelope { kind = "credits", state = match.Copy() });
                return;
            }

            nextProfile = CounterProfile.FromBattle(match.Level, match.Telemetry.Copy());
            nextLevel = nextProfile.TargetLevel;
            // This local save is performed once when the fight first reaches
            // victory. An online host will be the sole progression authority.
            if (!automatedTest && !browserBot) {
                PlayerPrefs.SetString("cf_next_profile", JsonUtility.ToJson(nextProfile));
                PlayerPrefs.Save();
            }
            screen = ScreenMode.Report;
            BeginCinema();
            localReady = peerReady = false;
            LogTestResult("report", match);
            if (playMode == PlayMode.Host)
                Send(new NetworkEnvelope { kind = "report", profile = nextProfile, level = nextLevel, state = match.Copy() });
        }

        private void StartHost()
        {
            CloseNetwork();
            if (!automatedTest && !browserBot) { nextLevel=personalLevel; skillMask=PlayerPrefs.GetInt("cf_skill_mask",0); }
            webRoom = requestedRoom ?? Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
            requestedRoom = null;
#if UNITY_WEBGL && !UNITY_EDITOR
            DirectConnection.Room = webRoom;
#endif
            playMode = PlayMode.Host;
            network = new DirectConnection();
            network.Host(Port);
            localReady = true;
            status = "Room open. Your partner joins from their own computer.";
            screen = ScreenMode.Connecting;
        }

        private void JoinHost()
        {
            if (IsWeb && !System.Text.RegularExpressions.Regex.IsMatch(joinAddress.Trim(), "^[a-fA-F0-9]{8}$")) {
                status="Enter the 8-character code from your partner's open room.";return;
            }
            CloseNetwork();
            webRoom = joinAddress.Trim().ToUpperInvariant();
            playMode = PlayMode.Client;
            network = new DirectConnection();
            network.Join(joinAddress.Trim(), Port);
            status = "Connecting to " + joinAddress + ":" + Port + "...";
            screen = ScreenMode.Connecting;
        }

        private void PollNetwork()
        {
            if (network == null) return;
            string line;
            while (network.TryReceive(out line))
            {
                if (line.StartsWith("@")) { status = line.Substring(1); continue; }
                if (line.StartsWith("!"))
                {
                    status = line.Substring(1);
                    CloseNetwork();
                    screen = ScreenMode.Menu;
                    break;
                }
                NetworkEnvelope packet;
                try { packet = JsonUtility.FromJson<NetworkEnvelope>(line); }
                catch (Exception) { status = "Invalid network message."; continue; }
                if (packet == null) continue;
                lastPacketTime = Time.realtimeSinceStartup;
                if (packet.kind == "hello") {
                    if (!SessionProtocol.Compatible(packet.version)) {
                        status = "Game versions differ. Install the same build on both devices.";
                        Send(new NetworkEnvelope { kind = "reject", message = status });
                        continue;
                    }
                    peerVerified = true;
                    if (playMode == PlayMode.Client) nextLevel = Mathf.Clamp(packet.level, 1, 100);
                    continue;
                }
                if (packet.kind == "reject") { status = packet.message; CloseNetwork(); screen = ScreenMode.Menu; break; }
                if (!peerVerified) continue;
                if (packet.kind == "ping") { Send(new NetworkEnvelope { kind = "pong" }); continue; }
                if (packet.kind == "pong") continue;
                if (packet.kind == "cinema_vote" && playMode == PlayMode.Host && packet.encounter == encounter && CinemaActive) { remoteCinemaVote=true;continue; }
                if (packet.kind == "cinema_end" && playMode == PlayMode.Client && packet.encounter == encounter) { cinemaComplete=true;continue; }
                if (packet.kind == "ready" && playMode == PlayMode.Host && packet.encounter == encounter && !CinemaActive) { peerReady = true; continue; }
                if (playMode == PlayMode.Host && packet.encounter != encounter) continue;
                if (playMode == PlayMode.Host && packet.kind == "input" && rollback != null)
                {
                    if (packet.tick < 1 || packet.tick > rollback.State.Tick + RollbackHost.MaxPredictionTicks) continue;
                    try
                    {
                        rollback.SubmitRemote(packet.tick, (Buttons)packet.buttons);
                        match = rollback.State;
                        if (match.Result != MatchResult.Fighting) SendState(lastLocalHostButtons);
                    }
                    catch (Exception ex) { status = "Input sync error: " + ex.Message; }
                }
                else if (playMode == PlayMode.Client && packet.kind == "state" && packet.state != null)
                    ApplyHostState(packet);
                else if (playMode == PlayMode.Client && packet.kind == "report")
                {
                    if (packet.encounter != encounter) continue;
                    if (packet.state != null) match = packet.state;
                    SavePersonalVictory(match);
                    nextLevel = packet.level;
                    nextProfile = packet.profile;
                    screen = ScreenMode.Report;
                    BeginCinema();
                    localReady = false;
                    LogTestResult("report", match);
                }
                else if (playMode == PlayMode.Client && (packet.kind == "defeat" || packet.kind == "credits") && packet.encounter == encounter) {
                    if (packet.state != null) match = packet.state;
                    SavePersonalVictory(match);
                    screen = packet.kind == "defeat" ? ScreenMode.Defeat : ScreenMode.Credits;
                    if(packet.kind=="credits")BeginCinema();
                    localReady = false; LogTestResult(packet.kind, match);
                }
            }
        }

        private void ApplyHostState(NetworkEnvelope packet)
        {
            if (packet.encounter < encounter) return;
            bool newLevel = match == null || encounter != packet.encounter;
            // A rollback can correct a snapshot without advancing its tick.
            if (!newLevel && packet.state.Tick < receivedStateTick) return;
            if (newLevel)
            {
                encounter = packet.encounter;
                clientInputs.Clear();
                clientSentTick = 0;
                screen = ScreenMode.Fight;
            }
            receivedStateTick = packet.state.Tick;
            // Every host tick needs an acknowledged remote command, including ticks
            // the guest missed while loading. Fill those gaps explicitly with neutral.
            while (clientSentTick < packet.state.Tick) {
                clientSentTick++;
                Send(new NetworkEnvelope { kind = "input", tick = clientSentTick, buttons = 0 });
            }
            int predictedTick = newLevel ? packet.state.Tick : clientSentTick;
            match = packet.state.Copy();
            lastHostButtons = (Buttons)packet.hostButtons;
            resultHandled = false;
            for (int tick = match.Tick + 1; tick <= predictedTick; tick++)
            {
                Buttons input;
                if (!clientInputs.TryGetValue(tick, out input)) input = Buttons.None;
                Simulation.Step(match, lastHostButtons, input);
            }
            // Authoritative snapshots make older input history unnecessary.
            var expired = new List<int>();
            foreach (int tick in clientInputs.Keys) if (tick <= packet.state.Tick) expired.Add(tick);
            foreach (int tick in expired) clientInputs.Remove(tick);
        }

        private void SendState(Buttons hostButtons)
        {
            if (network == null || !network.Connected || match == null) return;
            Send(new NetworkEnvelope { kind = "state", tick = match.Tick,
                hostButtons = (int)hostButtons, state = match.Copy() });
        }

        private void Send(NetworkEnvelope packet)
        {
            packet.encounter = encounter;
            packet.version = SessionProtocol.Version;
            if (network != null && network.Connected) network.Send(JsonUtility.ToJson(packet));
        }

        private void CloseNetwork()
        {
            if (network != null) { network.Dispose(); network = null; }
            rollback = null;
            clientInputs.Clear();
            helloSent = peerVerified = peerReady = localReady = false;
            encounter = 0; receivedStateTick = -1;
            cinemaComplete=true;cinemaEncounter=-1;cinemaVote=remoteCinemaVote=false;
            match = null;
        }

        private Buttons ReadPlayerOne()
        {
            if ((automatedTest || browserBot) && match != null) return PlayerBot.Read(match, playMode == PlayMode.Host ? 0 : 1, PlayerPersona.TandemPartner);
            Buttons value = TouchButtons();
            if (Input.GetKey(KeyCode.A)) value |= Buttons.Left;
            if (Input.GetKey(KeyCode.D)) value |= Buttons.Right;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) value |= Buttons.Jump;
            if (Input.GetKey(KeyCode.J)) value |= Buttons.Low;
            if (Input.GetKey(KeyCode.K)) value |= Buttons.High;
            if (Input.GetKey(KeyCode.L)) value |= Buttons.Guard;
            if (Input.GetKey(KeyCode.U)) value |= Buttons.Tandem;
            return value;
        }

        private static Buttons ReadPlayerTwo()
        {
            Buttons value = Buttons.None;
            if (Input.GetKey(KeyCode.LeftArrow)) value |= Buttons.Left;
            if (Input.GetKey(KeyCode.RightArrow)) value |= Buttons.Right;
            if (Input.GetKey(KeyCode.UpArrow)) value |= Buttons.Jump;
            if (Input.GetKey(KeyCode.Keypad1)) value |= Buttons.Low;
            if (Input.GetKey(KeyCode.Keypad2)) value |= Buttons.High;
            if (Input.GetKey(KeyCode.Keypad3)) value |= Buttons.Guard;
            if (Input.GetKey(KeyCode.Keypad0)) value |= Buttons.Tandem;
            return value;
        }

        private void OnGUI()
        {
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                new Vector3(Screen.width / 1280f, Screen.height / 720f, 1f));
            EnsureStyles();
            DrawBackground();
            if (CinemaActive) DrawVictoryCinema();
            else if (screen == ScreenMode.Menu) DrawMenu();
            else if (screen == ScreenMode.Connecting) DrawConnecting();
            else if (screen == ScreenMode.Fight) DrawFight();
            else if (screen == ScreenMode.Report) DrawReport();
            else if (screen == ScreenMode.Skills) DrawUpgrades();
            else if (screen == ScreenMode.Chronicle) DrawChronicle();
            else if (screen == ScreenMode.Guide) DrawGuide();
            else if (screen == ScreenMode.Profile) DrawProfile();
            else if (screen == ScreenMode.Defeat) DrawDefeat();
            else DrawCredits();
            if (screen == ScreenMode.Credits && !CinemaActive && ActionButton(new Rect(90, 620, 300, 50), "Return to camp", true)) {
                ReturnToHub();
            }
            GUI.matrix = previous;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 34; titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(1f, 0.87f, 0.65f);
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 20; bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = new Color(0.88f, 0.9f, 0.96f);
            smallStyle = new GUIStyle(bodyStyle); smallStyle.fontSize = 15;
        }

        private void DrawBackgroundLegacy()
        {
            Box(new Rect(0, 0, 1280, 720), new Color(0.035f, 0.05f, 0.09f));
            Box(new Rect(0, 420, 1280, 300), new Color(0.09f, 0.11f, 0.17f));
            for (int i = 0; i < 11; i++)
            {
                Box(new Rect(i * 135 - 20, 270 + i % 3 * 25, 42, 200),
                    new Color(0.12f, 0.15f, 0.22f));
                Box(new Rect(i * 135 - 40, 265 + i % 3 * 25, 82, 14),
                    new Color(0.17f, 0.19f, 0.27f));
            }
            Box(new Rect(0, 590, 1280, 130), new Color(0.16f, 0.14f, 0.16f));
            Box(new Rect(0, 590, 1280, 6), new Color(0.6f, 0.35f, 0.25f));
        }

        private void DrawMenuLegacy()
        {
            GUI.Label(new Rect(100, 90, 1000, 60), "BEACONBREAK", titleStyle);
            GUI.Label(new Rect(100, 155, 1000, 40), "A two-fighter campaign against the adaptive Wardens", bodyStyle);
            GUI.Label(new Rect(100, 240, 1000, 120),
                "100 encounters · coordinated strikes · friendly fire · bosses that remember", bodyStyle);
            GUI.Label(new Rect(100, 335, 1000, 40), "Continue at Level " + nextLevel, bodyStyle);
            if (GUI.Button(new Rect(100, 390, 220, 48), "Local co-op"))
            {
                playMode = PlayMode.Local;
                StartLevel(nextLevel);
            }
            if (GUI.Button(new Rect(340, 390, 220, 48), "Host online")) StartHost();
            if (GUI.Button(new Rect(580, 390, 220, 48), "Join online")) JoinHost();
            GUI.Label(new Rect(100, 455, 100, 32), "Host IP", smallStyle);
            joinAddress = GUI.TextField(new Rect(200, 455, 270, 30), joinAddress);
            GUI.Label(new Rect(100, 510, 1070, 100),
                "Player 1: A/D move, W jump, J low, K high, L guard, U tandem\n" +
                "Player 2: arrows move/jump, keypad 1 low, 2 high, 3 guard, 0 tandem", smallStyle);
            if (GUI.Button(new Rect(830, 390, 220, 48), "Skill tree (" +
                SkillTree.PointsAvailable(nextLevel, skillMask) + " points)"))
            {
                returnFromSkills = ScreenMode.Menu;
                screen = ScreenMode.Skills;
            }
            GUI.Label(new Rect(100, 630, 1070, 40), status, smallStyle);
        }

        private void DrawConnectingLegacy()
        {
            GUI.Label(new Rect(100, 145, 1000, 60), "CONNECTING FIGHTERS", titleStyle);
            GUI.Label(new Rect(100, 250, 1000, 100), status, bodyStyle);
            GUI.Label(new Rect(100, 395, 1000, 90),
                "Direct online play uses TCP port " + Port + ". The host may need port forwarding for players outside the same network.",
                smallStyle);
            if (GUI.Button(new Rect(100, 545, 220, 48), "Cancel"))
            {
                CloseNetwork();
                screen = ScreenMode.Menu;
            }
        }

        private void DrawFightLegacy()
        {
            LevelSpec spec = LevelSpec.Create(match.Level);
            GUI.Label(new Rect(35, 16, 1200, 42), "LEVEL " + match.Level + " / 100  ·  " + spec.ActName, titleStyle);
            DrawHealth(35, 72, "P1", match.Fighters[0], new Color(0.18f, 0.78f, 0.95f));
            DrawHealth(35, 112, "P2", match.Fighters[1], new Color(0.92f, 0.76f, 0.25f));
            DrawHealth(790, 72, spec.BossName, match.Fighters[2], new Color(0.95f, 0.29f, 0.36f));
            for (int i = 0; i < 3; i++) DrawFighter(match.Fighters[i], i);
            if (match.LastHitDamage > 0)
                GUI.Label(new Rect(450, 205, 500, 35),
                    (match.LastHitFriendly ? "ALLY HIT  " : "HIT  ") + match.LastHitDamage, bodyStyle);
            GUI.Label(new Rect(34, 635, 1200, 32),
                "Weapon: " + spec.WeaponName + "  ·  Combo cap: " + spec.MaxCombo +
                "  ·  Tandem " +
                (spec.TandemUnlocked ? match.TandemCooldown == 0 ? "READY" : "recharging" : "unlocks at Level 5") +
                "  ·  Friendly hits " + match.Telemetry.FriendlyHits, smallStyle);
            if (match.Adaptation != null)
                GUI.Label(new Rect(34, 675, 1200, 26),
                    "Boss intelligence: " + match.Adaptation.StoryLine, smallStyle);
        }

        private void DrawHealth(int x, int y, string label, FighterState fighter, Color color)
        {
            GUI.Label(new Rect(x, y, 430, 30), label + "  " + fighter.Health + "/" + fighter.MaxHealth, smallStyle);
            Box(new Rect(x, y + 25, 420, 9), new Color(0.2f, 0.2f, 0.23f));
            Box(new Rect(x, y + 25, 420f * fighter.Health / fighter.MaxHealth, 9), color);
        }

        private void DrawFighterLegacy(FighterState fighter, int index)
        {
            if (fighter.Health <= 0) return;
            float x = 40 + fighter.X / 20000f * 1200;
            float y = 580 - fighter.Y / 40f;
            Color color = index == 0 ? new Color(0.18f, 0.78f, 0.95f) :
                index == 1 ? new Color(0.92f, 0.76f, 0.25f) : new Color(0.95f, 0.29f, 0.36f);
            Box(new Rect(x - 23, y - 90, 46, 70), color);
            Box(new Rect(x - 17, y - 120, 34, 34), color);
            Box(new Rect(x - 29, y - 20, 22, 20), color);
            Box(new Rect(x + 7, y - 20, 22, 20), color);
            if (fighter.Guarding) Box(new Rect(x + fighter.Facing * 37 - 8, y - 105, 16, 85),
                new Color(0.55f, 0.88f, 1f));
            if (fighter.Attack != AttackType.None && fighter.AttackTicks >= 4 && fighter.AttackTicks <= 9)
                Box(new Rect(x + (fighter.Facing > 0 ? 20 : -90), y - 70, 70, 12),
                    new Color(1f, 0.92f, 0.55f));
        }

        private void DrawReportLegacy()
        {
            GUI.Label(new Rect(90, 100, 1100, 65), "THE RECORD PASSES ON", titleStyle);
            GUI.Label(new Rect(90, 190, 1100, 95),
                "The defeated Warden transmits both fighters' combat data to " +
                LevelSpec.Create(nextLevel).BossName + ".", bodyStyle);
            if ((nextLevel - 1) % 10 == 0)
                GUI.Label(new Rect(90, 248, 1100, 55),
                    "New act — " + LevelSpec.Create(nextLevel).ActStory, smallStyle);
            GUI.Label(new Rect(90, 320, 1100, 85), "\"" + nextProfile.StoryLine + "\"", bodyStyle);
            GUI.Label(new Rect(90, 430, 1100, 80),
                "Low guard: " + nextProfile.LowBlockWeight + "%  ·  Anti-air: " +
                nextProfile.AntiAirWeight + "%  ·  Spacing pressure: " +
                nextProfile.PressureWeight + "%  ·  Focus: " +
                (nextProfile.FocusPlayer == 0 ? "both" : "P" + nextProfile.FocusPlayer), bodyStyle);
            if (nextLevel == 5) GUI.Label(new Rect(90, 520, 1100, 40),
                "New skill unlocked: Tandem Strike", bodyStyle);
            if (nextLevel % 25 == 1 && nextLevel > 1)
                GUI.Label(new Rect(90, 550, 1100, 40),
                    "New weapon unlocked: " + LevelSpec.Create(nextLevel).WeaponName, bodyStyle);
            if (nextLevel % 20 == 1 && nextLevel > 1)
                GUI.Label(new Rect(90, 575, 1100, 40), "New combo branch unlocked", bodyStyle);
            if (playMode != PlayMode.Client && GUI.Button(new Rect(800, 560, 350, 45),
                "Skill tree (" + SkillTree.PointsAvailable(nextLevel, skillMask) + " points)"))
            {
                returnFromSkills = ScreenMode.Report;
                screen = ScreenMode.Skills;
            }
            GUI.Label(new Rect(90, 620, 1100, 40),
                playMode == PlayMode.Client ? "Waiting for host to begin Level " + nextLevel :
                "Press Enter for Level " + nextLevel, bodyStyle);
        }

        private void DrawSkills()
        {
            GUI.Label(new Rect(75, 35, 1080, 50), "CO-OP SKILL TREE", titleStyle);
            GUI.Label(new Rect(75, 93, 1080, 35),
                "Available points: " + SkillTree.PointsAvailable(nextLevel, skillMask) +
                "  ·  One point earned after every five completed levels", bodyStyle);
            for (int i = 0; i < SkillTree.OrderedNodes.Length; i++)
            {
                SkillNode node = SkillTree.OrderedNodes[i];
                int column = i / 7;
                int row = i % 7;
                int x = 50 + column * 410;
                int y = 155 + row * 67;
                string state = SkillTree.Has(skillMask, node.Bit) ? "OWNED" :
                    SkillTree.CanBuy(nextLevel, skillMask, node) ? "BUY" : "LOCKED";
                if (GUI.Button(new Rect(x, y, 380, 55), node.Name + "  [Level " +
                    node.RequiredLevel + "]  " + state + "\n" + node.Effect) &&
                    SkillTree.CanBuy(nextLevel, skillMask, node))
                {
                    skillMask = SkillTree.Buy(nextLevel, skillMask, node);
                    PlayerPrefs.SetInt("cf_skill_mask", skillMask);
                    PlayerPrefs.Save();
                }
            }
            if (GUI.Button(new Rect(75, 640, 200, 45), "Back")) screen = returnFromSkills;
        }

        private void DrawDefeatLegacy()
        {
            GUI.Label(new Rect(90, 160, 1100, 80), "BOTH FIGHTERS FALL", titleStyle);
            GUI.Label(new Rect(90, 260, 1100, 100),
                "The boss keeps the field. Reposition, guard, and watch your ally's attack lane.", bodyStyle);
            GUI.Label(new Rect(90, 430, 1100, 50),
                playMode == PlayMode.Client ? "Waiting for host to retry" :
                "Press R to retry Level " + match.Level, bodyStyle);
        }

        private void DrawCredits()
        {
            GUI.Label(new Rect(90, 160, 1100, 80), "THE VOICES ARE FREE", titleStyle);
            GUI.Label(new Rect(90, 270, 1100, 180),
                "Ten guardians stood between you and the truth. Together, you severed the command network and brought its people home. " +
                "The archive remembers them. It no longer decides their future.", bodyStyle);
            GUI.Label(new Rect(90, 540, 1100, 50), "Campaign complete", bodyStyle);
        }

        private void Box(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, pixel);
            GUI.color = old;
        }

        private void OnDestroy()
        {
            CloseNetwork();
            if (pixel != null) Destroy(pixel);
        }
    }
}
