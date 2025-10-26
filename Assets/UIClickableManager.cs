using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickableManager : Singleton<UIClickableManager>
{

    public event Action<List<GameObject>> OnUIClicked;

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            CheckUIClick();
        }
    }

    private void CheckUIClick() {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0) {
            List<GameObject> clickedObjects = new List<GameObject>();
            foreach (var r in results)
                clickedObjects.Add(r.gameObject);

            OnUIClicked?.Invoke(clickedObjects);
        }
    }
}
