using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ElevationResult {
    public float latitude;
    public float longitude;
    public float elevation;
}

[Serializable]
public class ElevationResponse {
    public ElevationResult[] results;
}

public class ElevationFetcher : Singleton<ElevationFetcher> {
    private const string apiUrl = "https://api.open-elevation.com/api/v1/lookup";

    public void GetElevation(Vector2 latLon, Action<float> onSuccess, Action<string> onError = null) {
        StartCoroutine(GetElevationRoutine(new List<Vector2> { latLon }, result => {
            print(result);
            if (result.Length > 0) onSuccess?.Invoke(result[0].elevation);
            else onError?.Invoke("No elevation data returned.");
        }, onError));
    }

    public void GetElevations(List<Vector2> latLons, Action<ElevationResult[]> onSuccess, Action<string> onError = null) {
        StartCoroutine(GetElevationRoutine(latLons, onSuccess, onError));
    }

    private IEnumerator GetElevationRoutine(List<Vector2> latLons, Action<ElevationResult[]> onSuccess, Action<string> onError) {
        if (latLons == null || latLons.Count == 0) {
            onError?.Invoke("No coordinates provided.");
            yield break;
        }
        
        string locations = string.Join("|", latLons.ConvertAll(ll =>
    $"{ll.y.ToString(CultureInfo.InvariantCulture)},{ll.x.ToString(CultureInfo.InvariantCulture)}"));
        string fullUrl = $"{apiUrl}?locations={locations}";

        print("fullUrl url: "+ fullUrl);
        using UnityWebRequest request = UnityWebRequest.Get(fullUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success){
            onError?.Invoke($"Error fetching elevation: {request.error}");
        } else {
            try {
                print("raw response: " + request.downloadHandler.text);
                var response = JsonUtility.FromJson<ElevationResponse>(request.downloadHandler.text);
                onSuccess?.Invoke(response.results);
            } catch (Exception ex) {
                onError?.Invoke($"Parsing error: {ex.Message}");
            }
        }
    }
}
