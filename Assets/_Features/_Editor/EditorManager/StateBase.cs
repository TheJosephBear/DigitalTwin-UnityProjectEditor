using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase : MonoBehaviour {
    public AppState State;
    public abstract void Enter();
    public abstract void Exit();
}
