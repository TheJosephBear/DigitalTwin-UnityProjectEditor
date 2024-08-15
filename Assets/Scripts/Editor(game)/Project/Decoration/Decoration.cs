using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour {
    /// <summary>
    /// Decoration instance spawned in scene, can be voted on
    /// </summary>
    
    public DecorationPreset decorationPreset;
    public int decorationVariantIdx;

}



[Serializable]
public class SerializableDecoration {
    public string decorationPresetName;
    public string modelAssetID;
    public Vector3 position;
}