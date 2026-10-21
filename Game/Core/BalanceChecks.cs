using System;
using CoopFighter.Game;

internal static class BalanceChecks
{
    private static Buttons FighterInput(MatchState state, int index)
    {
        FighterState player = state.Fighters[index];
        FighterState boss = state.Fighters[2];
        if (player.Health <= 0) return Buttons.None;
        int dx = boss.X - player.X;
        if (Math.Abs(dx) > 1250) return dx > 0 ? Buttons.Right : Buttons.Left;
        // A modest test bot guards telegraphed strikes and attacks every 20
        // ticks. This is a balance smoke check, not a game-playing benchmark.
        if (boss.AttackTicks > 9) return Buttons.Guard;
        return state.Tick % 20 == (index == 0 ? 0 : 10) ? Buttons.High : Buttons.None;
    }

    private static void Main()
    {
        int[] levels = { 1, 25, 50, 75, 100 };
        foreach (int level in levels)
        {
            int mask = 0;
            for (int i = 0; i < SkillTree.Nodes.Length; i++)
                if (SkillTree.CanBuy(level, mask, SkillTree.Nodes[i]))
                    mask = SkillTree.Buy(level, mask, SkillTree.Nodes[i]);
            MatchState state = Simulation.Start(level, null, 100 + level, mask);
            state.Fighters[0].X = 8500;
            state.Fighters[1].X = 11500;
            state.Fighters[1].Facing = -1;
            state.Fighters[2].X = 10000;
            for (int t = 0; t < 3600 && state.Result == MatchResult.Fighting; t++)
                Simulation.Step(state, FighterInput(state, 0), FighterInput(state, 1));
            Console.WriteLine("Level " + level + ": " + state.Result +
                " in " + state.Tick + " ticks; boss HP " + state.Fighters[2].Health +
                "; fighters " + state.Fighters[0].Health + "/" + state.Fighters[1].Health);
        }
    }
}
