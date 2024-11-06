using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestPoint : MonoBehaviour {

    public GameObject vcam;
    public string Name { get; private set; }
    
    public void Rename(string newName) {
        Name = newName;
    }

}
