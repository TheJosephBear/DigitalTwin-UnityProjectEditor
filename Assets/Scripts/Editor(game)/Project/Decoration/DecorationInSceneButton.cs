using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationInSceneButton : MonoBehaviour
{
    GameObject gameObjectRefference;
    DecorationPreset decorationPreset;
    Button button;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(GameObject go) {
        gameObjectRefference = go;
        decorationPreset = go.GetComponent<Decoration>().decorationPreset;
        button.onClick.AddListener(OnButtonClick);
        print(decorationPreset.Name);
        text.text = decorationPreset.Name;
    }

    void OnButtonClick() {
        // move camera to the gameObject
        print("trying to look at it");
        ICinemachineCamera activeCam = FindAnyObjectByType<CinemachineBrain>().ActiveVirtualCamera;
        print(activeCam.VirtualCameraGameObject.name);
        activeCam.VirtualCameraGameObject.transform.position = gameObjectRefference.transform.position + new Vector3(5f,5f,0);
        StartCoroutine(LookAtTimeOut(activeCam));
    }

    IEnumerator LookAtTimeOut(ICinemachineCamera activeCam) {
        activeCam.LookAt = gameObjectRefference.transform;
        yield return new WaitForSeconds(0.2f);
        activeCam.LookAt = null;
    }
}
