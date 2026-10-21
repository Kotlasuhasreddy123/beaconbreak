using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CoopFighter.Game;

namespace CoopFighter.UnityGame
{
    [Serializable]
    public sealed class NetworkEnvelope
    {
        public string kind;
        public string version;
        public int encounter;
        public int tick;
        public int buttons;
        public int hostButtons;
        public int level;
        public MatchState state;
        public CounterProfile profile;
        public string message;
    }

    // Transport only: parsing JSON and changing game state stay on Unity's
    // main thread. TCP framing is one UTF-8 JSON envelope per line.
#if !UNITY_WEBGL || UNITY_EDITOR
    public sealed class DirectConnection : IDisposable
    {
        private readonly ConcurrentQueue<string> incoming = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> outgoing = new ConcurrentQueue<string>();
        private readonly AutoResetEvent sendSignal = new AutoResetEvent(false);
        private readonly object sendLock = new object();
        private TcpListener listener;
        private TcpClient client;
        private StreamWriter writer;
        private Thread worker;
        private Thread sender;
        private volatile bool connected;
        private volatile bool disposed;

        public bool Connected { get { return connected; } }

        public void Host(int port)
        {
            if (worker != null) throw new InvalidOperationException("Connection already started.");
            worker = new Thread(delegate()
            {
                try
                {
                    listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    incoming.Enqueue("@Waiting for player 2 on port " + port);
                    TcpClient accepted = listener.AcceptTcpClient();
                    Attach(accepted);
                }
                catch (Exception ex) { if (!disposed) incoming.Enqueue("!" + ex.Message); }
            });
            worker.IsBackground = true;
            worker.Start();
        }

        public void Join(string host, int port)
        {
            if (worker != null) throw new InvalidOperationException("Connection already started.");
            worker = new Thread(delegate()
            {
                try
                {
                    TcpClient outgoing = new TcpClient();
                    client = outgoing;
                    var pending = outgoing.BeginConnect(host, port, null, null);
                    if (!pending.AsyncWaitHandle.WaitOne(8000)) throw new IOException("Connection timed out. Check the host address and that both devices share the same network.");
                    outgoing.EndConnect(pending);
                    Attach(outgoing);
                }
                catch (Exception ex) { if (!disposed) incoming.Enqueue("!" + ex.Message); }
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void Attach(TcpClient socket)
        {
            if (disposed) { socket.Close(); return; }
            client = socket;
            client.NoDelay = true;
            client.SendTimeout = 5000;
            NetworkStream stream = client.GetStream();
            writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.AutoFlush = true;
            connected = true;
            sender = new Thread(WriteLoop) { IsBackground = true };
            sender.Start();
            incoming.Enqueue("@Connected");
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            try
            {
                StringBuilder line = new StringBuilder();
                while (!disposed)
                {
                    int character = reader.Read();
                    if (character < 0) break;
                    if (character == '\n') {
                        if (incoming.Count > 512) throw new IOException("Peer exceeded message rate.");
                        incoming.Enqueue(line.ToString()); line.Length = 0;
                    } else if (character != '\r') {
                        if (line.Length >= 65536) throw new InvalidDataException("Network message too large.");
                        line.Append((char)character);
                    }
                }
            }
            catch (Exception ex) { if (!disposed) incoming.Enqueue("!" + ex.Message); }
            finally
            {
                connected = false;
                if (!disposed) incoming.Enqueue("!Connection closed.");
            }
        }

        public bool TryReceive(out string value)
        {
            return incoming.TryDequeue(out value);
        }

        public void Send(string json)
        {
            if (string.IsNullOrEmpty(json) || json.IndexOf('\n') >= 0)
                throw new ArgumentException("Expected one compact JSON line.", "json");
            if (!connected) return;
            if (outgoing.Count >= 512) { incoming.Enqueue("!Network is too slow. Please reconnect."); return; }
            outgoing.Enqueue(json); sendSignal.Set();
        }

        private void WriteLoop()
        {
            try {
                while (!disposed && connected) {
                    string message;
                    while (outgoing.TryDequeue(out message)) writer.WriteLine(message);
                    sendSignal.WaitOne(50);
                }
            } catch (Exception ex) {
                if (!disposed) incoming.Enqueue("!Connection lost: " + ex.Message);
                connected = false;
            }
        }

        public void Dispose()
        {
            disposed = true;
            connected = false;
            sendSignal.Set();
            if (client != null) client.Close();
            if (listener != null) listener.Stop();
            if (worker != null && worker.IsAlive) worker.Join(100);
            if (sender != null && sender.IsAlive) sender.Join(100);
        }
    }
#endif
}
