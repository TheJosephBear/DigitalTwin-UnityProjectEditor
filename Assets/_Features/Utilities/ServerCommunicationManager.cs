using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ServerCommunicationManager : Singleton<ServerCommunicationManager> {

    public string serverUrl = "http://127.0.0.1:5000";

    #region Login and Register

    public void Login(string username, string password, System.Action<bool, string> callback) {
        string url = $"{serverUrl}/login";
        Dictionary<string, string> formData = new Dictionary<string, string> {
            { "username", username },
            { "password", password }
        };
        StartCoroutine(PostRequest(url, formData, callback));
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
            { "newProjectName", newName }
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
    public void StartUpload(string data, string projectName) {
        string url = $"{serverUrl}/upload_editor_data";
        Dictionary<string, string> formData = new Dictionary<string, string>
        {
            { "myData", data },
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

    // Upload file into a project
    public void UploadFileToServer(string path, string fileName, string projectName, string assetHash) {
        string url = $"{serverUrl}/upload_model_files";
        StartCoroutine(UploadFileRequest(url, path, fileName, projectName, assetHash));
    }

    // Download file from a project
    public void DownloadFileFromServer(string fileName, string projectName, System.Action<byte[]> callback) {
        string url = $"{serverUrl}/downloadModels?project_name={UnityWebRequest.EscapeURL(projectName)}&file_name={UnityWebRequest.EscapeURL(fileName)}.obj";
        StartCoroutine(DownloadFileRequest(url, callback));
    }


    #endregion

    #region Private helper functions

    IEnumerator PostRequest(string url, Dictionary<string, string> formData, System.Action<bool, string> callback) {
        WWWForm form = new WWWForm();
        foreach (var field in formData) {
            form.AddField(field.Key, field.Value);
        }
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        HandleResponse(www, callback);
    }

    public IEnumerator GetRequest<T>(string url, System.Action<bool, T> callback, bool returnAsString = false) where T : class {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            try {
                if (returnAsString) {
                    object response = www.downloadHandler.text;
                    callback(true, (T)response);
                } else {
                    print(www.downloadHandler.text);
                    T response = JsonUtility.FromJson<T>(www.downloadHandler.text);
                    callback(true, response);
                }
            } catch (System.Exception ex) {
                Debug.LogError("Error parsing response: " + ex.Message);
                callback(false, default);
            }
        } else {
            Debug.Log("Error fetching data: " + www.error);
            callback(false, default);
        }
    }

    IEnumerator DeleteRequest(string url, Dictionary<string, string> formData, System.Action<bool, string> callback) {
        WWWForm form = new WWWForm();
        foreach (var field in formData) {
            form.AddField(field.Key, field.Value);
        }

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.method = UnityWebRequest.kHttpVerbDELETE;
        yield return www.SendWebRequest();

        HandleResponse(www, callback);
    }

    IEnumerator UploadFileRequest(string url, string path, string fileName, string projectName, string assetHash) {
        print("uploading");
        print("project name: "+projectName);
        if (path == null) {
            Debug.LogError("Upload file request err: PATH IS NULL");
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(path);
        WWWForm form = new WWWForm();

        // Use actual file extension from the path
        string extension = Path.GetExtension(fileName);
        form.AddBinaryData("file", fileData, fileName, "application/octet-stream");
        form.AddField("project_name", projectName);
        form.AddField("asset_hash", assetHash); // send assetHash so server knows which asset this file belongs to

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"Error uploading file {fileName}: {www.error}");
        } else {
            Debug.Log($"File {fileName} uploaded successfully for asset {assetHash}!");
        }
    }

    IEnumerator DownloadFileRequest(string url, System.Action<byte[]> callback) {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            callback(www.downloadHandler.data);
        } else {
            Debug.LogError("Error downloading file: " + www.error);
            callback(null);
        }
    }

    void HandleResponse(UnityWebRequest www, System.Action<bool, string> callback) {
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Request failed: " + www.error);
            callback?.Invoke(false, www.error);
        } else {
            Debug.Log("Request successful!");
            callback?.Invoke(true, www.downloadHandler.text);
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