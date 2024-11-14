using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityUI {

    public static void Fade(GameObject uiElement, bool fadeIn, float duration = 1f, bool disableOnFadeOut = true) {
        if(fadeIn) uiElement.SetActive(true);
        
        CanvasGroup canvasGroup = uiElement.GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = uiElement.AddComponent<CanvasGroup>();
        }
        // if u wanna fade out an element that isnt active then ur dumb
        if(!(!fadeIn && !uiElement.activeSelf)) uiElement.GetComponent<MonoBehaviour>().StartCoroutine(FadeCoroutine(uiElement, canvasGroup, fadeIn, duration, disableOnFadeOut));
    }

    static IEnumerator FadeCoroutine(GameObject uiElement, CanvasGroup canvasGroup, bool fadeIn, float duration, bool disableOnFadeOut) {
        float startAlpha = canvasGroup.alpha;
        float endAlpha = fadeIn ? 1f : 0f;
        float timeElapsed = 0f;

        while (timeElapsed < duration) {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        if(disableOnFadeOut && endAlpha == 0f) {
            uiElement.SetActive(false);
        }
    }
}
