using UnityEngine;

public class ViewerHUDUI : UIBehaviour {
    public override void Show() {
        base.Show();
        //    GetComponent<DecorationUI>().ToggleVariantUI(false);
        UIManager.Instance.SetRaycasterFromLatestUI();
    }


}
