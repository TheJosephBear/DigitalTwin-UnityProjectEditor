using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPauseManager : MonoBehaviour {
    public static AudioPauseManager Instance { get; private set; }

    List<AudioSource> pausedAudioSources;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        pausedAudioSources = new List<AudioSource>();
        }
    }

    public void PauseAllSounds(List<AudioSource> allAudioSources) {
        pausedAudioSources.Clear();
        foreach (AudioSource source in allAudioSources) {
            if (source != null) {
                if (source.isPlaying) {
                    source.Pause();
                    pausedAudioSources.Add(source);
                }
            }
        }
    }

    public void ResumeAllSounds() {
        foreach (AudioSource source in pausedAudioSources) {
            if (source != null) {
                source.UnPause();
            }
        }
        pausedAudioSources.Clear();
    }

    public void PauseSound(AudioSource source) {
        if (source.isPlaying) {
            if (source != null) {
                source.Pause();
                pausedAudioSources.Add(source);
            }
        }
    }

    public void ResumeSound(AudioSource source) {
        if (pausedAudioSources.Contains(source)) {
            if (source != null) {
                source.UnPause();
                pausedAudioSources.Remove(source);
            }
        }
    }
}
