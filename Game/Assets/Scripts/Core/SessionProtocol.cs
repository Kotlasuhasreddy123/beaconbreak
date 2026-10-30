namespace CoopFighter.Game
{
    public static class SessionProtocol
    {
        // Bump whenever deterministic rules, input bits, or serialized fields change.
        public const string Version = "shadow-warriors-3.0-ten-guardians-20261029";
        public static bool Compatible(string remote) { return remote == Version; }
    }
}
