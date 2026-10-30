using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CoopFighter.Game;
using UnityEditor;
using UnityEngine;

namespace CoopFighter.UnityGame.Editor
{
    public static class ScenarioValidation
    {
        private sealed class Scenario {
            public string Name; public PlayerPersona A, B; public int Delay; public bool Jitter, Retransmit;
            public Scenario(string name, PlayerPersona a, PlayerPersona b, int delay = 0, bool jitter = false, bool retransmit = false)
            { Name = name; A = a; B = b; Delay = delay; Jitter = jitter; Retransmit = retransmit; }
        }
        [Serializable] public sealed class Row {
            public string scenario; public int level, seed, ticks, p1Health, p2Health, bossHealth, allyHits;
            public string outcome; public bool deterministic, snapshot, rollback;
        }
        [Serializable] public sealed class Results { public string build = SessionProtocol.Version; public List<Row> runs = new List<Row>(); }
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }

        [MenuItem("Co-op Fighter/Run 20 Player Scenarios")]
        public static void Run()
        {
            Scenario[] cases = {
                new Scenario("01 First-time duo", PlayerPersona.Beginner, PlayerPersona.Beginner),
                new Scenario("02 Button mashers", PlayerPersona.Masher, PlayerPersona.Masher),
                new Scenario("03 Balanced duo", PlayerPersona.Balanced, PlayerPersona.Balanced),
                new Scenario("04 Defensive duo", PlayerPersona.Defender, PlayerPersona.Defender),
                new Scenario("05 Low-strike specialists", PlayerPersona.LowSpecialist, PlayerPersona.LowSpecialist),
                new Scenario("06 Aerial specialists", PlayerPersona.Jumper, PlayerPersona.Jumper),
                new Scenario("07 Opposite-side flankers", PlayerPersona.Flanker, PlayerPersona.Flanker),
                new Scenario("08 Tandem coordinators", PlayerPersona.TandemPartner, PlayerPersona.TandemPartner),
                new Scenario("09 Aggressive duo", PlayerPersona.Aggressor, PlayerPersona.Aggressor),
                new Scenario("10 Expert with novice", PlayerPersona.Flanker, PlayerPersona.Beginner),
                new Scenario("11 Guard and attacker", PlayerPersona.Defender, PlayerPersona.Aggressor),
                new Scenario("12 Ground and aerial", PlayerPersona.LowSpecialist, PlayerPersona.Jumper),
                new Scenario("13 Slow reactions", PlayerPersona.SlowReaction, PlayerPersona.SlowReaction),
                new Scenario("14 Idle partner", PlayerPersona.Balanced, PlayerPersona.Idle),
                new Scenario("15 Both idle", PlayerPersona.Idle, PlayerPersona.Idle),
                new Scenario("16 Ally-targeting player", PlayerPersona.Balanced, PlayerPersona.Griefer),
                new Scenario("17 Retreating duo", PlayerPersona.Runner, PlayerPersona.Runner),
                new Scenario("18 100ms one-way delay", PlayerPersona.Flanker, PlayerPersona.Flanker, 6),
                new Scenario("19 200ms plus jitter and reorder", PlayerPersona.Balanced, PlayerPersona.Jumper, 12, true),
                new Scenario("20 Retransmitted lost inputs", PlayerPersona.TandemPartner, PlayerPersona.Flanker, 8, true, true)
            };
            Results results = new Results();
            foreach (Scenario scenario in cases)
                foreach (int level in new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 })
                    for (int seed = 1; seed <= 3; seed++) results.runs.Add(RunOne(scenario, level, seed));
            CheckRegressions();
            Directory.CreateDirectory("TestResults");
            File.WriteAllText("TestResults/player-scenarios.json", JsonUtility.ToJson(results, true));
            StringBuilder report = new StringBuilder("# Shadow Warriors: 20 scenario simulation report\n\n");
            report.Append("600 simulated fights: 20 behavior/network categories × 10 guardians × 3 seeds. Each fight is capped at 180 simulated seconds. Input scripts use observed game state; these are reproducible bots, not human subjects. A timeout is recorded, not counted as victory.\n\n");
            report.Append("Network checks replay the same input trace through delayed/reordered/retransmitted delivery. They verify convergence, not subjective latency feel. Live two-process transport is a separate test.\n\n");
            report.Append("| Scenario | Wins | Losses | Timeouts | Average seconds | Ally hits | Replay / snapshot / rollback |\n|---|---:|---:|---:|---:|---:|---|\n");
            foreach (Scenario scenario in cases) {
                int wins = 0, losses = 0, timeouts = 0, ticks = 0, allies = 0, count = 0;
                foreach (Row row in results.runs) if (row.scenario == scenario.Name) {
                    count++; ticks += row.ticks; allies += row.allyHits;
                    if (row.outcome == "Victory") wins++; else if (row.outcome == "Defeat") losses++; else timeouts++;
                }
                report.AppendFormat("| {0} | {1} | {2} | {3} | {4:F1} | {5} | PASS |\n", scenario.Name, wins, losses, timeouts, ticks / (60.0 * count), allies);
            }
            report.Append("\nRegression checks: all 100 level definitions; fifth weapon reachable; frontal/rear guard; nonlethal ally hits; bounded prediction; version mismatch rejection; adaptive low/air transfer; serialized continuation; skill prerequisite/budget validity. All passed.\n");
            File.WriteAllText("TestResults/PLAYER-SCENARIOS.md", report.ToString());
            Results sweep = new Results();
            Scenario campaign = new Scenario("All-level coordinated-player sweep", PlayerPersona.TandemPartner, PlayerPersona.Flanker, 6, true);
            int sweepWins = 0;
            for (int level = 1; level <= 100; level++) {
                Row row = RunOne(campaign, level, 7); sweep.runs.Add(row);
                if (row.outcome == "Victory") sweepWins++;
            }
            File.WriteAllText("TestResults/all-level-sweep.json", JsonUtility.ToJson(sweep, true));
            File.AppendAllText("TestResults/PLAYER-SCENARIOS.md", "\nAdditional all-level sweep: " + sweepWins + "/100 wins with the coordinated bot pair. Each level starts with a fresh loadout and a synthetic prior adaptation profile; this is level coverage, not a claim that a human completed the campaign. Every run also passed deterministic replay, JSON restore, and delayed-input convergence.\n");
            Debug.Log("SCENARIO_SUITE_PASS: " + results.runs.Count + " fights; 20 categories; regression checks passed.");
        }

        private static Row RunOne(Scenario scenario, int level, int seed)
        {
            int mask = 0;
            foreach (SkillNode node in SkillTree.OrderedNodes)
                if (SkillTree.CanBuy(level, mask, node)) mask = SkillTree.Buy(level, mask, node);
            CounterProfile profile = level == 1 ? null : CounterProfile.FromBattle(level - 1,
                new BattleTelemetry { Attempts = new[] { 20, 20 }, Low = new[] { 15, 0 }, Aerial = new[] { 0, 15 } });
            if (profile != null) profile.TargetLevel = level; // Synthetic fixture, not campaign routing.
            MatchState initial = Simulation.Start(level, profile, seed * 7919, mask);
            MatchState baseline = initial.Copy(), replay = initial.Copy();
            List<Buttons> a = new List<Buttons> { Buttons.None }, b = new List<Buttons> { Buttons.None };
            while (baseline.Result == MatchResult.Fighting && baseline.Tick < 10800) {
                Buttons p1 = PlayerBot.Read(baseline, 0, scenario.A, seed);
                Buttons p2 = PlayerBot.Read(baseline, 1, scenario.B, seed);
                a.Add(p1); b.Add(p2);
                Simulation.Step(baseline, p1, p2); Simulation.Step(replay, p1, p2);
                Require(baseline.Hash() == replay.Hash(), "Determinism: " + scenario.Name);
                foreach (FighterState f in baseline.Fighters)
                    Require(f.Health >= 0 && f.Health <= f.MaxHealth && f.X >= 500 && f.X <= 19500 && f.Y >= 0, "State invariant");
                if (baseline.Tick % 211 == 0) {
                    replay = JsonUtility.FromJson<MatchState>(JsonUtility.ToJson(replay));
                    Require(baseline.Hash() == replay.Hash(), "Snapshot restore");
                }
            }
            RollbackHost host = new RollbackHost(initial);
            int n = baseline.Tick;
            var delivery = new SortedDictionary<int, List<int>>();
            for (int t = 1; t <= n; t++) {
                int lag = scenario.Delay + (scenario.Jitter ? (t * 13 + seed) % 9 : 0);
                if (scenario.Retransmit && t % 13 == 0) lag += 20;
                int time = t + lag;
                if (!delivery.ContainsKey(time)) delivery[time] = new List<int>();
                delivery[time].Add(t);
            }
            for (int time = 1; time <= n + 200; time++) {
                List<int> arrived;
                if (delivery.TryGetValue(time, out arrived)) foreach (int t in arrived) {
                    host.SubmitRemote(t, b[t]);
                    if (scenario.Retransmit) host.SubmitRemote(t, b[t]);
                }
                if (host.State.Tick < n) host.Step(a[host.State.Tick + 1]);
            }
            Require(host.State.Hash() == baseline.Hash(), "Rollback: " + scenario.Name + " level " + level);
            return new Row { scenario = scenario.Name, level = level, seed = seed, ticks = n,
                outcome = baseline.Result == MatchResult.Fighting ? "Timeout" : baseline.Result.ToString(),
                p1Health = baseline.Fighters[0].Health, p2Health = baseline.Fighters[1].Health,
                bossHealth = baseline.Fighters[2].Health, allyHits = baseline.Telemetry.FriendlyHits,
                deterministic = true, snapshot = true, rollback = true };
        }

        private static void CheckRegressions()
        {
            for (int level = 1; level <= 100; level++) Require(LevelSpec.Create(level).BossHealth > 0, "Level configuration");
            Require(LevelSpec.Create(81).WeaponTier == 4, "Final weapon unreachable");
            Require(!SessionProtocol.Compatible("wrong-version"), "Version mismatch accepted");
            RollbackHost stalled = new RollbackHost(Simulation.Start(1, null, 1));
            for (int i = 0; i < 1000; i++) stalled.Step(Buttons.None);
            Require(stalled.State.Tick == RollbackHost.MaxPredictionTicks, "Unbounded prediction");
            var profile = CounterProfile.FromBattle(1, new BattleTelemetry { Attempts = new[] { 10, 10 }, Low = new[] { 8, 0 }, Aerial = new[] { 0, 8 } });
            Require(profile.LowBlockWeight == 55 && profile.AntiAirWeight == 55, "Adaptation thresholds");
            MatchState ally = Simulation.Start(1, null, 1);
            ally.Fighters[0].X = 4000; ally.Fighters[1].X = 5100; ally.Fighters[1].Health = 1;
            for (int t = 0; t < 10; t++) Simulation.Step(ally, t == 0 ? Buttons.High : Buttons.None, Buttons.None);
            Require(ally.Fighters[1].Health == 1 && ally.Telemetry.FriendlyHits == 1, "Ally protection / hit deduplication");
            int front = GuardDamage(-1), back = GuardDamage(1);
            Require(front < back, "Directional guarding");
            int mask = 0;
            foreach (SkillNode node in SkillTree.OrderedNodes) if (SkillTree.CanBuy(100, mask, node)) mask = SkillTree.Buy(100, mask, node);
            Require(SkillTree.PointsAvailable(100, mask) >= 0, "Skill budget");
        }
        private static int GuardDamage(int facing) {
            MatchState s = Simulation.Start(1, null, 2);
            s.Fighters[0].X = 4000; s.Fighters[1].X = 5100; s.Fighters[1].Facing = facing;
            for (int t = 0; t < 10; t++) Simulation.Step(s, t == 0 ? Buttons.High : Buttons.None, Buttons.Guard);
            return s.Fighters[1].MaxHealth - s.Fighters[1].Health;
        }
    }
}
