using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class ServerCommunicationManager : Singleton<ServerCommunicationManager> {

    [Tooltip("Base URL of the server. For WebGL builds, this will be overridden by the hosting page URL.")]
    public string serverUrl = "http://127.0.0.1:5000";

    // In WebGL builds, we need to get the server URL dynamically from the hosting page
    // The function comes from the JavaScript plugin defined in Assets/Plugins/WebGL/GetCurrentURL.jslib
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetCurrentURL();
#endif

    private void Awake() {
        base.Awake();
#if UNITY_WEBGL && !UNITY_EDITOR
        string currentUrl = GetCurrentURL();
        if (!string.IsNullOrEmpty(currentUrl)) {
            Uri uri = new Uri(currentUrl);
            serverUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            Debug.Log($"Server URL set to: {serverUrl}");
        }
#endif
        Debug.Log($"Using server URL: {serverUrl}");
    }

    public string GetWebGLURL() {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GetCurrentURL();
#else
        return Application.absoluteURL;
#endif
    }

    #region Login and Register

    public void Login(string username, string password, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/login";
        Dictionary<string, string> formData = new Dictionary<string, string> {
            { "username", username },
            { "password", password }
        };
        StartCoroutine(PostRequest(url, formData, callback));
    }

    public void Logout(System.Action<bool, string> callback) {
        string url = $"{serverUrl}/logout";
        StartCoroutine(PostRequest(url, new Dictionary<string, string>(), callback));
    }

    public void Register(string username, string password, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/register";
        Dictionary<string, string> formData = new Dictionary<string, string> {
            { "username", username },
            { "password", password }
        };
        StartCoroutine(PostRequest(url, formData, callback));
    }

    #endregion

    #region Projects

    public void CreateProject(string projectName, string projectId, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/createProject";

        Dictionary<string, string> formData = new Dictionary<string, string> {
            { "project_name", projectName },
            { "project_id", projectId }
        };

        StartCoroutine(PostRequest(url, formData, callback));
    }

    public void EditProjectName(string oldName, string newName, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/editProjectName";
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            { "oldProjectName", oldName },
            { "newProjectName", newName },
        };
        StartCoroutine(PostRequest(url, formData, callback));
    }

    public void EditProject(string oldName, string newName, string description, string imageID, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/editProject";
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            { "oldProjectName", oldName },
            { "projectName", newName },
            { "projectDescription", description },
            { "projectImageID", imageID }
        };
        StartCoroutine(PostRequest(url, formData, callback));
    }

    public void DuplicateProject(string name, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/duplicate_project";
        Dictionary<string, string> formData = new Dictionary<string, string> { { "project_name", name } };
        StartCoroutine(PostRequest(url, formData, callback));
    }

    public void DeleteProject(string projectName, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/deleteProject";
        Dictionary<string, string> formData = new Dictionary<string, string> { { "project_name", projectName } };
        StartCoroutine(DeleteRequest(url, formData, callback));
    }

    public void FetchAllProjects(System.Action<bool, ProjectMetadata[]> callback) {
        string url = $"{serverUrl}/getAllProjects";
        StartCoroutine(GetRequest<ProjectListResponse>(url, (success, data) => {
            callback(success, data.projects);
        }));
    }

    public void GenerateViewerIframe(string projectName, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/generate_iframe?project_name={UnityWebRequest.EscapeURL(projectName)}";
        StartCoroutine(GetRequest<IframeResponse>(url, (success, data) => {
            callback(success, data.iframe_code);
        }));
    }

    #endregion

    #region Editor

    // Upload project data (no models)
    public void StartProjectDataUpload(string data, string projectName) {
        string url = $"{serverUrl}/upload_editor_data";
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            { "myData", data },
            { "project_name", projectName }
        };
        StartCoroutine(PostRequest(url, formData, null));
    }

    // Upload survey data
    public void StartSurveyUpload(string surveyJson, string projectName) {
        string url = $"{serverUrl}/upload_survey_data";
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            { "survey_data", surveyJson },
            { "project_name", projectName }
        };
        StartCoroutine(PostRequest(url, formData, null));
    }

    // Download project data (no models)
    public void StartDataDownload(string projectName, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/download?project_name={UnityWebRequest.EscapeURL(projectName)}";
        //    StartCoroutine(GetRequest(url, callback));
        StartCoroutine(GetRequest<string>(url, (success, data) => {
            callback(success, data);
        }, true));
    }

    // Download survey data
    public void StartSurveyDownload(string projectName, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/download_survey_data?project_name={UnityWebRequest.EscapeURL(projectName)}";
        StartCoroutine(GetRequest<string>(url, (success, data) => {
            callback(success, data);
        }, true));
    }

    // Upload file into a project
    public void UploadFileToServer(string path, string fileName, string projectName, string assetHash) {
        string url = $"{serverUrl}/upload_model_files";
        StartCoroutine(UploadFileRequest(url, path, fileName, projectName, assetHash));
    }

    // Download file from a project
    public void DownloadFileFromServer(
    string projectName,
    string assetHash,
    string fileName,
    System.Action<byte[]> callback) {

        string url =
            $"{serverUrl}/downloadModels" +
            $"?project_name={UnityWebRequest.EscapeURL(projectName)}" +
            $"&asset_hash={UnityWebRequest.EscapeURL(assetHash)}" +
            $"&file_name={UnityWebRequest.EscapeURL(fileName)}";

        StartCoroutine(DownloadFileRequest(url, callback));
    }

    public void DownloadImageFromServer(
    string projectName,
    string assetHash,
    string fileName,
    System.Action<byte[]> callback) {

        string url =
            $"{serverUrl}/download_image_files" +
            $"?project_name={UnityWebRequest.EscapeURL(projectName)}" +
            $"&asset_hash={UnityWebRequest.EscapeURL(assetHash)}" +
            $"&file_name={UnityWebRequest.EscapeURL(fileName)}";

        StartCoroutine(DownloadFileRequest(url, callback));
    }

    public void UploadImageToServer(string path, string fileName, string projectName, string assetHash) {
        string url = $"{serverUrl}/upload_image_files";
        StartCoroutine(UploadFileRequest(url, path, fileName, projectName, assetHash));
    }

    public void DownloadPreviewImageFromServer(string projectName, System.Action<byte[]> callback) {
        string url = $"{serverUrl}/download_preview_image" +
                     $"?project_name={UnityWebRequest.EscapeURL(projectName)}";
        // print(url);
        StartCoroutine(DownloadFileRequest(url, callback));
    }

    public void UploadPreviewImageToServer(string path, string fileName, string projectName, string assetHash) {
        string url = $"{serverUrl}/upload_preview_image";
        StartCoroutine(UploadFileRequest(url, path, fileName, projectName, assetHash));
    }


    public void ListFilesForAsset(string projectName, string assetHash, System.Action<List<string>> callback) {
        string url =
            $"{serverUrl}/list_model_files" +
            $"?project_name={UnityWebRequest.EscapeURL(projectName)}" +
            $"&asset_hash={UnityWebRequest.EscapeURL(assetHash)}";

        StartCoroutine(ListFilesRequest(url, callback));
    }

    #endregion

    #region Private helper functions

    IEnumerator PostRequest(string url, Dictionary<string, string> formData, System.Action<bool, string> callback) {
     //   MessageDisplayManager.Instance.DisplayMessage("Post request");

        WWWForm form = new WWWForm();
        foreach (var field in formData) {
            form.AddField(field.Key, field.Value, System.Text.Encoding.UTF8);
        }
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        HandleResponse(www, callback);
    }

    public IEnumerator GetRequest<T>(string url, System.Action<bool, T> callback, bool returnAsString = false) where T : class {
   //     MessageDisplayManager.Instance.DisplayMessage("GetRequest");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            string rawText = www.downloadHandler.text;
            try {
                if (returnAsString) {

                    object response = rawText;
                    callback(true, (T)response);
                } else {
                    if (string.IsNullOrEmpty(rawText)) {
                        Debug.LogWarning("[Server] Raw text is null or empty. JsonUtility will return a null/default object.");
                    }

                    T response = JsonUtility.FromJson<T>(rawText);
                    callback(true, response);
                }
            } catch (System.InvalidCastException castEx) {
                // Specifically catch casting errors (common when returnAsString is true but T is not string)
                Debug.LogError($"[Server] Cast Exception: Cannot cast raw text to {typeof(T).Name}. " +
                               $"Ensure you are calling this method with <string> when returnAsString is true. \nError: {castEx.Message}");
                callback(false, default);
            } catch (System.Exception ex) {
                Debug.LogError($"[Server] General Exception during parsing/callback: {ex.GetType().Name}");
                Debug.LogError($"[Server] Stack Trace: {ex.StackTrace}");
                Debug.LogError($"[Server] Raw data that caused failure: {rawText}");
                callback(false, default);
            }
        } else {
            Debug.Log("Error fetching data: " + www.error);
            callback(false, default);
        }
    }

    IEnumerator DeleteRequest(string url, Dictionary<string, string> formData, System.Action<bool, string> callback) {
     //   MessageDisplayManager.Instance.DisplayMessage("DeleteRequest");

        WWWForm form = new WWWForm();
        foreach (var field in formData) {
            form.AddField(field.Key, field.Value, System.Text.Encoding.UTF8);
        }

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.method = UnityWebRequest.kHttpVerbDELETE;
        yield return www.SendWebRequest();

        HandleResponse(www, callback);
    }

    IEnumerator UploadFileRequest(string url, string path, string fileName, string projectName, string assetHash) {
    //    MessageDisplayManager.Instance.DisplayMessage("UploadFileRequest");

        if (path == null) {
            Debug.LogError("Upload file request err: PATH IS NULL");
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(path);
        WWWForm form = new WWWForm();

        // Use actual file extension from the path
        string extension = Path.GetExtension(fileName);
        form.AddBinaryData("file", fileData, fileName, "application/octet-stream");
        form.AddField("project_name", projectName, System.Text.Encoding.UTF8);
        form.AddField("asset_hash", assetHash, System.Text.Encoding.UTF8); // send assetHash so server knows which asset this file belongs to

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"Error uploading file {fileName}: {www.error}");
        } else {
            Debug.Log($"File {fileName} uploaded successfully for asset {assetHash}!");
        }
    }

    IEnumerator DownloadFileRequest(string url, System.Action<byte[]> callback) {
  //      MessageDisplayManager.Instance.DisplayMessage("DownloadFileRequest");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            callback(www.downloadHandler.data);
        } else {
            Debug.Log("Error downloading file: " + www.error);
            callback(null);
        }
    }

    void HandleResponse(UnityWebRequest www, System.Action<bool, string> callback) {
        if (www.result != UnityWebRequest.Result.Success) {
            if (www.responseCode == 401) {
                Debug.LogWarning("Unauthorized access. Please check your credentials.");
            } else {
                Debug.LogError("Request failed: " + www.error);
            }
            callback?.Invoke(false, www.error);
        } else {
            Debug.Log("Request successful!");
            callback?.Invoke(true, www.downloadHandler.text);
        }
    }

    IEnumerator ListFilesRequest(string url, System.Action<List<string>> callback) {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            print(www.downloadHandler.text);
            callback(JsonUtility.FromJson<StringListWrapper>(www.downloadHandler.text).items);
        } else {
            callback(null);
        }
    }

    #endregion


}
[System.Serializable]
public class IframeResponse {
    public string iframe_code;
    public string message;
    public string code;
}

[System.Serializable]
public class ProjectListResponse {
    public ProjectMetadata[] projects;
}

[Serializable]
class StringListWrapper {
    public List<string> items;
}
