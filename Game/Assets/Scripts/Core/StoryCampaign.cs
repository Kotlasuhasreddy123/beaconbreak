using System;

namespace CoopFighter.Game
{
    // Stable legacy IDs preserve saves and upgrade unlocks. Only these ten
    // encounters are on the story route; intermediate IDs remain test fixtures.
    public static class StoryCampaign
    {
        public const int Count = 10;
        public static int Normalize(int level) { return Math.Min(100, Math.Max(10, ((level + 9) / 10) * 10)); }
        public static int Number(int level) { return (Normalize(level) / 10); }
        public static int Next(int level) { return Math.Min(100, Normalize(level) + 10); }
        public static bool IsBoss(int level) { return level >= 10 && level <= 100 && level % 10 == 0; }
        public static readonly string[] Rules = {
            "VEYRA / Shield faces forward. One draws her guard; the other flanks.",
            "SOREN / Copies your favorite strike. Change between low and aerial attacks.",
            "BRANN / Chains pull you together. Separate before attacking again.",
            "NIMA / Hunts whoever dealt more damage. Trade the lead; protect the marked fighter.",
            "ORIS / Alternates ground sweep and anti-air. Jump the sweep; land for the sky cut.",
            "SABLE / Armor holds while you stand together. Surround her to break the choir.",
            "IVO / Charges along his locked facing. Cross behind him during the wind-up.",
            "RHEA / Every third attack catches jumpers. Watch the tell, not her rhythm.",
            "CAEL / Shockwave reaches both sides. Jump together when the ground flashes.",
            "ASTER / Three phases: shield, charge, shockwave. Adapt as his health falls."
        };
        public static readonly string[] SceneTitles = {
            "THE NAME SHE DID NOT SEND", "A HUMAN CARGO", "ORDERS BEFORE THE WAR",
            "OUR SIGNATURES", "THE UNPREDICTED HAND", "SAVE THE VOICES",
            "THE MACHINE DISCARDS ITS MAKER", "THE DOOR SHE KEPT", "NO MORE PROMISES", "AN UNWRITTEN MORNING"
        };
        public static readonly string[] Aftermath = {
            "Veyra raises her blade toward the fallen Scout. The Keeper steps between them. She lowers it: 'My daughter is behind that gate. Bring her out.'",
            "Soren shatters his own mirror. Behind it: a cargo cage, not treasure. The fighters stop their pursuit to unlock it. A child carries Veyra's ribbon.",
            "Brann anchors his chains to both fighters. They pull apart, tearing the prison lock from its frame. Inside lies an order dated before the first uprising.",
            "Nima lays two seals beside the order. They fit the fighters' old insignia. The Keeper reaches for a weapon; the Scout offers an empty hand instead.",
            "Oris projects a future in which the Keeper abandons the Scout. The real Keeper turns back beneath falling lightning. The projection fractures.",
            "Sable shields the voice archive with her body. The Scout lowers a blade. The Keeper connects their stolen memory: the voices can leave without their jailer.",
            "Ivo's replacement blade turns on its maker. The fighters intercept it from opposite sides. Ivo tears out its command wire and gives them the severed core.",
            "Rhea unlocks a concealed door. Captive scouts pass her without saluting. She gives the Keeper her last route map: 'You do not owe me forgiveness.'",
            "Cael kneels beside the broken crossing. He offers no oath. Together, all three brace the fallen span until the last refugee reaches the far side.",
            "Aster offers one fighter a perfect future without the other. Both turn away. One severs the command ring while the other carries the voices into daylight."
        };
        public static readonly string[] Transfer = {
            "Veyra sends Soren your attack timings, then erases the child's route from the packet. The first gap in the perfect record is an act of mercy.",
            "Soren sends Brann the stolen combat ledger. Hidden in its margins are the cage keys. The network has learned your moves; you have learned its trade.",
            "Brann transmits your spacing to Nima. With it he sends the original order: the Beacon built its prisons before it predicted a war.",
            "Nima restores the prisoners' names and sends your patterns to Oris. The fighters keep their own guilty names in the ledger. Repair begins with truth.",
            "Oris sends Sable the failed prediction alongside your habits. For the first time, an official record admits: 'The model can be wrong.'",
            "Sable moves the voices into the stolen memory, then sends Ivo your rhythm. The mission changes: destroy the controller, preserve the people.",
            "Ivo sends Rhea the core's override and your attack ranges. The machine has marked every human guardian for replacement, including her prisoners.",
            "Rhea sends Cael your route openly. The scouts need time to escape; the fighters choose to become the distraction instead of hiding behind them.",
            "Cael sends Aster the final record: two fighters paused to save strangers. He leaves the bridge standing. 'Judge what they do next.'",
            "The command signal dies. The archive survives. Veyra finds her daughter; the scouts carry the names home. Tomorrow belongs to people again."
        };
        public static string Rule(int level) { return Rules[Number(level)-1]; }
        public static string Status(MatchState s) {
            int n=Number(s.Level);var b=s.Fighters[2];
            if(n==2)return "MIRROR  P1: "+Copied(s,0)+" / P2: "+Copied(s,1);
            if(n==4)return "MARKED: P"+(s.Telemetry.BossDamage[0]>=s.Telemetry.BossDamage[1]?1:2);
            if(n==6)return s.Fighters[0].Health>0 && s.Fighters[1].Health>0 && (long)(s.Fighters[0].X-b.X)*(s.Fighters[1].X-b.X)<0?"CHOIR ARMOR BROKEN":"CHOIR ARMOR ACTIVE";
            if(n==10)return b.Health*3>b.MaxHealth*2?"PHASE 1 / SHIELD":b.Health*3>b.MaxHealth?"PHASE 2 / CHARGE":"FINAL PHASE / SHOCKWAVE";
            return b.AttackTicks>0?Tell(b.Attack):b.AttackCooldown>0?"RECOVERING / YOUR OPENING":"WATCH THE GUARDIAN";
        }
        static string Copied(MatchState s,int slot) {
            int count=s.Telemetry.Attempts[slot];if(count<6)return "LEARNING";
            if(s.Telemetry.Low[slot]*2>count)return "LOW";
            if(s.Telemetry.Aerial[slot]*2>count)return "AIR";
            if((count-s.Telemetry.Low[slot]-s.Telemetry.Aerial[slot])*2>count)return "HIGH";
            return "UNPREDICTABLE";
        }
        public static AttackType Attack(MatchState s, AttackType fallback) {
            int n=Number(s.Level); var b=s.Fighters[2];
            if(n==5) return b.ComboDepth%2==0?AttackType.BossSweep:AttackType.BossAntiAir;
            if(n==7) return AttackType.BossCharge;
            if(n==8) return b.ComboDepth%3==0?AttackType.BossAntiAir:AttackType.BossStrike;
            if(n==9) return AttackType.BossWave;
            if(n==10) return b.Health*3>b.MaxHealth*2?AttackType.BossStrike:b.Health*3>b.MaxHealth?AttackType.BossCharge:AttackType.BossWave;
            return fallback;
        }
        public static string Tell(AttackType attack) {
            return attack==AttackType.BossWave?"SHOCKWAVE / JUMP":attack==AttackType.BossSweep?"SWEEP / JUMP":attack==AttackType.BossCharge?"CHARGE / CROSS BEHIND":attack==AttackType.BossAntiAir?"SKY CUT / STAY LOW":"STRIKE / GUARD";
        }
    }
}
