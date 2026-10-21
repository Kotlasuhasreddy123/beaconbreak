using System;

namespace CoopFighter.Game
{
    internal static class CoreChecks
    {
        private static void Require(bool value, string message)
        {
            if (!value) throw new Exception(message);
        }

        private static void Main()
        {
            for (int level = 1; level <= 100; level++)
            {
                LevelSpec spec = LevelSpec.Create(level);
                Require(spec.Level == level && spec.BossHealth > 0 && spec.BossDamage > 0,
                    "Invalid level " + level);
            }

            MatchState first = Simulation.Start(1, null, 12345);
            MatchState second = first.Copy();
            for (int tick = 0; tick < 1000; tick++)
            {
                Buttons p1 = tick % 53 == 0 ? Buttons.High :
                    tick % 29 < 15 ? Buttons.Right : Buttons.Left;
                Buttons p2 = tick % 67 == 0 ? Buttons.Low :
                    tick % 31 < 10 ? Buttons.Jump | Buttons.Right : Buttons.Left;
                Simulation.Step(first, p1, p2);
                Simulation.Step(second, p1, p2);
                Require(first.Hash() == second.Hash(), "Replay diverged at tick " + tick);
            }

            MatchState friendly = Simulation.Start(1, null, 1);
            friendly.Fighters[0].X = 4000;
            friendly.Fighters[1].X = 5100;
            int startHealth = friendly.Fighters[1].Health;
            Simulation.Step(friendly, Buttons.High, Buttons.None);
            for (int i = 0; i < 10; i++) Simulation.Step(friendly, Buttons.None, Buttons.None);
            Require(friendly.Fighters[1].Health < startHealth, "Friendly hit was not detected.");
            Require(friendly.Telemetry.FriendlyHits == 1, "Friendly hit was not deduplicated.");

            BattleTelemetry habits = new BattleTelemetry();
            habits.Attempts[0] = 10; habits.Low[0] = 8;
            habits.Attempts[1] = 10; habits.Aerial[1] = 8;
            habits.BossDamage[0] = 80; habits.BossDamage[1] = 20;
            habits.FriendlyHits = 6;
            CounterProfile profile = CounterProfile.FromBattle(1, habits);
            Require(profile.TargetLevel == 2 && profile.LowBlockWeight == 55 &&
                profile.AntiAirWeight == 55 && profile.PressureWeight == 45 &&
                profile.FocusPlayer == 1,
                "Boss adaptation was incorrect.");
            Require(Simulation.Start(2, profile, 1).Adaptation.TargetLevel == 2,
                "Transferred adaptation was not loaded.");

            Require(SkillTree.PointsAvailable(6, 0) == 1 &&
                SkillTree.CanBuy(6, 0, SkillTree.Nodes[0]), "First skill point unavailable.");
            int mask = SkillTree.Buy(6, 0, SkillTree.Nodes[0]);
            Require(SkillTree.Has(mask, 0) && SkillTree.PointsAvailable(6, mask) == 0,
                "Skill purchase was not accounted for.");
            Require(!SkillTree.CanBuy(6, mask, SkillTree.Nodes[5]),
                "Later skill was unlocked too early.");

            MatchState baseline = Simulation.Start(3, null, 4711);
            RollbackHost rollback = new RollbackHost(Simulation.Start(3, null, 4711));
            Buttons[] remoteInputs = new Buttons[301];
            for (int tick = 1; tick <= 300; tick++)
            {
                Buttons local = tick % 40 < 20 ? Buttons.Right : Buttons.Left;
                Buttons remote = tick % 45 < 20 ? Buttons.Left : Buttons.Right;
                if (tick % 57 == 0) remote |= Buttons.High;
                remoteInputs[tick] = remote;
                Simulation.Step(baseline, local, remote);
                rollback.Step(local);
                if (tick > 5) rollback.SubmitRemote(tick - 5, remoteInputs[tick - 5]);
            }
            for (int tick = 296; tick <= 300; tick++)
                rollback.SubmitRemote(tick, remoteInputs[tick]);
            Require(rollback.State.Hash() == baseline.Hash(),
                "Delayed input rollback diverged from no-latency play.");

            Console.WriteLine("PASS: 100 levels, deterministic replay, friendly fire, adaptation transfer, skill tree, delayed-input rollback");
        }
    }
}
