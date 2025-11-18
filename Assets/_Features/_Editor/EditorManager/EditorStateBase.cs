using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EditorStateBase : MonoBehaviour {
    public EditorState State;
    public abstract void Enter();
    public abstract void Exit();
}
