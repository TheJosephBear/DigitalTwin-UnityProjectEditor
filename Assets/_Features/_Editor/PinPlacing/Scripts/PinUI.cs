using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PinUI: MonoBehaviour {
    [Header("List References")]
    [SerializeField] private Transform scrollContentParent;
    [SerializeField] private GameObject pinButtonPrefab;
    [SerializeField] private GameObject addPinButton; // Prefab or existing "+" button at bottom

    [Header("Detail Inspector References")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField descriptionInput;
    [SerializeField] private Image iconPreview;
    [SerializeField] private Button movePinButton;

    private PinManager pinManager;
    private PinObject currentInspectedPin;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    public void Initialize(PinManager manager) {
        pinManager = manager;
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }

    public void UpdatePinList(List<PinObject> pins) {
        ClearList();

        foreach (PinObject pin in pins) {
            GameObject btnObj = Instantiate(pinButtonPrefab, scrollContentParent);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = pin.Name;

            PinObject currentPin = pin;
            btnObj.GetComponent<Button>().onClick.AddListener(() => {
                pinManager.SelectPin(currentPin);
            });

            spawnedButtons.Add(btnObj);
        }

        // Always keep the "+" button as the last element in the Scroll Content
        if (addPinButton != null) {
            addPinButton.transform.SetAsLastSibling();
            addPinButton.SetActive(true);
        }
    }

    private void ClearList() {
        foreach (GameObject btn in spawnedButtons) {
            Destroy(btn);
        }
        spawnedButtons.Clear();
    }

    public void ShowDetails(PinObject pin) {
        currentInspectedPin = pin;
        if (detailsPanel != null) detailsPanel.SetActive(true);

        if (nameInput != null) nameInput.text = pin.Name;
        if (descriptionInput != null) descriptionInput.text = pin.Description;
        if (iconPreview != null) {
            iconPreview.sprite = pin.Image;
            iconPreview.gameObject.SetActive(pin.Image != null);
        }
    }

    public void HideDetails() {
        currentInspectedPin = null;
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }

    // --- Inspector Event Handlers ---

    public void OnNameChanged(string newName) {
        if (currentInspectedPin != null) {
            currentInspectedPin.Name = newName;
            UpdatePinList(pinManager.GetPins());
        }
    }

    public void OnDescriptionChanged(string newDesc) {
        if (currentInspectedPin != null) {
            currentInspectedPin.Description = newDesc;
        }
    }

    public void OnAddButtonClicked() {
        pinManager.CreateNewPin();
    }

    public void OnStartMovingClicked() {
        if (currentInspectedPin != null) {
            pinManager.StartMovingPin(currentInspectedPin);
        }
    }
}
