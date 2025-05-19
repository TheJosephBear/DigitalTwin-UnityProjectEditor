using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorObjectManager : Singleton<EditorObjectManager> {
    /// <summary>
    /// Manages editorobjectUI visibility and content
    /// </summary> 

    EditorObjectInfoUI UI;


    public void ToggleUI(bool show) {
        if (UI == null) {
            UI = FindAnyObjectByType<EditorObjectInfoUI>();
        }
   //     UImanager.Instance.ToggleUI(UIType.EditorObjectInfoUI, show);
    }

    // fill ui with given data (of abstract class)
  /*  public void FillEditorObjectListUI<T>(List<T> objectList, string title) where T : EditorObjectBase {
        UI.SetTitle(title);
        UI.FillList(objectList);
    }
  */

    public void FillEditorObjectListUI(List<EditorObjectBase> objectList, string title) {
        UI.SetTitle(title);
        UI.FillList(objectList);
    }

    public void FillInstanceInfoUI<T>(T editorObject) where T : EditorObjectBase  {
        UI.FillInstanceInfo(editorObject);
    }

}