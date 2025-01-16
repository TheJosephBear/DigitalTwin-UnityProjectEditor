using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditorObjectInfoUI : UIBehaviour {

    public GameObject ButtonPrefab;
    public GameObject ContentRefference;
    public TextMeshProUGUI TitleRefference;
    public TextMeshProUGUI InstanceNameRefference;
    public TextMeshProUGUI DescriptionRefference;

    void AddButtonToList<T>(T instance) where T : EditorObjectBase {
        GameObject newButton = Instantiate(ButtonPrefab, ContentRefference.transform);
        newButton.GetComponent<EditorObjectButton>().EditorObjectInstance = instance;
    }
    
    public void SetTitle(string text) {
        TitleRefference.text = text;
    }

    public void FillList<T>(List<T> list) where T : EditorObjectBase {
        foreach (var item in list) {
            AddButtonToList(item);
        }
    }

    public void FillInstanceInfo<T>(T instance) where T : EditorObjectBase {
        SetInstanceName(instance.Name);
        SetDescription(instance.Description);
    }

    void SetInstanceName(string text) {
        InstanceNameRefference.text = text;

    }

    void SetDescription(string text) {
        DescriptionRefference.text = text;
    }



}
