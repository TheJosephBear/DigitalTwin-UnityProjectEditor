using UnityEngine;

public abstract class EditorObjectBase : MonoBehaviour {
    /// <summary>
    /// Base for editor objects (instances of maps, interest points, decorations, ...)
    /// Attributes for similiar UI showcase
    /// </summary>

    public string Name { get; private set; }
    public string Description { get; private set; }
  //  public EOManagerBase<T> managerRefference; // where T je nìco, ale je to vždycky nìco jiného

    public void SetName(string newName) { 
        Name = newName; //  Utilities.UniqueNameEnsure(newName, list)
    }

    public void SetDescription(string newDescription) {
        Description = newDescription;
    }

}
