using System.Collections;
using TMPro;
using UnityEngine;

public class MessageDisplayUI: MonoBehaviour {
    public GameObject buttonPrefab;
    public GameObject container;

    public void ShowMessage(string message, float duration = 5f) {
        if (buttonPrefab == null || container == null) {
            Debug.LogError("Button prefab or container is not assigned!");
            return;
        }

        GameObject newButton = Instantiate(buttonPrefab, container.transform);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = message;
        StartCoroutine(FadeAndDestroy(newButton, duration));
    }

    private IEnumerator FadeAndDestroy(GameObject button, float duration) {
        yield return new WaitForSeconds(duration);

        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = button.AddComponent<CanvasGroup>();
        }

        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        Destroy(button);
    }
}
