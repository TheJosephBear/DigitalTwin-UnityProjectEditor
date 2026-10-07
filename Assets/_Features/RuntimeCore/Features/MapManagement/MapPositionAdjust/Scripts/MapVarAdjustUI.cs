using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapVarAdjustUI : MonoBehaviour { // Or UIBehaviour depending on your base class

    public TMP_InputField posXInput, posYInput, posZInput;
    public TMP_InputField rotXInput, rotYInput, rotZInput;

    // This flag stops the infinite event loop
    private bool _isUpdatingUIFromCode = false;

    private void Update() {
        UpdateTransformValues();
    }

    public void UpdateTransformValues() {
        var manager = MapVariantAdjustManager.Instance;
        if (manager == null || manager.GetCopiedVariant() == null) return;

        Vector3 pos = manager.GetPosition();
        Vector3 rot = manager.GetRotationEuler();

        // Safety check: If the manager already has NaN, don't let it touch the UI strings
        if (float.IsNaN(pos.x) || float.IsNaN(rot.x)) return;

        // Set the flag to true so our listeners know to ignore these changes
        _isUpdatingUIFromCode = true;

        if (!posXInput.isFocused) posXInput.text = pos.x.ToString("F2");
        if (!posYInput.isFocused) posYInput.text = pos.y.ToString("F2");
        if (!posZInput.isFocused) posZInput.text = pos.z.ToString("F2");

        if (!rotXInput.isFocused) rotXInput.text = rot.x.ToString("F2");
        if (!rotYInput.isFocused) rotYInput.text = rot.y.ToString("F2");
        if (!rotZInput.isFocused) rotZInput.text = rot.z.ToString("F2");

        // Done updating, allow user inputs to pass through again
        _isUpdatingUIFromCode = false;
    }

    public void OnFinished() {
        MapVariantAdjustManager.Instance.ExitAdjusting(saveChanges: true);
    }

    public void OnCancel() {
        MapVariantAdjustManager.Instance.ExitAdjusting(saveChanges: false);
    }

    #region Position and Rotation Inputs

    private void ApplySafePosition() {
        // IF WE GENERATED THIS CHANGE VIA CODE, STOP HERE!
        if (_isUpdatingUIFromCode) return;

        bool isXValid = float.TryParse(posXInput.text, out float x);
        bool isYValid = float.TryParse(posYInput.text, out float y);
        bool isZValid = float.TryParse(posZInput.text, out float z);

        if (isXValid && isYValid && isZValid) {
            MapVariantAdjustManager.Instance.UpdatePosition(new Vector3(x, y, z));
        }
    }

    public void OnTransformPositionChangedX(string _) => ApplySafePosition();
    public void OnTransformPositionChangedY(string _) => ApplySafePosition();
    public void OnTransformPositionChangedZ(string _) => ApplySafePosition();

    private float TryParseOr(float fallback, string input) {
        if (string.IsNullOrEmpty(input)) return fallback;
        return float.TryParse(input, out float val) ? val : fallback;
    }

    private void ApplySafeRotation() {
        // IF WE GENERATED THIS CHANGE VIA CODE, STOP HERE!
        if (_isUpdatingUIFromCode) return;

        Vector3 current = MapVariantAdjustManager.Instance.GetRotationEuler();

        // Extra check: if current rotation is already broken, default to 0
        if (float.IsNaN(current.x)) current = Vector3.zero;

        float x = TryParseOr(current.x, rotXInput.text);
        float y = TryParseOr(current.y, rotYInput.text);
        float z = TryParseOr(current.z, rotZInput.text);

        MapVariantAdjustManager.Instance.UpdateRotation(new Vector3(x, y, z));
    }

    public void OnTransformRotationChangedX(string _) => ApplySafeRotation();
    public void OnTransformRotationChangedY(string _) => ApplySafeRotation();
    public void OnTransformRotationChangedZ(string _) => ApplySafeRotation();

    #endregion
}