using System;
using CoopFighter.Game;
using UnityEditor;
using UnityEngine;

namespace CoopFighter.UnityGame.Editor
{
    public static class ProjectValidation
    {
        [MenuItem("Co-op Fighter/Validate Project")]
        public static void Run()
        {
            if (EditorBuildSettings.scenes.Length == 0 ||
                EditorBuildSettings.scenes[0].path != "Assets/Scenes/Main.unity")
                throw new InvalidOperationException("Main scene is not in Build Settings.");

            MatchState original = Simulation.Start(1, null, 12345);
            for (int i = 0; i < 120; i++)
                Simulation.Step(original, i % 30 == 0 ? Buttons.High : Buttons.Right,
                    i % 35 == 0 ? Buttons.Low : Buttons.Left);

            NetworkEnvelope outbound = new NetworkEnvelope {
                kind = "state", tick = original.Tick, state = original.Copy(),
                hostButtons = (int)Buttons.Right
            };
            NetworkEnvelope inbound = JsonUtility.FromJson<NetworkEnvelope>(JsonUtility.ToJson(outbound));
            if (inbound == null || inbound.state == null ||
                inbound.state.Hash() != original.Hash())
                throw new InvalidOperationException("Network state JSON round trip changed gameplay state.");

            BattleTelemetry habits = new BattleTelemetry();
            habits.Attempts[0] = 10; habits.Low[0] = 8;
            habits.BossDamage[0] = 80; habits.BossDamage[1] = 20;
            CounterProfile transfer = CounterProfile.FromBattle(1, habits);
            CounterProfile restored = JsonUtility.FromJson<CounterProfile>(JsonUtility.ToJson(transfer));
            if (restored == null || restored.LowBlockWeight != transfer.LowBlockWeight ||
                restored.FocusPlayer != transfer.FocusPlayer)
                throw new InvalidOperationException("Boss transfer JSON round trip failed.");

            Debug.Log("Beaconbreak validation passed: scene, simulation snapshot JSON, and boss transfer JSON.");
        }
    }
}
