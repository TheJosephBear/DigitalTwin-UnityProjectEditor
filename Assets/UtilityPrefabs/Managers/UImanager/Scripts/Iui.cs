using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBehaviour : MonoBehaviour {
    // Animation + sound + maybe effect, everyone will handle it similarly
    public GameObject canvas;
    public abstract void Show();
    public abstract void Hide();
}
