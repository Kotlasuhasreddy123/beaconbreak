namespace CoopFighter.Game
{
    public static class SessionProtocol
    {
        // Bump whenever deterministic rules, input bits, or serialized fields change.
        public const string Version = "shadow-warriors-2.3-missions-20261025";
        public static bool Compatible(string remote) { return remote == Version; }
    }
}
