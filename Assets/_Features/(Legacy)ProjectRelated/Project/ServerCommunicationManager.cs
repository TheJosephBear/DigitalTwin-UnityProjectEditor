using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public void FetchAllProjects(System.Action<bool, List<string>> callback) {
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
    public void UploadFileToServer(string path, string fileName, string projectName) {
        string url = $"{serverUrl}/upload_model_files";
        StartCoroutine(UploadFileRequest(url, path, fileName, projectName));
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
            print("------------");
            print("PostRequest");
            print("field" + field);
            print("fieldK" + field.Key);
            print("fieldV" + field.Value);
            print("------------");
            form.AddField(field.Key, field.Value);
        }
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        HandleResponse(www, callback);
    }

    public IEnumerator GetRequest<T>(string url, System.Action<bool, T> callback, bool returnAsString = false) {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            try {
                if (returnAsString) {
                    object response = www.downloadHandler.text;
                    callback(true, (T)response);
                } else {
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

    IEnumerator UploadFileRequest(string url, string path, string fileName, string projectName) {
        byte[] fileData = File.ReadAllBytes(path);
        WWWForm form = new WWWForm();

        print("------------");
        print("Upload file request");
        print("path" + path);
        print("filename" + fileName);
        print("projectname" + projectName);
        print("------------");

        form.AddBinaryData("file", fileData, fileName + ".obj", "application/octet-stream");
        form.AddField("project_name", projectName);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error uploading file: " + www.error);
        } else {
            Debug.Log("File uploaded successfully!");
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

    #region ghost code

    /*
    public string serverUrl = "http://127.0.0.1:5000";
    public string serverUrlUploadJson = "http://127.0.0.1:5000/upload_editor_data";
    public string serverUrlUploadFiles = "http://127.0.0.1:5000/uploadFiles";
    public string serverUrlDownload = "http://127.0.0.1:5000/download";
    public string serverUrlDownloadFiles = "http://127.0.0.1:5000/downloadModels";

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
    IEnumerator PostRequest(string url, Dictionary<string, string> formData, System.Action<bool, string> callback) {
        // Prepare form data
        WWWForm form = new WWWForm();
        foreach (var field in formData) {
            form.AddField(field.Key, field.Value);
        }

        // Send POST request
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        // Handle response
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error: " + www.error);
            callback(false, www.error); // Error case, return false with the error message
        } else {
            Debug.Log("Response: " + www.downloadHandler.text);
            callback(true, www.downloadHandler.text); // Success case, return true with the server's response
        }
    }

    public void CreateProject(string projectName, System.Action<bool, string> callback) {
        StartCoroutine(CreateProjectCoroutine(projectName, callback));
    }

    IEnumerator CreateProjectCoroutine(string projectName, System.Action<bool, string> callback) {
        WWWForm form = new WWWForm();
        form.AddField("projectName", projectName);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/createProject", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error creating project on da server : " + www.error);
            callback(false, www.error);
        } else {
            Debug.Log("Project created successfully on da server!");
            callback(true, www.downloadHandler.text);
        }
    }

    public void EditProjectName(string oldName, string newName, System.Action<bool, string> callback) {
        StartCoroutine(EditProjectNameCoroutine(oldName, newName, callback));
    }

    IEnumerator EditProjectNameCoroutine(string oldName, string newName, System.Action<bool, string> callback) {
        WWWForm form = new WWWForm();
        form.AddField("oldProjectName", oldName);
        form.AddField("newProjectName", newName);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/editProjectName", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error editing project name: " + www.error);
            callback(false, www.error);
        } else {
            Debug.Log("Project name edited successfully!");
            callback(true, www.downloadHandler.text);
        }
    }

    public void DuplicateProject(string name, System.Action<bool, string> callback) {
        StartCoroutine(DuplicateProjectCoroutine(name, callback));
    }

    IEnumerator DuplicateProjectCoroutine(string name, System.Action<bool, string> callback) {
        WWWForm form = new WWWForm();
        form.AddField("projectName", name);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/duplicate_project", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error editing project name: " + www.error);
            callback(false, www.error);
        } else {
            Debug.Log("Project name edited successfully!");
            callback(true, www.downloadHandler.text);
        }
    }

    public void DeleteProject(string projectName, System.Action<bool, string> callback) {
        StartCoroutine(DeleteProjectCoroutine(projectName, callback));
    }

    IEnumerator DeleteProjectCoroutine(string projectName, System.Action<bool, string> callback) {
        WWWForm form = new WWWForm();
        form.AddField("projectName", projectName);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/deleteProject", form); // DELETE in HTTP technically, but UnityWebRequest doesn't have a DELETE with form data
        www.method = UnityWebRequest.kHttpVerbDELETE;  // Change method to DELETE
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error deleting project: " + www.error);
            callback(false, www.error);
        } else {
            Debug.Log("Project deleted successfully!");
            callback(true, www.downloadHandler.text);
        }
    }

    public void FetchAllProjects(System.Action<List<string>> callback) {
        StartCoroutine(FetchAllProjectsCoroutine(callback));
    }

    IEnumerator FetchAllProjectsCoroutine(System.Action<List<string>> callback) {
        string url = "http://127.0.0.1:5000/getAllProjects";

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error fetching projects: " + www.error);
            callback(null);
        } else {
            Debug.Log("Projects fetched successfully!");

            // Parse the JSON response
            string jsonResponse = www.downloadHandler.text;
            ProjectListResponse response = JsonUtility.FromJson<ProjectListResponse>(jsonResponse);

            if (response != null && response.projects != null) {
                callback(response.projects);
            } else {
                callback(null);
            }
        }
    }

    public void StartUpload(string data, string projectName) {
        StartCoroutine(UploadDataCoroutine(data, projectName));
    }

    IEnumerator UploadDataCoroutine(string data, string projectName) {
        // Create form data
        WWWForm form = new WWWForm();
        form.AddField("myData", data);
        form.AddField("projectName", projectName); // Include project name

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


    public void StartDataDownload(string projectName, System.Action<string> callback) {
        StartCoroutine(DownloadDataCoroutine(projectName, callback));
    }

    IEnumerator DownloadDataCoroutine(string projectName, System.Action<string> callback) {
        string url = $"{serverUrlDownload}?projectName={UnityWebRequest.EscapeURL(projectName)}";
        UnityWebRequest www = UnityWebRequest.Get(url);
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
    public void UploadFileToServer(string path, string fileName, string projectName) {
        StartCoroutine(UploadFileCoroutine(path, fileName, projectName));
    }

    IEnumerator UploadFileCoroutine(string path, string fileName, string projectName) {


        fileName += ".obj"; // need to figure out a better way


        byte[] fileData = File.ReadAllBytes(path);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, fileName, "application/octet-stream");
        form.AddField("projectName", projectName);


        UnityWebRequest www = UnityWebRequest.Post(serverUrlUploadFiles, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error uploading file: " + www.error); ;
        } else {
            Debug.Log("File uploaded successfully!");
            Debug.Log("Server response: " + www.downloadHandler.text);
        }
    }

    public void DownloadFileFromServer(string fileName, string projectName, System.Action<byte[]> callback) {
        StartCoroutine(DownloadFileCoroutine(fileName, projectName, callback));
    }

    IEnumerator DownloadFileCoroutine(string fileName, string projectName, System.Action<byte[]> callback) {
        print("Downloading the model from server");
        string url = $"{serverUrlDownloadFiles}?projectName={UnityWebRequest.EscapeURL(projectName)}&fileName={UnityWebRequest.EscapeURL(fileName)}.obj"; // .obj !!!!!

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error downloading file: " + www.error);
            callback(null);
        } else {
            print("File downloaded successfully");
            callback(www.downloadHandler.data);
        }
    }

    public void GenerateViewerIframe(string projectName, System.Action<string> callback) {
        StartCoroutine(GenerateViewerIframeCoroutine(projectName, callback));
    }

    IEnumerator GenerateViewerIframeCoroutine(string projectName, System.Action<string> callback) {

        string url = $"http://127.0.0.1:5000/generate_iframe?projectName={UnityWebRequest.EscapeURL(projectName)}";

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error generating iframe: " + www.error);
            callback(null);
        } else {
            // Parse the JSON response
            string jsonResponse = www.downloadHandler.text;
            IframeResponse response = JsonUtility.FromJson<IframeResponse>(jsonResponse);

            if (response != null && response.code == "SUCCESS") {
                Debug.Log("Iframe generated successfully!");
                callback(response.iframe_code); // Pass the iframe code back
            } else {
                Debug.LogError("Error in response: " + response.message);
                callback(null);
            }
        }
    }
    
    */
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
    public List<string> projects;
}