using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationInstantiatedButton : MonoBehaviour
{
    public Decoration decoration;
    Button button;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(Decoration deco) {
        decoration = deco;
        button.onClick.AddListener(OnButtonClick);
        button.onClick.AddListener(() => FindAnyObjectByType<DecorationUI>().RefreshDecorationButtonListSelection()); // Nechuù·rna ale nevÌm jak jinak aktualizovat ty outline vöech tlaËÌtek
        text.text = decoration.decorationPreset.Name;
    }

    void OnButtonClick() {
        AudioManager.Instance.PlaySound(SoundType.click);
    }


    public void onMoveToPosition() {
        // move camera to the gameObject
        print("trying to look at it");
        ICinemachineCamera activeCam = FindAnyObjectByType<CinemachineBrain>().ActiveVirtualCamera;
        print(activeCam.VirtualCameraGameObject.name);
        activeCam.VirtualCameraGameObject.transform.position = decoration.transform.position + new Vector3(5f, 5f, 0);
        StartCoroutine(LookAtTimeOut(activeCam));
    }

    IEnumerator LookAtTimeOut(ICinemachineCamera activeCam) {
        activeCam.LookAt = decoration.transform;
        yield return new WaitForSeconds(0.2f);
        activeCam.LookAt = null;
    }

    public void onPrejmenovat() {
        PopUpTextInput.Instance.AskForInput("P¯ejmenovat dekoraci ve scÈnÏ", (input) => {
            if (input != null)
                DecorationManager.Instance.RenameSelectedDecoration(input);
        });
    }

    public void onOdstranit() {
      //  DecorationManager.Instance.DeleteSelectedDecoration();
    }
}
