using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EOManagerBase<T> : MonoBehaviour where T : EditorObjectBase {

    protected List<T> instanceList = new List<T>();

    public void AddInstance(T instance) {
        if (!instanceList.Contains(instance)) {
            instanceList.Add(instance);
        }
    }

    public void RemoveInstance(T instance) {
        if (instanceList.Contains(instance)) {
            instanceList.Remove(instance);
        }
    }

    public List<T> GetInstances() {
        return instanceList;
    }

}
