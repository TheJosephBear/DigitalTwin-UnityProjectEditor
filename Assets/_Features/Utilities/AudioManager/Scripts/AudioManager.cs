using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager> {

    public AudioMixer audioMixer;
    Dictionary<SoundType, AudioSource> activeLoopingSources;
    List<AudioSource> allAudioSources;
    List<AudioSource> pausedAudioSources;

    protected override void Awake() {
        base.Awake();
        activeLoopingSources = new Dictionary<SoundType, AudioSource>();
        allAudioSources = new List<AudioSource>();
        pausedAudioSources = new List<AudioSource>();
    }

    public void PlaySound(SoundType soundType, float fadeInDuration = 0f, Vector3 position = default(Vector3), bool is3D = false) {
        Sound sound = SoundBoard.Instance.GetSound(soundType);
        if (sound != null) {
            GameObject soundGameObject = new GameObject("Sound_" + soundType);
            AudioSource source = soundGameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
            source.volume = 0f; // Start volume at 0 for fade-in
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.spatialBlend = is3D ? 1.0f : 0.0f;
            soundGameObject.transform.position = position;

            source.Play();
            allAudioSources.Add(source);

            if (fadeInDuration > 0) {
                StartCoroutine(FadeIn(source, sound.volume, fadeInDuration));
            } else {
                source.volume = sound.volume; // Set the final volume if no fade-in
            }

            if (!sound.loop) {
                Destroy(soundGameObject, sound.clip.length);
            } else {
                activeLoopingSources[soundType] = source;
            }
        } else {
            Debug.LogWarning("Sound not found: " + soundType);
        }
    }

    public void StopSound(SoundType soundType) {
        if (activeLoopingSources.ContainsKey(soundType)) {
            AudioSource source = activeLoopingSources[soundType];
            source.Stop();
            Destroy(source.gameObject);
            activeLoopingSources.Remove(soundType);
            allAudioSources.Remove(source);
        }
    }

    public void FadeOutSound(SoundType soundType, float duration) {
        if (activeLoopingSources.ContainsKey(soundType)) {
            AudioSource source = activeLoopingSources[soundType];
            StartCoroutine(FadeOutAndStop(source, duration));
        }
    }
    public void StopAllSounds(bool fade = false, float fadeDuration = 1f) {
        if (fade) {
            foreach (AudioSource source in new List<AudioSource>(allAudioSources)) {
                StartCoroutine(FadeOutAndStop(source, fadeDuration));
            }
            foreach (AudioSource source in new List<AudioSource>(pausedAudioSources)) {
                StartCoroutine(FadeOutAndStop(source, fadeDuration));
            }
        } else {
            foreach (AudioSource source in new List<AudioSource>(allAudioSources)) {
                source.Stop();
                Destroy(source.gameObject);
            }
            foreach (AudioSource source in new List<AudioSource>(pausedAudioSources)) {
                source.Stop();
                Destroy(source.gameObject);
            }
            activeLoopingSources.Clear();
            allAudioSources.Clear();
            pausedAudioSources.Clear();
        }
    }
    public void PauseAllSounds() {
        AudioPauseManager.Instance.PauseAllSounds(allAudioSources);
    }

    public void ResumeAllSounds() {
        AudioPauseManager.Instance.ResumeAllSounds();
    }

    public void PauseSound(SoundType soundType) {
        if (activeLoopingSources.ContainsKey(soundType)) {
            AudioSource source = activeLoopingSources[soundType];
            AudioPauseManager.Instance.PauseSound(source);
        }
    }

    public void ResumeSound(SoundType soundType) {
        if (activeLoopingSources.ContainsKey(soundType)) {
            AudioSource source = activeLoopingSources[soundType];
            AudioPauseManager.Instance.ResumeSound(source);
        }
    }

    IEnumerator FadeOutAndStop(AudioSource audioSource, float duration) {
        if (audioSource == null) yield break;
        float startVolume = audioSource.volume;
        float timeElapsed = 0f;
        while (audioSource != null && audioSource.volume > 0) {
            timeElapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, timeElapsed / duration);
            yield return null;
        }
        if (audioSource != null) {
            audioSource.Stop();
            Destroy(audioSource.gameObject);
            activeLoopingSources.Remove(ConvertStringToSoundType(audioSource?.clip?.name ?? ""));
            allAudioSources.Remove(audioSource); 
        }
    }

    SoundType ConvertStringToSoundType(string soundName) {
        SoundType result;
        if (Enum.TryParse(soundName, out result)) {
            return result;
        } else {
            Debug.LogWarning("SoundType not found for name: " + soundName);
            return default(SoundType);
        }
    }

    public AudioSource GetAudioSource(SoundType soundType) {
        if (activeLoopingSources.ContainsKey(soundType)) {
            return activeLoopingSources[soundType];
        }
        return null;
    }

    IEnumerator FadeIn(AudioSource audioSource, float targetVolume, float duration) {
        float timeElapsed = 0f;

        while (audioSource != null && timeElapsed < duration) {
            timeElapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, targetVolume, timeElapsed / duration);
            yield return null;
        }

        if (audioSource != null) {
            audioSource.volume = targetVolume; // Ensure final volume is set
        }
    }
}


