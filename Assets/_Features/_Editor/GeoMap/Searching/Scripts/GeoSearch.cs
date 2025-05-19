using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeoSearch : MonoBehaviour {

    [Header("UI References")]
    public GameObject resultButtonPrefab;
    public GameObject resultButtonContainerRefference;

    [Header("Search Settings")]
    public string preferredLanguage = "cs";
    public int resultLimit = 3;
    public bool includeAddressDetails = true;

    [Header("Debounce Settings")]
    public float debounceTime = 2f;

    private string pendingQuery;
    private Coroutine debounceCoroutine;
    private GeoSearchResultButton lastTopResult;

    public void OnSearch(string value) {
        pendingQuery = value;

        if (debounceCoroutine != null)
            StopCoroutine(debounceCoroutine);

        debounceCoroutine = StartCoroutine(DebounceSearch());
    }

    private IEnumerator DebounceSearch() {
        yield return new WaitForSeconds(debounceTime);

        if (!string.IsNullOrEmpty(pendingQuery)) {
            OnlineMapsTextWebService request = OnlineMapsOSMNominatim.Search(
                pendingQuery,
                preferredLanguage,
                resultLimit,
                includeAddressDetails
            );

            request.OnComplete += OnSearchCompleted;
        }

        debounceCoroutine = null;
    }

    private void OnSearchCompleted(string response) {
        OnlineMapsOSMNominatimResult[] results = OnlineMapsOSMNominatim.GetResults(response);
        if (results == null || results.Length == 0) {
            Debug.LogWarning("No results found.");
            lastTopResult = null;
            return;
        }

        ClearResults();

        lastTopResult = null;

        for (int i = 0; i < Mathf.Min(resultLimit, results.Length); i++) {
            var result = results[i];
            GeoSearchResultButton resultButton = Instantiate(resultButtonPrefab, resultButtonContainerRefference.transform).GetComponent<GeoSearchResultButton>();
            resultButton.SetupButton(result.latitude, result.longitude, result.display_name);

            if (i == 0)
                lastTopResult = resultButton;
        }
    }

    public void ClearResults() {
        foreach (Transform child in resultButtonContainerRefference.transform) {
            Destroy(child.gameObject);
        }
    }

    public void SubmitTopResult() {
        if (lastTopResult != null) {
            lastTopResult.OnClick();
        } else {
            Debug.LogWarning("No top result available to submit.");
        }
    }
}
