using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        private bool automatedTest;
        private string testOutput;
        private int testResults;
        private float testFinishAt;
        private readonly HashSet<int> loggedEncounters = new HashSet<int>();
        private readonly List<TestResult> completedTests = new List<TestResult>();
        private string localAddresses = "";
        private string webRoom = "", requestedRoom;
        private bool browserBot;
        private float lastBrowserPublish;
#if UNITY_WEBGL && !UNITY_EDITOR
        private const bool IsWeb = true;
        [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void BB_Status(string json);
        [System.Runtime.InteropServices.DllImport("__Internal")] private static extern int BB_Touch();
#else
        private const bool IsWeb = false;
#endif

        private void Start()
        {
            touchControls = Input.touchSupported;
#if UNITY_WEBGL && !UNITY_EDITOR
            touchControls = BB_Touch() != 0;
#endif
#if !UNITY_WEBGL || UNITY_EDITOR
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    localAddresses += (localAddresses.Length == 0 ? "" : "  /  ") + ip;
#else
            joinAddress = "";
#endif
            string[] args = Environment.GetCommandLineArgs();
            string join = null; bool host = false;
            foreach (string arg in args) {
                if (arg == "--host") host = true;
                if (arg.StartsWith("--join=")) join = arg.Substring(7);
                if (arg.StartsWith("--test-output=")) { automatedTest = true; testOutput = arg.Substring(14); }
            }
            if (automatedTest) { nextLevel = 5; skillMask = 0; nextProfile = null; }
            if (host) StartHost();
            else if (join != null) { joinAddress = join; JoinHost(); }
        }

        private void UpdateSession()
        {
            UpdateAudio();
            if (Time.realtimeSinceStartup - lastBrowserPublish > 0.5f) {
                lastBrowserPublish = Time.realtimeSinceStartup; PublishBrowserStatus();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && screen != ScreenMode.Menu) {
                ReturnToHub(); status = "Returned to camp. Your progress is saved on this device."; return;
            }
            if (network != null && network.Connected) {
                float now = Time.realtimeSinceStartup;
                if (!helloSent) {
                    helloSent = true; lastPacketTime = now;
                    Send(new NetworkEnvelope { kind = "hello", level = nextLevel });
                }
                if (now - lastPingTime > 1f) { lastPingTime = now; Send(new NetworkEnvelope { kind = "ping" }); PublishBrowserStatus(); }
                if (now - lastPacketTime > 12f) {
                    CloseNetwork(); screen = ScreenMode.Menu;
                    status = "Your partner disconnected. Host a new room to resume the saved level."; return;
                }
                bool between = screen == ScreenMode.Connecting || screen == ScreenMode.Report || screen == ScreenMode.Defeat;
                if ((automatedTest || browserBot) && between && peerVerified && testResults < 3) {
                    if (playMode == PlayMode.Client && !localReady) ReadyGuest();
                    if (playMode == PlayMode.Host) localReady = true;
                }
                if (playMode == PlayMode.Host && between && localReady && peerVerified && peerReady)
                    StartLevel(screen == ScreenMode.Defeat ? match.Level : nextLevel);
            }
            if (automatedTest && testResults >= 3 && Time.realtimeSinceStartup > testFinishAt) Application.Quit();
            UpdatePresentation();
        }

        // Exposed to the page for room links and explicit automated browser tests.
        public void ConfigureBrowser(string options)
        {
            string[] parts = options.Split('|');
            if (parts.Length < 2) return;
            requestedRoom = parts[1].ToUpperInvariant();
            browserBot = parts.Length > 2 && parts[2] == "bot";
            if (browserBot) { nextLevel = 5; skillMask = 0; nextProfile = null; }
            if (parts[0] == "host") StartHost();
            else { joinAddress = requestedRoom; JoinHost(); }
        }
        public void ReceiveWebLine(string line) {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (network != null) network.Inject(line);
#endif
        }
        [Serializable] private class BrowserStatus {
            public string screen, role, room, result; public int tick, level, encounter, hp1, hp2, boss;
            public uint hash; public bool verified;
            public int p1X, p2X, p1Y, p2Y, p1Attacks, p2Attacks;
            public TestResult[] completed;
        }
        private void PublishBrowserStatus() {
#if UNITY_WEBGL && !UNITY_EDITOR
            BB_Status(JsonUtility.ToJson(new BrowserStatus { screen = screen.ToString(), role = playMode.ToString(), room = webRoom,
                encounter = encounter, verified = peerVerified, tick = match == null ? 0 : match.Tick,
                level = match == null ? nextLevel : match.Level, result = match == null ? "" : match.Result.ToString(),
                hash = match == null ? 0 : match.Hash(), hp1 = match == null ? 0 : match.Fighters[0].Health,
                hp2 = match == null ? 0 : match.Fighters[1].Health, boss = match == null ? 0 : match.Fighters[2].Health,
                p1X = match == null ? 0 : match.Fighters[0].X, p2X = match == null ? 0 : match.Fighters[1].X,
                p1Y = match == null ? 0 : match.Fighters[0].Y, p2Y = match == null ? 0 : match.Fighters[1].Y,
                p1Attacks = match == null ? 0 : match.Telemetry.Attempts[0], p2Attacks = match == null ? 0 : match.Telemetry.Attempts[1],
                completed = completedTests.ToArray() }));
#endif
        }

        private void ReadyGuest()
        {
            if (!peerVerified) return;
            localReady = true;
            Send(new NetworkEnvelope { kind = "ready" });
        }

        [Serializable] private class TestResult {
            public string role, result; public int encounter, level, tick; public uint hash;
        }
        private void LogTestResult(string result, MatchState final)
        {
            if ((!automatedTest && !browserBot) || final == null || !loggedEncounters.Add(encounter)) return;
            string role = playMode == PlayMode.Host ? "host" : "guest";
            TestResult record = new TestResult {
                role = role, result = result, encounter = encounter, level = final.Level,
                tick = final.Tick, hash = final.Hash() };
            completedTests.Add(record);
            if (automatedTest) {
                Directory.CreateDirectory(testOutput);
                File.AppendAllText(Path.Combine(testOutput, role + ".jsonl"), JsonUtility.ToJson(record) + Environment.NewLine);
            }
            testResults++;
            // Exercise a same-level restart followed by normal next-level progression.
            if (playMode == PlayMode.Host && testResults == 1 && result == "report") nextLevel = final.Level;
            testFinishAt = Time.realtimeSinceStartup + 3f;
            PublishBrowserStatus();
        }
    }
}
