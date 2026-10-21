using System;

namespace CoopFighter.Game
{
    public sealed class SkillNode
    {
        public readonly int Bit;
        public readonly string Id;
        public readonly string Name;
        public readonly string Effect;
        public readonly int RequiredLevel;
        public readonly int Prerequisites;

        public SkillNode(int bit, string id, string name, string effect,
            int requiredLevel, int prerequisites)
        {
            Bit = bit; Id = id; Name = name; Effect = effect;
            RequiredLevel = requiredLevel; Prerequisites = prerequisites;
        }
    }

    public static class SkillTree
    {
        public static readonly SkillNode[] Nodes = {
            new SkillNode(0, "ally_guard", "Ally Guard", "Friendly damage 25%", 6, 0),
            new SkillNode(1, "flow_one", "Flow I", "+1 combo step", 11, 0),
            new SkillNode(2, "spear_mastery", "Reach", "+200 attack reach", 26, 0),
            new SkillNode(3, "sky_step", "Sky Step", "Higher jump", 31, 1 << 1),
            new SkillNode(4, "tandem_pulse", "Tandem Pulse", "Faster tandem recharge", 36, 0),
            new SkillNode(5, "guardian_oath", "Guardian Oath", "+40 shared fighter health", 41, 1 << 0),
            new SkillNode(6, "flow_two", "Flow II", "+1 combo step", 51, 1 << 1),
            new SkillNode(7, "blade_focus", "Blade Focus", "+5 strike damage", 56, 1 << 2),
            new SkillNode(8, "airbreaker", "Airbreaker", "+10 aerial damage", 61, 1 << 3),
            new SkillNode(9, "tandem_finale", "Tandem Finale", "+40 tandem damage", 76, 1 << 4),
            new SkillNode(10, "master_guard", "Master Guard", "Friendly damage 15%", 81, 1 << 5),
            new SkillNode(11, "flow_three", "Flow III", "+1 combo step", 91, 1 << 6),
            new SkillNode(12, "low_edge", "Low Edge", "+6 low damage", 16, 1 << 1),
            new SkillNode(13, "guard_link", "Guard Link", "Stronger guard", 21, 1 << 0),
            new SkillNode(14, "quick_chain", "Quick Chain", "Faster attack recovery", 46, 1 << 1),
            new SkillNode(15, "ally_recovery", "Ally Recovery", "Shorter friendly stun", 66, 1 << 0),
            new SkillNode(16, "endurance", "Endurance", "+35 fighter health", 71, 1 << 5),
            new SkillNode(17, "sky_breaker", "Sky Breaker", "+8 aerial damage", 86, 1 << 8),
            new SkillNode(18, "beacon_edge", "Beacon Edge", "+5 versus guarded bosses", 96, 1 << 7)
        };
        public static readonly SkillNode[] OrderedNodes = OrderByLevel();

        private static SkillNode[] OrderByLevel()
        {
            SkillNode[] result = (SkillNode[])Nodes.Clone();
            Array.Sort(result, delegate(SkillNode a, SkillNode b)
            {
                return a.RequiredLevel.CompareTo(b.RequiredLevel);
            });
            return result;
        }

        public static bool Has(int mask, int bit)
        {
            return (mask & (1 << bit)) != 0;
        }

        public static int PointsAvailable(int highestUnlockedLevel, int mask)
        {
            int earned = Math.Max(0, (highestUnlockedLevel - 1) / 5);
            int spent = 0;
            for (int i = 0; i < Nodes.Length; i++) if (Has(mask, Nodes[i].Bit)) spent++;
            return earned - spent;
        }

        public static bool CanBuy(int highestUnlockedLevel, int mask, SkillNode node)
        {
            return node != null && node.RequiredLevel <= highestUnlockedLevel &&
                !Has(mask, node.Bit) && (mask & node.Prerequisites) == node.Prerequisites &&
                PointsAvailable(highestUnlockedLevel, mask) > 0;
        }

        public static int Buy(int highestUnlockedLevel, int mask, SkillNode node)
        {
            if (!CanBuy(highestUnlockedLevel, mask, node))
                throw new InvalidOperationException("Skill is locked or there are no points available.");
            return mask | (1 << node.Bit);
        }
    }
}
