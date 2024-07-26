using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBoard : MonoBehaviour {
    public static SoundBoard Instance { get; private set; }
    public List<Sound> sounds = new List<Sound>();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }
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
