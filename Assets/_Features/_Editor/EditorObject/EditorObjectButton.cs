using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorObjectButton : MonoBehaviour {

    public EditorObjectBase EditorObjectInstance; // refference of the button

    
    public void onPridatNovy() {
        // řekni si manageru at prida
    }

    public void onVybrat() {
        EditorObjectManager.Instance.FillInstanceInfoUI(EditorObjectInstance);
    }

    public void onPrejmenovat() {
        PopUp.Instance.AskForInput("Zadejte nový název", (input) => {
     //       EditorObjectInstance.SetName(Utilities.UniqueNameEnsure(input));
        });
    }

    public void onSmazat() {
        // rekni si manageru at odebere

    }
}
