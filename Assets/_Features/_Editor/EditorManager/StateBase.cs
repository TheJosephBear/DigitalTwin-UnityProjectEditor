using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase : MonoBehaviour {
    public ProjectState State;
    public abstract void Enter();
    public abstract void Exit();
}
