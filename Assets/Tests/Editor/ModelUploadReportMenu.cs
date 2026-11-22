using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Opens the report written by ModelUploadTests (play mode tests) in the browser.
/// </summary>
public static class ModelUploadReportMenu {

    [MenuItem("Tools/Model Upload Tests/Open Latest Report")]
    static void OpenLatestReport() {
        string root = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "TestResults", "ModelUpload");
        string latest = Directory.Exists(root)
            ? Directory.GetDirectories(root).OrderByDescending(d => d).FirstOrDefault(d => File.Exists(Path.Combine(d, "report.html")))
            : null;

        if (latest == null) {
            EditorUtility.DisplayDialog("Model Upload Tests", "No report yet. Run the ModelUploadTests in the Test Runner (PlayMode tab).", "OK");
            return;
        }
        Application.OpenURL("file:///" + Path.Combine(latest, "report.html").Replace('\\', '/'));
    }

    [MenuItem("Tools/Model Upload Tests/Open Results Folder")]
    static void OpenResultsFolder() {
        string root = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "TestResults", "ModelUpload");
        Directory.CreateDirectory(root);
        EditorUtility.RevealInFinder(root);
    }
}
