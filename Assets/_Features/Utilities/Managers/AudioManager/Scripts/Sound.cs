using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "JosephBearScriptables/Sound")]
public class Sound : ScriptableObject {
    public string soundName;
    public MixerType mixerType;
    public AudioClip clip;
    public bool loop = false;
    public float volume = 1.0f;
    public float pitch = 1.0f;
}

public enum MixerType {
    SoundEffect,
    Theme,
    EnvironmentAmbience,
    EnvironmentEffect
}