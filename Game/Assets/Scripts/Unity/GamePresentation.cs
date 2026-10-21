using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        private Texture2D arena, circle;
        private float impactUntil;
        private int effectTick = -1, effectEncounter, impactTarget, impactDamage;
        private bool impactFriendly, touchControls;
        private GUIStyle heroStyle, buttonStyle;
        private readonly Color gold = new Color(0.94f, 0.76f, 0.44f);
        private readonly Color ink = new Color(0.025f, 0.033f, 0.038f);
        private readonly Rect[] touchRects = {
            new Rect(20, 595, 90, 100), new Rect(120, 595, 90, 100), new Rect(230, 595, 90, 100),
            new Rect(845, 595, 90, 100), new Rect(945, 595, 90, 100), new Rect(1045, 595, 90, 100), new Rect(1145, 595, 115, 100)
        };
        private readonly Buttons[] touchFlags = { Buttons.Left, Buttons.Right, Buttons.Jump, Buttons.Low, Buttons.High, Buttons.Guard, Buttons.Tandem };
        private void UpdatePresentation()
        {
            if (Input.touchCount > 0) touchControls = true;
            if (Input.GetKeyDown(KeyCode.T)) touchControls = !touchControls;
            if (match != null && match.LastHitDamage > 0 && (effectEncounter != encounter || match.Tick > effectTick)) {
                effectEncounter = encounter; effectTick = match.Tick; impactUntil = Time.unscaledTime + 0.3f;
                impactTarget = match.LastHitTarget; impactDamage = match.LastHitDamage; impactFriendly = match.LastHitFriendly;
                PlayImpact(impactFriendly, impactDamage);
            }
        }
        private Buttons TouchButtons()
        {
            Buttons result = Buttons.None;
            if (!touchControls || screen != ScreenMode.Fight) return result;
            for (int t = 0; t < Input.touchCount; t++) {
                Touch touch = Input.GetTouch(t);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) continue;
                Vector2 point = new Vector2(touch.position.x * 1280f / Screen.width, (Screen.height - touch.position.y) * 720f / Screen.height);
                for (int i = 0; i < touchRects.Length; i++) if (touchRects[i].Contains(point)) result |= touchFlags[i];
            }
            if (Input.touchCount == 0 && Input.GetMouseButton(0)) {
                Vector2 point = new Vector2(Input.mousePosition.x * 1280f / Screen.width, (Screen.height - Input.mousePosition.y) * 720f / Screen.height);
                for (int i = 0; i < touchRects.Length; i++) if (touchRects[i].Contains(point)) result |= touchFlags[i];
            }
            return result;
        }
        private void PrepareArt()
        {
            if (circle != null) return;
            arena = Resources.Load<Texture2D>("Arena");
            circle = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                circle.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(32 - d)));
            }
            circle.Apply();
            heroStyle = new GUIStyle(titleStyle) { fontSize = 58 };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            GUI.skin.button.fontSize = 15; GUI.skin.textField.fontSize = 24;
        }
        private void DrawBackground()
        {
            PrepareArt();
            if (arena != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), arena, ScaleMode.StretchToFill);
            else Box(new Rect(0, 0, 1280, 720), ink);
            if (screen != ScreenMode.Fight) Box(new Rect(0, 0, 1280, 720), new Color(0.015f, 0.025f, 0.03f, 0.55f));
            else {
                Box(new Rect(0, 0, 1280, 178), new Color(0.015f, 0.025f, 0.03f, 0.85f));
                Box(new Rect(0, 598, 1280, 122), new Color(0.015f, 0.025f, 0.03f, 0.86f));
            }
        }
        private bool ActionButton(Rect rect, string text, bool primary = false)
        {
            Color old = GUI.backgroundColor; GUI.backgroundColor = primary ? gold : new Color(0.23f, 0.35f, 0.37f);
            bool clicked = GUI.Button(rect, text, buttonStyle); GUI.backgroundColor = old; return clicked;
        }
        private void Panel(Rect rect) {
            Box(rect, new Color(0.025f, 0.04f, 0.045f, 0.94f)); Box(new Rect(rect.x, rect.y, rect.width, 2), gold);
        }
        private void DrawMenu()
        {
            Panel(new Rect(55, 65, 675, 595));
            GUI.Label(new Rect(85, 90, 600, 35), "T W O   F I G H T E R S .   O N E   L A S T   L I G H T .", smallStyle);
            GUI.Label(new Rect(80, 132, 650, 90), "BEACONBREAK", heroStyle);
            GUI.Label(new Rect(85, 228, 590, 80), "Fight together. Learn their tells. Break the record.\n100 encounters against bosses that remember.", bodyStyle);
            GUI.Label(new Rect(85, 325, 590, 35), "CAMPAIGN / LEVEL " + nextLevel.ToString("00") + "     •     2 DEVICES REQUIRED", smallStyle);
            if (ActionButton(new Rect(85, 375, 280, 58), "CREATE ROOM", true)) StartHost();
            if (ActionButton(new Rect(385, 375, 280, 58), "JOIN PARTNER")) JoinHost();
            GUI.Label(new Rect(85, 452, 135, 40), IsWeb ? "ROOM CODE" : "HOST ADDRESS", smallStyle);
            joinAddress = GUI.TextField(new Rect(235, 450, 430, 42), joinAddress);
            if (ActionButton(new Rect(85, 515, 280, 45), "TEAM SKILLS")) { returnFromSkills = screen; screen = ScreenMode.Skills; }
            if (ActionButton(new Rect(385, 515, 280, 45), touchControls ? "TOUCH: ON" : "TOUCH: OFF")) touchControls = !touchControls;
            GUI.Label(new Rect(85, 585, 590, 55), status.Length > 0 ? status : "Each player opens the game on their own device.\nCreate a room, then share its code with your partner.", smallStyle);
            DrawFighter(new FighterState { X = 14100, Facing = 1, Health = 100 }, 0);
            DrawFighter(new FighterState { X = 17400, Facing = -1, Health = 100, Guarding = true }, 1);
            GUI.Label(new Rect(790, 625, 430, 40), "CO-OP MARTIAL ARTS CAMPAIGN", smallStyle);
        }
        private void DrawConnecting()
        {
            Panel(new Rect(160, 125, 960, 480));
            GUI.Label(new Rect(200, 160, 880, 50), playMode == PlayMode.Host ? "YOUR PARTNER'S PLACE IS READY" : "JOINING THE FIGHT", titleStyle);
            GUI.Label(new Rect(200, 240, 880, 80), IsWeb ? "ROOM  " + webRoom : "HOST  " + localAddresses, heroStyle);
            GUI.Label(new Rect(200, 345, 880, 85), peerVerified ? "Two devices connected. Get ready to enter the arena." : status, bodyStyle);
            if (playMode == PlayMode.Client && peerVerified && !localReady && ActionButton(new Rect(200, 445, 350, 55), "I'M READY", true)) ReadyGuest();
            if (playMode == PlayMode.Host || localReady) GUI.Label(new Rect(200, 455, 750, 40), "Waiting for your partner to press I'M READY…", bodyStyle);
            if (ActionButton(new Rect(800, 525, 270, 45), "LEAVE ROOM")) { CloseNetwork(); screen = ScreenMode.Menu; }
        }
        private void DrawFight()
        {
            LevelSpec spec = LevelSpec.Create(match.Level);
            GUI.Label(new Rect(30, 12, 850, 45), spec.ActName.ToUpper() + " / " + match.Level.ToString("00"), titleStyle);
            GUI.Label(new Rect(930, 20, 320, 30), "YOU ARE " + (playMode == PlayMode.Host ? "P1 • CYAN" : "P2 • AMBER"), smallStyle);
            DrawHealth(30, 65, "P1 / THE SCOUT", match.Fighters[0], new Color(0.22f, 0.85f, 0.92f));
            DrawHealth(30, 110, "P2 / THE KEEPER", match.Fighters[1], gold);
            DrawHealth(800, 65, spec.Style.ToString().ToUpper() + " / WARDEN", match.Fighters[2], new Color(0.94f, 0.35f, 0.25f));
            GUI.Label(new Rect(800, 115, 430, 32), (180 - match.Tick / 60) + "s LEFT • " + spec.WeaponName.ToUpper(), smallStyle);
            for (int i = 0; i < 3; i++) DrawFighter(match.Fighters[i], i);
            if (match.Fighters[2].AttackTicks > 9) {
                float x = 40 + match.Fighters[2].X * 0.06f;
                GUI.Label(new Rect(x - 75, 300 - match.Fighters[2].Y / 40f, 240, 40), "! " + (match.Fighters[2].Attack == AttackType.BossAntiAir ? "ANTI-AIR" : "STRIKE"), bodyStyle);
            }
            if (Time.unscaledTime < impactUntil && impactTarget > 0) {
                FighterState target = match.Fighters[impactTarget - 1]; float x = 40 + target.X * 0.06f, y = 435 - target.Y / 40f;
                GUI.Label(new Rect(x - 40, y - (0.3f - impactUntil + Time.unscaledTime) * 90, 250, 40), (impactFriendly ? "ALLY −" : "−") + impactDamage, titleStyle);
                for (int i = 0; i < 7; i++) { float angle = i * 0.9f;
                    Line(new Vector2(x + Mathf.Cos(angle) * 25, y + 40 + Mathf.Sin(angle) * 25), new Vector2(x + Mathf.Cos(angle) * 50, y + 40 + Mathf.Sin(angle) * 50), 3, gold);
                }
            }
            if (touchControls) {
                string[] labels = { "<", ">", "JUMP", "LOW", "HIGH", "GUARD", "TANDEM" };
                for (int i = 0; i < touchRects.Length; i++) { Box(touchRects[i], new Color(0.13f, 0.21f, 0.24f, 0.95f)); GUI.Label(new Rect(touchRects[i].x + 12, 627, touchRects[i].width, 40), labels[i], smallStyle); }
            } else {
                GUI.Label(new Rect(30, 620, 1200, 32), "A / D MOVE    SPACE JUMP    J LOW    K HIGH    L GUARD    U TANDEM    T TOUCH    M MUTE    ESC LEAVE", smallStyle);
                GUI.Label(new Rect(30, 662, 1220, 35), "TANDEM " + (!spec.TandemUnlocked ? "UNLOCKS AT LEVEL 5" : match.TandemCooldown == 0 ? "READY — PRESS U TOGETHER NEAR THE BOSS" : "RECHARGING") + " • ALLY HITS " + match.Telemetry.FriendlyHits, smallStyle);
            }
            if (rollback != null && rollback.WaitingForRemote) GUI.Label(new Rect(390, 230, 700, 50), "Waiting for partner's connection…", bodyStyle);
            if (match.Tick < 300) GUI.Label(new Rect(290, 195, 900, 80), "Guard the wind-up. Attack from opposite sides.\nYour weapons can hit your partner.", bodyStyle);
            if (ActionButton(new Rect(1145, 180, 110, 34), "LEAVE")) { CloseNetwork(); screen = ScreenMode.Menu; }
        }
        private void DrawReport()
        {
            Panel(new Rect(130, 90, 1020, 550));
            GUI.Label(new Rect(175, 120, 930, 55), "THE RECORD PASSES ON", titleStyle);
            GUI.Label(new Rect(175, 200, 900, 75), "The defeated Warden sends your combat record ahead.\nThe next guardian is already preparing.", bodyStyle);
            Line(new Vector2(235, 320), new Vector2(1030, 320), 3, gold);
            Disc(new Vector2(Mathf.Lerp(235, 1030, (Time.unscaledTime * 0.3f) % 1), 320), 10, gold);
            if (nextProfile != null) {
                GUI.Label(new Rect(175, 365, 920, 80), "“" + nextProfile.StoryLine + "”", bodyStyle);
                GUI.Label(new Rect(175, 450, 920, 40), "LOW GUARD " + nextProfile.LowBlockWeight + "% / ANTI-AIR " + nextProfile.AntiAirWeight + "% / PRESSURE " + nextProfile.PressureWeight + "%", smallStyle);
            }
            DrawReady(new Rect(175, 545, 420, 55), "READY FOR LEVEL " + nextLevel);
            if (playMode == PlayMode.Host && ActionButton(new Rect(650, 545, 420, 55), "TEAM SKILLS")) { returnFromSkills = screen; screen = ScreenMode.Skills; }
        }
        private void DrawDefeat()
        {
            Panel(new Rect(180, 145, 920, 435));
            GUI.Label(new Rect(225, 185, 820, 75), "RISE TOGETHER", heroStyle);
            GUI.Label(new Rect(225, 285, 820, 100), "The Warden held the field. Watch the wind-up, guard facing the enemy, and attack from opposite sides.", bodyStyle);
            DrawReady(new Rect(225, 450, 480, 60), "READY TO RETRY");
        }
        private void DrawReady(Rect rect, string label) {
            if (localReady) GUI.Label(rect, "READY / Waiting for partner", bodyStyle);
            else if (ActionButton(rect, label, true)) { if (playMode == PlayMode.Client) ReadyGuest(); else localReady = true; }
        }
        private void DrawFighter(FighterState f, int index)
        {
            float x = 40 + f.X * 0.06f, ground = 570, y = ground - f.Y / 40f;
            Color accent = index == 0 ? new Color(0.18f, 0.85f, 0.95f) : index == 1 ? gold : new Color(0.95f, 0.3f, 0.2f);
            Color old = GUI.color; GUI.color = new Color(0, 0, 0, 0.4f); GUI.DrawTexture(new Rect(x - 52, ground - 6, 104, 18), circle); GUI.color = old;
            if (f.Health <= 0) { Line(new Vector2(x - 38, ground - 10), new Vector2(x + 42, ground - 12), 24, ink); Disc(new Vector2(x + 52, ground - 15), 15, ink); return; }
            float size = index == 2 ? 1.18f : 1f;
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = previous * Matrix4x4.TRS(new Vector3(x, y, 0), Quaternion.identity, new Vector3(f.Facing * size, size, 1));
            bool moving = (f.PreviousButtons & (Buttons.Left | Buttons.Right)) != 0 && !f.Guarding;
            float stride = moving ? Mathf.Sin((match == null ? Time.unscaledTime * 60 : match.Tick) * 0.24f) * 22 : 7;
            float breath = Mathf.Sin(Time.unscaledTime * 2.5f + index) * 2;
            bool attacking = f.Attack != AttackType.None;
            float extension = attacking ? Mathf.Sin(Mathf.Clamp01((14 - f.AttackTicks) / 10f) * Mathf.PI) : 0;
            float crouch = f.Attack == AttackType.Low ? 22 * extension : 0;
            Vector2 hips = new Vector2(0, -65 + crouch), chest = new Vector2(6 + extension * 12, -117 + crouch + breath), head = chest + new Vector2(3, -30);
            Vector2 kneeA = new Vector2(-18 - stride, -34), kneeB = new Vector2(24 + stride, -30), footA = new Vector2(-28 - stride, -4), footB = new Vector2(35 + stride, -4);
            if (f.Y > 0) { kneeA = new Vector2(-25, -45); footA = new Vector2(-45, -28); kneeB = new Vector2(40, -70); footB = new Vector2(74, -40); }
            if (f.Attack == AttackType.Low) { kneeB = new Vector2(45, -20); footB = new Vector2(42 + extension * 72, -8); }
            Limb(hips, kneeA, footA, 21, ink); Limb(hips, kneeB, footB, 22, ink); Line(hips, chest, 42, ink);
            Disc(head, 19, ink); Line(head + new Vector2(-4, 15), chest, 18, ink);
            Limb(chest, chest + new Vector2(-28, 30), chest + new Vector2(0, 45), 15, ink);
            Vector2 arm = chest + new Vector2(28, 22), hand = chest + new Vector2(48, 5);
            if (f.Guarding) { arm = chest + new Vector2(30, 0); hand = chest + new Vector2(20, -30); }
            else if (attacking) { arm = chest + new Vector2(30 + extension * 30, 10); hand = chest + new Vector2(45 + extension * 65, -8 + extension * 20); }
            Limb(chest, arm, hand, 18, ink);
            Line(hips + new Vector2(-18, -4), hips + new Vector2(19, -4), 7, accent);
            Line(hips, hips + new Vector2(-32 - stride * 0.4f, 17), 8, accent);
            Line(head + new Vector2(-14, -4), head + new Vector2(16, -4), 5, accent);
            Line(hand - new Vector2(5, 0), hand + new Vector2(5, 0), 7, accent);
            if (index == 2) { Line(head + new Vector2(-12, -8), head + new Vector2(-28, -30), 8, ink); Line(head + new Vector2(10, -10), head + new Vector2(18, -35), 8, ink); }
            int tier = match == null ? 0 : LevelSpec.Create(match.Level).WeaponTier;
            Vector2 tip = hand + new Vector2(attacking ? 38 + tier * 7 : 12, attacking ? -10 : -60 - tier * 9);
            Line(hand, tip, tier == 1 ? 5 : 8, new Color(0.62f, 0.65f, 0.58f)); Line(hand + new Vector2(-8, -4), hand + new Vector2(10, 4), 5, accent);
            if (f.StunTicks > 0) Disc(head + new Vector2(0, -27), 4, gold);
            GUI.matrix = previous;
            if (match != null) GUI.Label(new Rect(x - 27, ground + 12, 120, 25), index < 2 ? "P" + (index + 1) : "WARDEN", smallStyle);
        }
        private void Limb(Vector2 a, Vector2 b, Vector2 c, float width, Color color) { Line(a, b, width, color); Line(b, c, width - 3, color); }
        private void Disc(Vector2 center, float radius, Color color) { Color old = GUI.color; GUI.color = color; GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2), circle); GUI.color = old; }
        private void Line(Vector2 a, Vector2 b, float width, Color color) {
            Matrix4x4 previous = GUI.matrix;
            // Compose in fighter-local coordinates. RotateAroundPivot uses screen
            // coordinates and breaks mirrored/scaled skeletons on the web canvas.
            GUI.matrix = previous * Matrix4x4.TRS(new Vector3(a.x, a.y, 0),
                Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg), Vector3.one);
            Box(new Rect(0, -width / 2, Vector2.Distance(a, b), width), color);
            GUI.matrix = previous; Disc(a, width / 2, color); Disc(b, width / 2, color);
        }
    }
}
