using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationInstantiated : MonoBehaviour {
    /// <summary>
    /// Decoration instance spawned in scene
    /// </summary>
    
    public string Name { get; private set; }
    public DecorationPreset decorationPreset;
    public DecorationVariant decorationVariant;

    public void SetName(string name) {
        Name = name;
    }

}