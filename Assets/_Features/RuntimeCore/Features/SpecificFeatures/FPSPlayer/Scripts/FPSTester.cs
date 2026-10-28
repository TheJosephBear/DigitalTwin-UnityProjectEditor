using UnityEngine;

public class FPSTester : MonoBehaviour
{
   public void OnFPS() {
        FPSManager.Instance.ToggleUI(true);
    }
}
