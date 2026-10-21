using UnityEngine;
namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        private AudioSource impactAudio;
        private AudioClip hitSound, allySound;
        private bool muted;
        private void UpdateAudio() {
            if (Input.GetKeyDown(KeyCode.M)) muted = !muted;
        }
        private void PlayImpact(bool friendly, int damage) {
            if (automatedTest || muted || Application.isBatchMode) return;
            if (impactAudio == null) {
                impactAudio = gameObject.AddComponent<AudioSource>();
                if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
                hitSound = CreateTone("Impact", 90, 0.18f); allySound = CreateTone("AllyWarning", 360, 0.12f);
            }
            impactAudio.PlayOneShot(friendly ? allySound : hitSound, Mathf.Clamp(damage / 100f, 0.2f, 0.6f));
        }
        private static AudioClip CreateTone(string name, float pitch, float duration) {
            const int rate = 22050; float[] samples = new float[(int)(rate * duration)];
            uint noise = 7;
            for (int i = 0; i < samples.Length; i++) {
                float t = i / (float)rate, envelope = Mathf.Exp(-t * 32) * Mathf.Min(1, t * 1000);
                noise = noise * 1664525 + 1013904223;
                samples[i] = (Mathf.Sin(t * pitch * Mathf.PI * 2) * 0.7f + ((noise >> 16) / 32768f - 1) * 0.3f) * envelope;
            }
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
    }
}
