using System;

namespace CoopFighter.Game
{
    [Flags]
    public enum Buttons
    {
        None = 0, Left = 1, Right = 2, Jump = 4, Low = 8,
        High = 16, Guard = 32, Tandem = 64
    }

    public enum FighterKind { Player, Boss }
    public enum MatchResult { Fighting, Victory, Defeat }
    public enum BossStyle { Sentinel, Duelist, Grappler, Zoner }
    public enum AttackType { None, Low, High, Aerial, BossStrike, BossAntiAir }

    [Serializable]
    public sealed class LevelSpec
    {
        private static readonly LevelSpec[] Cache = new LevelSpec[101];
        public int Level;
        public string ActName;
        public string ActStory;
        public string BossName;
        public BossStyle Style;
        public int BossHealth;
        public int BossDamage;
        public int BossSpeed;
        public int ArenaWidth;
        public int WeaponTier;
        public string WeaponName;
        public int MaxCombo;
        public bool TandemUnlocked;

        public static LevelSpec Create(int level)
        {
            if (level < 1 || level > 100) throw new ArgumentOutOfRangeException("level");
            if (Cache[level] != null) return Cache[level];
            string[] acts = { "Ash Gate", "Glass Docks", "Iron Orchard", "Hollow Market",
                "Storm Archive", "Copper Basilica", "Moon Foundry", "Silent Citadel",
                "Rift Crossing", "The Last Beacon" };
            string[] stories = {
                "Find the first Warden and learn why the Beacon records every duel.",
                "Follow the data couriers through a city built on broken mirrors.",
                "Break the foundry's supply line while its guardians study your reach.",
                "Recover the names of fighters erased from the combat archive.",
                "Survive the storm vault and uncover the Warden network's command chain.",
                "Turn the cathedral's signal against its own sentinels.",
                "Forge a weapon the network has never catalogued.",
                "Free the captured scouts and expose the architects of the trials.",
                "Cross the rift while the Wardens combine every prior countermeasure.",
                "Reach the Beacon and decide what its final record will contain."
            };
            string[] titles = { "Sentinel", "Duelist", "Grappler", "Zoner" };
            string[] weapons = { "training blade", "long spear", "paired sabers",
                "chain staff", "beacon edge" };
            int act = (level - 1) / 10;
            int encounter = (level - 1) % 10;
            BossStyle style = (BossStyle)((act + encounter) % 4);
            return Cache[level] = new LevelSpec {
                Level = level,
                ActName = acts[act],
                ActStory = stories[act],
                BossName = acts[act] + " " + titles[(int)style] + " " + (encounter + 1),
                Style = style,
                // A bounded 3.5x campaign curve; later acts add tactical depth.
                BossHealth = 650 + (level - 1) * 42 + (encounter == 9 ? 350 : 0),
                BossDamage = 8 + (level - 1) / 5,
                BossSpeed = 70 + act * 3 + (style == BossStyle.Duelist ? 18 : 0),
                ArenaWidth = 20000,
                WeaponTier = Math.Min(4, (level - 1) / 20),
                WeaponName = weapons[Math.Min(4, (level - 1) / 20)],
                MaxCombo = Math.Min(5, 1 + (level - 1) / 20),
                TandemUnlocked = level >= 5
            };
        }
    }

    [Serializable]
    public sealed class FighterState
    {
        public int Id;
        public FighterKind Kind;
        public int X;
        public int Y;
        public int VerticalSpeed;
        public int Facing;
        public int Health;
        public int MaxHealth;
        public int StunTicks;
        public int AttackTicks;
        public int AttackCooldown;
        public AttackType Attack;
        public int HitTargets;
        public int ComboDepth;
        public int ComboGrace;
        public bool Guarding;
        public Buttons PreviousButtons;

        public FighterState Copy()
        {
            return (FighterState)MemberwiseClone();
        }
    }

    [Serializable]
    public sealed class BattleTelemetry
    {
        public int[] Attempts = new int[2];
        public int[] Low = new int[2];
        public int[] Aerial = new int[2];
        public int[] BossDamage = new int[2];
        public int FriendlyHits;

        public BattleTelemetry Copy()
        {
            return new BattleTelemetry {
                Attempts = (int[])Attempts.Clone(), Low = (int[])Low.Clone(),
                Aerial = (int[])Aerial.Clone(), BossDamage = (int[])BossDamage.Clone(),
                FriendlyHits = FriendlyHits
            };
        }
    }

    [Serializable]
    public sealed class CounterProfile
    {
        public int SourceLevel;
        public int TargetLevel;
        public int LowBlockWeight;
        public int AntiAirWeight;
        public int PressureWeight;
        public int FocusPlayer;
        public string StoryLine;

        public CounterProfile Copy()
        {
            return (CounterProfile)MemberwiseClone();
        }

        public static CounterProfile FromBattle(int completedLevel, BattleTelemetry telemetry)
        {
            if (completedLevel < 1 || completedLevel >= 100)
                throw new ArgumentOutOfRangeException("completedLevel");
            if (telemetry == null) throw new ArgumentNullException("telemetry");
            bool low = false, aerial = false;
            for (int i = 0; i < 2; i++)
            {
                if (telemetry.Attempts[i] < 8) continue;
                low |= telemetry.Low[i] * 100 >= telemetry.Attempts[i] * 55;
                aerial |= telemetry.Aerial[i] * 100 >= telemetry.Attempts[i] * 55;
            }
            bool pressure = telemetry.FriendlyHits >= 5;
            int totalBossDamage = telemetry.BossDamage[0] + telemetry.BossDamage[1];
            int focus = totalBossDamage == 0 ? 0 :
                telemetry.BossDamage[0] * 100 >= totalBossDamage * 60 ? 1 :
                telemetry.BossDamage[1] * 100 >= totalBossDamage * 60 ? 2 : 0;
            return new CounterProfile {
                SourceLevel = completedLevel, TargetLevel = completedLevel + 1,
                LowBlockWeight = low ? 55 : 20,
                AntiAirWeight = aerial ? 55 : 20,
                PressureWeight = pressure ? 45 : 20,
                FocusPlayer = focus,
                StoryLine = low ? "Your low strikes were recorded. I have learned to guard them." :
                    aerial ? "Your leaps were recorded. I will meet you above." :
                    pressure ? "Your blades crossed. I will force you closer." :
                    "The record is incomplete. Show me what you can do."
            };
        }
    }

    [Serializable]
    public sealed class MatchState
    {
        public int Tick;
        public int Level;
        public int SkillMask;
        public MatchResult Result;
        public FighterState[] Fighters;
        public BattleTelemetry Telemetry;
        public CounterProfile Adaptation;
        public int RandomState;
        public int TandemCooldown;
        public int[] LastTandemPress = new int[2];
        public int LastHitAttacker;
        public int LastHitTarget;
        public int LastHitDamage;
        public bool LastHitFriendly;

        public MatchState Copy()
        {
            return new MatchState {
                Tick = Tick, Level = Level, SkillMask = SkillMask, Result = Result,
                Fighters = new [] { Fighters[0].Copy(), Fighters[1].Copy(), Fighters[2].Copy() },
                Telemetry = Telemetry.Copy(), Adaptation = Adaptation == null ? null : Adaptation.Copy(),
                RandomState = RandomState, TandemCooldown = TandemCooldown,
                LastTandemPress = (int[])LastTandemPress.Clone(),
                LastHitAttacker = LastHitAttacker, LastHitTarget = LastHitTarget,
                LastHitDamage = LastHitDamage, LastHitFriendly = LastHitFriendly
            };
        }

        public uint Hash()
        {
            uint h = 2166136261;
            Mix(ref h, Tick); Mix(ref h, Level); Mix(ref h, SkillMask); Mix(ref h, (int)Result);
            Mix(ref h, RandomState); Mix(ref h, TandemCooldown);
            Mix(ref h, LastTandemPress[0]); Mix(ref h, LastTandemPress[1]);
            Mix(ref h, LastHitAttacker); Mix(ref h, LastHitTarget); Mix(ref h, LastHitDamage);
            Mix(ref h, LastHitFriendly ? 1 : 0);
            if (Adaptation != null) {
                Mix(ref h, Adaptation.SourceLevel); Mix(ref h, Adaptation.TargetLevel);
                Mix(ref h, Adaptation.LowBlockWeight); Mix(ref h, Adaptation.AntiAirWeight);
                Mix(ref h, Adaptation.PressureWeight); Mix(ref h, Adaptation.FocusPlayer);
            }
            for (int i = 0; i < 3; i++)
            {
                FighterState f = Fighters[i];
                Mix(ref h, f.Id); Mix(ref h, (int)f.Kind); Mix(ref h, f.MaxHealth);
                Mix(ref h, f.X); Mix(ref h, f.Y); Mix(ref h, f.VerticalSpeed);
                Mix(ref h, f.Facing); Mix(ref h, f.Health); Mix(ref h, f.StunTicks);
                Mix(ref h, f.AttackTicks); Mix(ref h, f.AttackCooldown);
                Mix(ref h, (int)f.Attack); Mix(ref h, f.HitTargets);
                Mix(ref h, f.ComboDepth); Mix(ref h, f.ComboGrace);
                Mix(ref h, f.Guarding ? 1 : 0); Mix(ref h, (int)f.PreviousButtons);
            }
            for (int i = 0; i < 2; i++)
            {
                Mix(ref h, Telemetry.Attempts[i]); Mix(ref h, Telemetry.Low[i]);
                Mix(ref h, Telemetry.Aerial[i]); Mix(ref h, Telemetry.BossDamage[i]);
            }
            Mix(ref h, Telemetry.FriendlyHits);
            return h;
        }

        private static void Mix(ref uint h, int value)
        {
            unchecked { h = (h ^ (uint)value) * 16777619; }
        }
    }

    public static class Simulation
    {
        public const int TicksPerSecond = 60;
        private const int Ground = 0;
        private const int JumpSpeed = 530;
        private const int Gravity = 35;

        public static MatchState Start(int level, CounterProfile adaptation, int seed, int skillMask = 0)
        {
            LevelSpec spec = LevelSpec.Create(level);
            if (adaptation != null && adaptation.TargetLevel != level)
                throw new ArgumentException("Adaptation targets another level.", "adaptation");
            return new MatchState {
                Level = level, SkillMask = skillMask, Result = MatchResult.Fighting,
                RandomState = seed == 0 ? 1 : seed,
                Fighters = new [] {
                    NewFighter(1, FighterKind.Player, 3500, 400 + level / 4 +
                        (SkillTree.Has(skillMask, 5) ? 40 : 0) +
                        (SkillTree.Has(skillMask, 16) ? 35 : 0)),
                    NewFighter(2, FighterKind.Player, 5500, 400 + level / 4 +
                        (SkillTree.Has(skillMask, 5) ? 40 : 0) +
                        (SkillTree.Has(skillMask, 16) ? 35 : 0)),
                    NewFighter(3, FighterKind.Boss, 15000, spec.BossHealth)
                },
                // Unity serializes inline null classes as default objects. Use an
                // explicit neutral profile so snapshot restoration preserves policy.
                Telemetry = new BattleTelemetry(), Adaptation = adaptation == null ? new CounterProfile {
                    TargetLevel = level, LowBlockWeight = 20, AntiAirWeight = 20,
                    PressureWeight = 20, StoryLine = "No prior record. Keep your approach unpredictable."
                } : adaptation.Copy(),
                LastTandemPress = new [] { -1000, -1000 }
            };
        }

        private static FighterState NewFighter(int id, FighterKind kind, int x, int health)
        {
            return new FighterState { Id = id, Kind = kind, X = x, Y = Ground,
                Facing = kind == FighterKind.Boss ? -1 : 1,
                Health = health, MaxHealth = health };
        }

        public static void Step(MatchState state, Buttons player1, Buttons player2)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (state.Result != MatchResult.Fighting) return;
            state.Tick++;
            state.LastHitAttacker = state.LastHitTarget = state.LastHitDamage = 0;
            state.LastHitFriendly = false;
            if (state.TandemCooldown > 0) state.TandemCooldown--;
            UpdatePlayer(state, 0, player1);
            UpdatePlayer(state, 1, player2);
            UpdateBoss(state);
            for (int i = 0; i < 3; i++) AdvanceAttack(state.Fighters[i]);
            // Fixed actor/target order is part of the simulation contract.
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (i != j) ResolveAttack(state, i, j);
            TryTandem(state);
            if (state.Fighters[2].Health <= 0) state.Result = MatchResult.Victory;
            else if (state.Fighters[0].Health <= 0 && state.Fighters[1].Health <= 0)
                state.Result = MatchResult.Defeat;
            else if (state.Tick >= 10800) state.Result = MatchResult.Defeat;
        }

        private static void UpdatePlayer(MatchState state, int slot, Buttons buttons)
        {
            FighterState f = state.Fighters[slot];
            if (f.Health <= 0) { f.PreviousButtons = buttons; return; }
            TickCommon(f);
            f.Guarding = (buttons & Buttons.Guard) != 0 && f.StunTicks == 0 &&
                f.AttackTicks == 0 && f.Y == Ground;
            if (f.StunTicks == 0 && !f.Guarding)
            {
                int direction = ((buttons & Buttons.Right) != 0 ? 1 : 0) -
                                ((buttons & Buttons.Left) != 0 ? 1 : 0);
                f.X = Clamp(f.X + direction * 95, 500, LevelSpec.Create(state.Level).ArenaWidth - 500);
                if (direction != 0) f.Facing = direction;
                if (Pressed(buttons, f.PreviousButtons, Buttons.Jump) && f.Y == Ground)
                    f.VerticalSpeed = JumpSpeed + (SkillTree.Has(state.SkillMask, 3) ? 90 : 0);
                if (f.AttackTicks == 0 && f.AttackCooldown == 0)
                {
                    AttackType attack = AttackType.None;
                    if (Pressed(buttons, f.PreviousButtons, Buttons.Low)) attack = AttackType.Low;
                    else if (Pressed(buttons, f.PreviousButtons, Buttons.High))
                        attack = f.Y > Ground ? AttackType.Aerial : AttackType.High;
                    if (attack != AttackType.None)
                    {
                        int maxCombo = LevelSpec.Create(state.Level).MaxCombo +
                            (SkillTree.Has(state.SkillMask, 1) ? 1 : 0) +
                            (SkillTree.Has(state.SkillMask, 6) ? 1 : 0) +
                            (SkillTree.Has(state.SkillMask, 11) ? 1 : 0);
                        f.ComboDepth = f.ComboGrace > 0 ? Math.Min(maxCombo, f.ComboDepth + 1) : 1;
                        f.ComboGrace = 32;
                        f.Attack = attack; f.AttackTicks = 12;
                        f.AttackCooldown = SkillTree.Has(state.SkillMask, 14) ? 20 : 24;
                        f.HitTargets = 0;
                        state.Telemetry.Attempts[slot]++;
                        if (attack == AttackType.Low) state.Telemetry.Low[slot]++;
                        if (attack == AttackType.Aerial) state.Telemetry.Aerial[slot]++;
                    }
                }
                if (Pressed(buttons, f.PreviousButtons, Buttons.Tandem))
                    state.LastTandemPress[slot] = state.Tick;
            }
            f.PreviousButtons = buttons;
        }

        private static void UpdateBoss(MatchState state)
        {
            FighterState boss = state.Fighters[2];
            if (boss.Health <= 0) return;
            TickCommon(boss);
            FighterState p1 = state.Fighters[0], p2 = state.Fighters[1];
            FighterState target = p1.Health <= 0 ? p2 : p2.Health <= 0 ? p1 :
                Math.Abs(p1.X - boss.X) <= Math.Abs(p2.X - boss.X) ? p1 : p2;
            if (state.Adaptation != null && state.Adaptation.FocusPlayer != 0)
            {
                FighterState focus = state.Fighters[state.Adaptation.FocusPlayer - 1];
                if (focus.Health > 0 && Math.Abs(focus.X - boss.X) < 6500) target = focus;
            }
            int distance = target.X - boss.X;
            // A committed attack cannot swivel to track a fighter crossing behind it.
            if (boss.AttackTicks == 0) boss.Facing = distance < 0 ? -1 : 1;
            int abs = Math.Abs(distance);
            LevelSpec spec = LevelSpec.Create(state.Level);
            int preferred = spec.Style == BossStyle.Zoner ? 3800 :
                spec.Style == BossStyle.Grappler ? 1100 : 1500;
            if (state.Adaptation != null && state.Adaptation.PressureWeight >= 40)
                preferred = Math.Min(preferred, 1000);
            if (boss.StunTicks == 0 && boss.AttackTicks == 0)
            {
                if (abs > preferred + 300) boss.X += boss.Facing * spec.BossSpeed;
                else if (abs < preferred - 500 && spec.Style == BossStyle.Zoner)
                    boss.X -= boss.Facing * spec.BossSpeed;
                boss.X = Clamp(boss.X, 500, spec.ArenaWidth - 500);
                if (abs < (spec.Style == BossStyle.Zoner ? 4500 : 2400) && boss.AttackCooldown == 0)
                {
                    bool airborne = target.Y > 700;
                    int antiWeight = state.Adaptation == null ? 20 : state.Adaptation.AntiAirWeight;
                    boss.Attack = airborne && NextRandom(state) % 100 < antiWeight ?
                        AttackType.BossAntiAir : AttackType.BossStrike;
                    // Every boss attack has a visible wind-up before the 9..4 active window.
                    boss.AttackTicks = spec.Style == BossStyle.Duelist ? 18 : 24;
                    boss.AttackCooldown = spec.Style == BossStyle.Duelist ? 28 : 40;
                    boss.HitTargets = 0;
                }
            }
            boss.Guarding = false;
            if (boss.StunTicks == 0 && boss.AttackTicks == 0)
            {
                int lowWeight = state.Adaptation == null ? 20 : state.Adaptation.LowBlockWeight;
                uint roll = NextRandom(state) % 100;
                bool incomingLow = (p1.Attack == AttackType.Low && p1.AttackTicks > 0) ||
                    (p2.Attack == AttackType.Low && p2.AttackTicks > 0);
                boss.Guarding = incomingLow && roll < lowWeight;
            }
        }

        private static void TickCommon(FighterState f)
        {
            if (f.StunTicks > 0) f.StunTicks--;
            if (f.AttackCooldown > 0) f.AttackCooldown--;
            if (f.ComboGrace > 0) f.ComboGrace--;
            if (f.Y > Ground || f.VerticalSpeed > 0)
            {
                f.Y += f.VerticalSpeed;
                f.VerticalSpeed -= Gravity;
                if (f.Y <= Ground) { f.Y = Ground; f.VerticalSpeed = 0; }
            }
        }

        private static void AdvanceAttack(FighterState f)
        {
            if (f.AttackTicks > 0) f.AttackTicks--;
            if (f.AttackTicks == 0) f.Attack = AttackType.None;
        }

        private static void ResolveAttack(MatchState state, int attackerIndex, int targetIndex)
        {
            FighterState attacker = state.Fighters[attackerIndex];
            FighterState target = state.Fighters[targetIndex];
            if (attacker.Health <= 0 || target.Health <= 0 || attacker.Attack == AttackType.None ||
                attacker.AttackTicks < 4 || attacker.AttackTicks > 9 ||
                (attacker.HitTargets & (1 << targetIndex)) != 0) return;
            if (attacker.Kind == FighterKind.Boss && target.Kind == FighterKind.Boss) return;
            int dx = target.X - attacker.X;
            if (dx * attacker.Facing < 0) return;
            int range = (attacker.Attack == AttackType.BossStrike ||
                attacker.Attack == AttackType.BossAntiAir) &&
                LevelSpec.Create(state.Level).Style == BossStyle.Zoner ? 4500 :
                attacker.Attack == AttackType.Low ? 1500 : 2000 +
                (attacker.Kind == FighterKind.Player ? LevelSpec.Create(state.Level).WeaponTier * 200 +
                    (SkillTree.Has(state.SkillMask, 2) ? 200 : 0) : 0);
            if (Math.Abs(dx) > range) return;
            if (attacker.Attack == AttackType.Low && target.Y > 700) return;
            if (attacker.Attack == AttackType.High && Math.Abs(target.Y - attacker.Y) > 1600) return;
            if (attacker.Attack == AttackType.Aerial && Math.Abs(target.Y - attacker.Y) > 1300) return;
            if (attacker.Attack == AttackType.BossStrike && target.Y > 700) return;
            if (attacker.Attack == AttackType.BossAntiAir && target.Y < 500) return;
            bool friendly = attacker.Kind == FighterKind.Player && target.Kind == FighterKind.Player;
            int raw = attacker.Kind == FighterKind.Boss ? LevelSpec.Create(state.Level).BossDamage +
                (attacker.Attack == AttackType.BossAntiAir ? 5 : 0) :
                (attacker.Attack == AttackType.Low ? 22 : 28) +
                LevelSpec.Create(state.Level).WeaponTier * 3 + (attacker.ComboDepth - 1) * 2;
            if (attacker.Kind == FighterKind.Player && SkillTree.Has(state.SkillMask, 7)) raw += 5;
            if (attacker.Attack == AttackType.Aerial && SkillTree.Has(state.SkillMask, 8)) raw += 10;
            if (attacker.Attack == AttackType.Low && SkillTree.Has(state.SkillMask, 12)) raw += 6;
            if (attacker.Attack == AttackType.Aerial && SkillTree.Has(state.SkillMask, 17)) raw += 8;
            int friendlyPercent = SkillTree.Has(state.SkillMask, 10) ? 15 :
                SkillTree.Has(state.SkillMask, 0) ? 25 : 30;
            int damage = friendly ? Math.Max(1, raw * friendlyPercent / 100) : raw;
            bool blocked = target.Guarding && (attacker.X - target.X) * target.Facing >= 0;
            if (blocked) damage = Math.Max(1,
                damage / (target.Kind == FighterKind.Player && SkillTree.Has(state.SkillMask, 13) ? 6 : 4));
            if (target.Kind == FighterKind.Boss && blocked && attacker.Attack == AttackType.Low)
                damage = Math.Max(1, damage / 2);
            if (target.Kind == FighterKind.Boss && blocked &&
                SkillTree.Has(state.SkillMask, 18)) damage += 5;
            // Allies can disrupt spacing but cannot deliberately finish a teammate.
            damage = Math.Min(damage, friendly ? Math.Max(0, target.Health - 1) : target.Health);
            target.Health -= damage;
            target.StunTicks = friendly ? (SkillTree.Has(state.SkillMask, 15) ? 1 : 3) :
                blocked ? 3 : 12;
            // Wardens commit to their clearly telegraphed strike. Attacking into
            // the wind-up does damage but cannot permanently stun-lock the boss.
            if (target.Kind == FighterKind.Boss) target.StunTicks = System.Math.Min(3, target.StunTicks);
            else if (!friendly && !blocked) { target.Attack = AttackType.None; target.AttackTicks = 0; }
            int push = attacker.Facing * (friendly ? 280 : 440);
            if (attacker.Kind == FighterKind.Boss && state.Adaptation != null &&
                state.Adaptation.PressureWeight >= 40 && targetIndex < 2)
            {
                FighterState ally = state.Fighters[1 - targetIndex];
                push = ally.X < target.X ? -440 : 440;
            }
            target.X = Clamp(target.X + push, 500,
                LevelSpec.Create(state.Level).ArenaWidth - 500);
            attacker.HitTargets |= (1 << targetIndex);
            if (friendly) state.Telemetry.FriendlyHits++;
            if (attacker.Kind == FighterKind.Player && target.Kind == FighterKind.Boss)
                state.Telemetry.BossDamage[attackerIndex] += damage;
            state.LastHitAttacker = attacker.Id; state.LastHitTarget = target.Id;
            state.LastHitDamage = damage; state.LastHitFriendly = friendly;
        }

        private static void TryTandem(MatchState state)
        {
            if (state.Level < 5 || state.TandemCooldown > 0 ||
                state.Fighters[0].Health <= 0 || state.Fighters[1].Health <= 0) return;
            if (state.Tick - state.LastTandemPress[0] > 8 ||
                state.Tick - state.LastTandemPress[1] > 8) return;
            FighterState boss = state.Fighters[2];
            if (boss.Health <= 0 || Math.Abs(boss.X - state.Fighters[0].X) > 2800 ||
                Math.Abs(boss.X - state.Fighters[1].X) > 2800) return;
            int damage = Math.Min(boss.Health, 75 + LevelSpec.Create(state.Level).WeaponTier * 12 +
                (SkillTree.Has(state.SkillMask, 9) ? 40 : 0));
            boss.Health -= damage; boss.StunTicks = 25;
            state.Telemetry.BossDamage[0] += damage / 2;
            state.Telemetry.BossDamage[1] += damage - damage / 2;
            state.TandemCooldown = SkillTree.Has(state.SkillMask, 4) ? 220 : 300;
            state.LastTandemPress[0] = state.LastTandemPress[1] = -1000;
            state.LastHitAttacker = 12; state.LastHitTarget = 3;
            state.LastHitDamage = damage; state.LastHitFriendly = false;
        }

        private static bool Pressed(Buttons current, Buttons previous, Buttons button)
        {
            return (current & button) != 0 && (previous & button) == 0;
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        private static uint NextRandom(MatchState state)
        {
            uint x = unchecked((uint)state.RandomState);
            x ^= x << 13; x ^= x >> 17; x ^= x << 5;
            state.RandomState = unchecked((int)x);
            return x;
        }
    }
}
