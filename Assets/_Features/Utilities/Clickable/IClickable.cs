using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClickable {
    public void OnClickDown();
    public void OnClick();
    public void OnClickUp();
    public void OnHover();
    public void OnUnhover();
}
