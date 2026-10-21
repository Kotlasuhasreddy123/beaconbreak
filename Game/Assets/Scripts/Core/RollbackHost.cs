using System;
using System.Collections.Generic;

namespace CoopFighter.Game
{
    // The host predicts absent remote input as the last known input. A late
    // correction restores the snapshot before that tick and replays forward.
    public sealed class RollbackHost
    {
        private sealed class Frame
        {
            public MatchState Before;
            public Buttons Local;
            public Buttons AppliedRemote;
            public Buttons LastRemoteBefore;
        }

        private readonly Dictionary<int, Frame> frames = new Dictionary<int, Frame>();
        private readonly Dictionary<int, Buttons> actualRemote = new Dictionary<int, Buttons>();
        private Buttons lastRemote;
        private int confirmedThrough;
        private int prunedThrough;
        public const int MaxPredictionTicks = 120;
        private MatchState state;

        public MatchState State { get { return state; } }
        public int ConfirmedThrough { get { return confirmedThrough; } }
        public bool WaitingForRemote { get { return state.Tick - confirmedThrough >= MaxPredictionTicks; } }
        public bool ResultConfirmed
        {
            get { return state.Result != MatchResult.Fighting && confirmedThrough >= state.Tick; }
        }

        public RollbackHost(MatchState initial)
        {
            if (initial == null || initial.Tick != 0)
                throw new ArgumentException("A fresh match at tick zero is required.", "initial");
            state = initial.Copy();
        }

        public void Step(Buttons local)
        {
            if (state.Result != MatchResult.Fighting || WaitingForRemote) return;
            int tick = state.Tick + 1;
            Buttons remote;
            if (!actualRemote.TryGetValue(tick, out remote)) remote = lastRemote;
            frames[tick] = new Frame { Before = state.Copy(), Local = local,
                AppliedRemote = remote, LastRemoteBefore = lastRemote };
            Simulation.Step(state, local, remote);
            lastRemote = remote;
            PruneConfirmedHistory();
        }

        public void SubmitRemote(int tick, Buttons input)
        {
            if (tick < 1) throw new ArgumentOutOfRangeException("tick");
            if (tick <= prunedThrough) return; // Harmless retransmission of confirmed history.
            if (tick > state.Tick + MaxPredictionTicks) throw new ArgumentOutOfRangeException("tick");
            Buttons prior;
            if (actualRemote.TryGetValue(tick, out prior))
            {
                if (prior != input) throw new InvalidOperationException("Conflicting input for one tick.");
                return;
            }
            actualRemote[tick] = input;
            while (actualRemote.ContainsKey(confirmedThrough + 1)) confirmedThrough++;

            Frame frame;
            if (tick <= state.Tick)
            {
                if (!frames.TryGetValue(tick, out frame))
                    throw new InvalidOperationException("Late input exceeded rollback history.");
                if (frame.AppliedRemote != input) ReplayFrom(tick);
            }
            PruneConfirmedHistory();
        }

        private void ReplayFrom(int tick)
        {
            int formerLastTick = state.Tick;
            Frame first = frames[tick];
            state = first.Before.Copy();
            Buttons previous = first.LastRemoteBefore;
            for (int t = tick; t <= formerLastTick; t++)
            {
                Frame frame = frames[t];
                Buttons remote;
                if (!actualRemote.TryGetValue(t, out remote)) remote = previous;
                frame.Before = state.Copy();
                frame.LastRemoteBefore = previous;
                frame.AppliedRemote = remote;
                Simulation.Step(state, frame.Local, remote);
                previous = remote;
                if (state.Result != MatchResult.Fighting) break;
            }
            lastRemote = previous;
            // If a corrected result ends the encounter earlier, discard frames
            // from the old future. They were never confirmed gameplay.
            for (int t = state.Tick + 1; t <= formerLastTick; t++) frames.Remove(t);
        }

        private void PruneConfirmedHistory()
        {
            // Keep one preceding frame for replays near the confirmed edge.
            int removeThrough = confirmedThrough - 2;
            for (int tick = prunedThrough + 1; tick <= removeThrough; tick++)
            {
                frames.Remove(tick);
                actualRemote.Remove(tick);
            }
            prunedThrough = Math.Max(prunedThrough, removeThrough);
        }
    }
}
