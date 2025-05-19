using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeoLocalizationUI : UIBehaviour {

    public TextMeshProUGUI InfoTextReff;
    public TMP_InputField ZoomScaleInputFieldReff;
    public Movable Basemapreff;

    public void PrintInfo(string text) {
        InfoTextReff.text = text;
    }

    public void ZoomToScale() {
        float zoomScale;
        if (float.TryParse(ZoomScaleInputFieldReff.text, out zoomScale)) {
            GeoMapManager.Instance.ZoomToFitScale(zoomScale, 1f);
        }
    }

    public void RotPos() {
        if (Basemapreff.MovableType == MovableType.Rotation) {
            Basemapreff.MovableType = MovableType.Position;
            Basemapreff.ShownAxis = GizmoAxis.All;
        } else {
            Basemapreff.MovableType = MovableType.Rotation;
            Basemapreff.ShownAxis = GizmoAxis.Y;
        }
        Basemapreff.OnClickDown();
        Basemapreff.OnClickDown();
    }

    public void OnLockGeoMap() {
        GeoMapLocalizationManager.Instance.LockGeoMap();
    }

    public void OnPlaceMapModel() {
        GeoMapLocalizationManager.Instance.PlaceMapModel();
    }

    public void OnExit() {
        GeoMapManager.Instance.ExitGeoLocalization();
    }

}
