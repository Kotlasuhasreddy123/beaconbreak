#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace CoopFighter.UnityGame
{
    public sealed class DirectConnection : IDisposable
    {
        [DllImport("__Internal")] private static extern void BB_Open(string role, string room);
        [DllImport("__Internal")] private static extern void BB_Send(string data);
        [DllImport("__Internal")] private static extern void BB_Close();
        private readonly Queue<string> incoming = new Queue<string>();
        public static string Room;
        public bool Connected { get; private set; }
        public void Host(int unused) { BB_Open("host", Room); }
        public void Join(string room, int unused) { BB_Open("guest", room); }
        public void Inject(string line) {
            if (line == "@Connected") Connected = true;
            if (line.StartsWith("!")) Connected = false;
            if (incoming.Count < 512) incoming.Enqueue(line);
        }
        public bool TryReceive(out string line) { line = incoming.Count > 0 ? incoming.Dequeue() : null; return line != null; }
        public void Send(string json) { if (Connected) BB_Send(json); }
        public void Dispose() { Connected = false; BB_Close(); incoming.Clear(); }
    }
}
#endif
