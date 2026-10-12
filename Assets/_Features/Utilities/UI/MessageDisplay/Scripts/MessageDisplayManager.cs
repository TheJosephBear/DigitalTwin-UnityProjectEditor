public class MessageDisplayManager: Singleton<MessageDisplayManager> {

    public MessageDisplayUI uiScript;

    public void DisplayMessage(string message, float duration = 5f) {
        uiScript.ShowMessage(message, duration);
    }

    public void ShowMessage(string message, float duration = 5f) {
        DisplayMessage(message, duration);
    }
}
