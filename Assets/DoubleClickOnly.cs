using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class DoubleClickOnly : MonoBehaviour {
    public float doubleClickTime = 0.3f;
    public UnityEngine.Events.UnityEvent onDoubleClick; 

    private Button button;
    private float lastClickTime = 0f;

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    void HandleClick() {
        float currentTime = Time.time;
        if (currentTime - lastClickTime < doubleClickTime) {
            // Double-click detected
            onDoubleClick.Invoke();
        }
        lastClickTime = currentTime;
    }
}