using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WebCommunicationManager : Singleton<WebCommunicationManager> {

    public string serverUrlUploadJson = "http://127.0.0.1:5000/upload";
    public string serverUrlUploadFiles = "http://127.0.0.1:5000/uploadFiles";
    public string serverUrlDownload = "http://127.0.0.1:5000/download";
    public string serverUrlDownloadFiles = "http://127.0.0.1:5000/downloadModels";

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