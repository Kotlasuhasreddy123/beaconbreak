using System;
using System.Threading;
using CoopFighter.UnityGame;

internal static class TransportChecks
{
    private static void Main()
    {
        using (DirectConnection host = new DirectConnection())
        using (DirectConnection guest = new DirectConnection())
        {
            host.Host(27552);
            guest.Join("127.0.0.1", 27552);
            DateTime deadline = DateTime.UtcNow.AddSeconds(3);
            while ((!host.Connected || !guest.Connected) && DateTime.UtcNow < deadline)
                Thread.Sleep(10);
            if (!host.Connected || !guest.Connected)
                throw new Exception("Loopback connection did not open.");
            guest.Send("{\"kind\":\"input\",\"tick\":1,\"buttons\":16}");
            string line;
            bool delivered = false;
            while (DateTime.UtcNow < deadline)
            {
                while (host.TryReceive(out line))
                    if (line.Contains("\"tick\":1")) delivered = true;
                if (delivered) break;
                Thread.Sleep(10);
            }
            if (!delivered) throw new Exception("Input frame was not delivered.");
        }
        Console.WriteLine("PASS: direct host/join loopback transport");
    }
}
