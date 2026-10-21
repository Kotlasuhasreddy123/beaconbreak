namespace CoopFighter.Game
{
    public static class SessionProtocol
    {
        // Bump whenever deterministic rules, input bits, or serialized fields change.
        public const string Version = "beaconbreak-2.0-rules-20261021";
        public static bool Compatible(string remote) { return remote == Version; }
    }
}
