using UnityEngine;
using System.Collections;

namespace BG_Lib.AndroidMediation.Scripts.sample
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class SampleScenePersistentAudio : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float ClipLengthSeconds = 4f;
        private const float MasterVolume = 0.18f;

        private static SampleScenePersistentAudio instance;

        [SerializeField] private AudioClip defaultClip;

        private AudioSource audioSource;
        private AudioClip loopClip;
        private Coroutine ensurePlayingCoroutine;

        public static SampleScenePersistentAudio Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            ForceEnableAudio();
            ConfigureAudioSource();
            EnsureClip();
        }

        private void Start()
        {
            RestartPlayback();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                RestartPlayback();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
                RestartPlayback();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;

            if (ensurePlayingCoroutine != null)
                StopCoroutine(ensurePlayingCoroutine);
        }

        public void PlayLoop()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            ForceEnableAudio();
            ConfigureAudioSource();
            EnsureClip();

            if (audioSource.isPlaying)
                return;

            audioSource.Play();
        }

        public void StopLoop()
        {
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();
        }

        private void ConfigureAudioSource()
        {
            if (audioSource == null)
                return;

            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
            audioSource.ignoreListenerPause = true;
            audioSource.ignoreListenerVolume = true;
            audioSource.bypassEffects = false;
            audioSource.bypassListenerEffects = false;
            audioSource.bypassReverbZones = true;
        }

        private void EnsureClip()
        {
            if (defaultClip != null)
            {
                audioSource.clip = defaultClip;
                return;
            }

            if (loopClip != null)
            {
                audioSource.clip = loopClip;
                return;
            }

            int totalSamples = Mathf.CeilToInt(SampleRate * ClipLengthSeconds);
            float[] samples = new float[totalSamples];
            float[] noteFrequencies = { 261.63f, 329.63f, 392.0f, 329.63f };
            int samplesPerNote = totalSamples / noteFrequencies.Length;

            for (int i = 0; i < totalSamples; i++)
            {
                float time = i / (float)SampleRate;
                int noteIndex = Mathf.Min(noteFrequencies.Length - 1, i / samplesPerNote);
                float noteFrequency = noteFrequencies[noteIndex];
                float notePosition = (i % samplesPerNote) / (float)samplesPerNote;
                float fade = Mathf.Clamp01(Mathf.Min(notePosition * 8f, (1f - notePosition) * 10f));
                float baseWave = Mathf.Sin(2f * Mathf.PI * noteFrequency * time);
                float harmonicWave = Mathf.Sin(2f * Mathf.PI * noteFrequency * 2f * time) * 0.25f;
                float padWave = Mathf.Sin(2f * Mathf.PI * 130.81f * time) * 0.18f;
                samples[i] = (baseWave + harmonicWave + padWave) * fade * MasterVolume;
            }

            loopClip = AudioClip.Create("SampleSceneDefaultLoop", totalSamples, 1, SampleRate, false);
            loopClip.SetData(samples, 0);
            audioSource.clip = loopClip;
        }

        private void RestartPlayback()
        {
            PlayLoop();

            if (ensurePlayingCoroutine != null)
                StopCoroutine(ensurePlayingCoroutine);

            ensurePlayingCoroutine = StartCoroutine(EnsurePlayingRoutine());
        }

        private IEnumerator EnsurePlayingRoutine()
        {
            for (int i = 0; i < 20; i++)
            {
                ForceEnableAudio();

                if (audioSource != null && !audioSource.isPlaying)
                    audioSource.Play();

                yield return null;
            }

            ensurePlayingCoroutine = null;
        }

        private static void ForceEnableAudio()
        {
            AudioListener.pause = false;
            AudioListener.volume = 1f;
            AudioSettings.Mobile.stopAudioOutputOnMute = false;
        }
    }
}
