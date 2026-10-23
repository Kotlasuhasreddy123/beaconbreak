namespace CoopFighter.Game
{
    public static class SessionProtocol
    {
        // Bump whenever deterministic rules, input bits, or serialized fields change.
        public const string Version = "duskbound-2.1-story-20261023";
        public static bool Compatible(string remote) { return remote == Version; }
    }
}
