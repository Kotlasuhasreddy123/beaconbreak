using System;

namespace CoopFighter.Game
{
    // Reproducible behavioral fixtures, not a substitute for human playtesting.
    public enum PlayerPersona { Beginner, Masher, Balanced, Defender, LowSpecialist,
        Jumper, Flanker, TandemPartner, Aggressor, Idle, Griefer, Runner, SlowReaction }

    public static class PlayerBot
    {
        public static Buttons Read(MatchState state, int slot, PlayerPersona persona, int seed = 0)
        {
            FighterState f = state.Fighters[slot], boss = state.Fighters[2];
            if (f.Health <= 0 || persona == PlayerPersona.Idle) return Buttons.None;
            int tick = state.Tick, dx = boss.X - f.X;
            int direction = dx < 0 ? -1 : 1;
            Buttons toward = direction < 0 ? Buttons.Left : Buttons.Right;
            int phase = tick + slot * 11 + seed * 7;
            if (persona == PlayerPersona.Griefer) {
                int ally = state.Fighters[1 - slot].X - f.X;
                return (ally < 0 ? Buttons.Left : Buttons.Right) |
                    (phase % 20 == 0 ? Buttons.High : Buttons.None);
            }
            if (persona == PlayerPersona.Runner) return dx > 0 ? Buttons.Left : Buttons.Right;
            bool skilled = persona == PlayerPersona.Flanker || persona == PlayerPersona.TandemPartner || persona == PlayerPersona.Balanced;
            // Experienced players deliberately acquire opposite attack lanes.
            if (skilled && slot == 1 && f.X < boss.X + 1050 && boss.X < 18000)
                return Buttons.Right | (phase % 36 == 0 ? Buttons.Jump : Buttons.None);
            if (f.Facing != direction) return toward;
            int reach = persona == PlayerPersona.LowSpecialist ? 1150 : 1600;
            if (Math.Abs(dx) > reach) return toward;
            if (persona == PlayerPersona.Defender) {
                if (boss.AttackTicks > 0 || phase % 50 > 2) return Buttons.Guard;
                return Buttons.High;
            }
            if (skilled && boss.AttackTicks > 0 && boss.AttackTicks <= 20 && boss.Facing * dx < 0)
                return Buttons.Guard;
            int cadence = persona == PlayerPersona.Beginner ? 67 : persona == PlayerPersona.SlowReaction ? 49 :
                persona == PlayerPersona.Masher ? 4 : persona == PlayerPersona.Aggressor ? 18 : 22;
            Buttons action = phase % cadence == 0 ? Buttons.High : Buttons.None;
            // Experienced fighters time the next strike to recovery, rather than
            // repeatedly pressing during cooldown and losing their input edge.
            if (persona == PlayerPersona.Flanker || persona == PlayerPersona.TandemPartner)
                action = f.AttackCooldown == 0 && f.AttackTicks == 0 ? Buttons.High : Buttons.None;
            if (persona == PlayerPersona.LowSpecialist) action = phase % 22 == 0 ? Buttons.Low : Buttons.None;
            if (persona == PlayerPersona.Jumper) {
                action = phase % 34 == 0 ? Buttons.Jump : phase % 34 == 6 || phase % 34 == 27 ? Buttons.High : Buttons.None;
            }
            if (persona == PlayerPersona.Beginner && phase % 107 < 18) action |= Buttons.Guard;
            if (persona == PlayerPersona.TandemPartner && tick % 300 == 0) action |= Buttons.Tandem;
            return action;
        }
    }
}
