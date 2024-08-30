using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;

public class MapUI : UIBehaviour {

    public GameObject canvas;

    public override void Show() {
        canvas.SetActive(true);
    }

    public override void Hide() {
        canvas.SetActive(false);
    }
}