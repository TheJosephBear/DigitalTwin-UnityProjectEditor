using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundBoard : Singleton<SoundBoard> {

    public List<Sound> sounds = new List<Sound>();

    protected override void Awake() {
        base.Awake();
    }

    public Sound GetSound(SoundType soundType) {
        foreach (Sound sound in sounds) {
            if (sound.soundName == soundType.ToString())
                return sound;
        }
        Debug.LogError("Sound named " + soundType + " aint in the databse chief...");
        return null;
    }
}
