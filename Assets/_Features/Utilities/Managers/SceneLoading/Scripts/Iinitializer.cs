using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Iinitializer {
    // Each scene has an initializer that decides what happens once the scene is loaded
    // Initialization happens after external managers decides that the scene is loaded AND shown
    public void Initialize();
    public void StartRunning();
    public void Unload();
}
