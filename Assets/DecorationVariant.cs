using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationVariant {

    public string Name {get; private set;}
    public ModelAsset Model;

    public DecorationVariant(string name, ModelAsset model) {
        Name = name;
        Model = model;
    }

    public void SetName(string name) {
        Name = name;
    }
}
