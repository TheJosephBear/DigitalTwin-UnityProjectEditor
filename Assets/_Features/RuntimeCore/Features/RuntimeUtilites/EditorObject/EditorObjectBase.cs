using UnityEngine;

public abstract class EditorObjectBase : MonoBehaviour {
    /// <summary>
    /// Base for editor objects (instances of maps, interest points, decorations, ...)
    /// Attributes for similiar UI showcase
    /// </summary>

    public string ID { get; protected set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
  //  public EOManagerBase<T> managerRefference; // where T je něco, ale je to vždycky něco jiného

    public void SetName(string newName) { 
        Name = newName; //  Utilities.UniqueNameEnsure(newName, list)
    }

    public void SetDescription(string newDescription) {
        Description = newDescription;
    }

    private void OnEnable() {
        ID = System.Guid.NewGuid().ToString();
    }
}
