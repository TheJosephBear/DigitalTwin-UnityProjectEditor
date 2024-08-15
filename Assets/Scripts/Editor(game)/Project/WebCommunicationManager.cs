using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WebCommunicationManager : Singleton<WebCommunicationManager> {

    public string serverUrlUploadJson = "http://127.0.0.1:5000/upload";
    public string serverUrlUploadFiles = "http://127.0.0.1:5000/uploadFiles";
    public string serverUrlDownload = "http://127.0.0.1:5000/download";

    public void StartUpload(string data) {
        StartCoroutine(UploadDataCoroutine(data));
    }

    public void StartDownload(System.Action<string> callback) {
        StartCoroutine(DownloadDataCoroutine(callback));
    }

    IEnumerator UploadDataCoroutine(string data) {
        // Create form data
        WWWForm form = new WWWForm();
        form.AddField("myData", data);

        // Send request
        UnityWebRequest www = UnityWebRequest.Post(serverUrlUploadJson, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error uploading data: " + www.error);
        } else {
            Debug.Log("Data uploaded successfully!");
            Debug.Log("Server response: " + www.downloadHandler.text);
        }
    }

    IEnumerator DownloadDataCoroutine(System.Action<string> callback) {
        UnityWebRequest www = UnityWebRequest.Get(serverUrlDownload);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error downloading data: " + www.error);
            callback(null);
        } else {
            Debug.Log("Data downloaded successfully!");
            callback(www.downloadHandler.text);
        }
    }

    // Uploads any .obj, .txt, .json, ... file to my server
    public void UploadFileToServer(string path, string fileName) {
        StartCoroutine(UploadFileCoroutine(path, fileName));
    }

    IEnumerator UploadFileCoroutine(string path, string fileName) {
        // Read the file data into a byte array
        byte[] fileData = File.ReadAllBytes(path);

        // Create form data
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, fileName, "application/octet-stream");

        // Send the request
        UnityWebRequest www = UnityWebRequest.Post(serverUrlUploadFiles, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error uploading file: " + www.error); ;
        } else {
            Debug.Log("File uploaded successfully!");
            Debug.Log("Server response: " + www.downloadHandler.text);
        }
    }
}
