#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// When you press Play in the editor and the local Flask server isn't running,
/// starts it (run.bat from the server repo next to this one) and waits for it
/// to come up before entering Play mode.
/// Looks for the server in ../DigitalTwin-Server or ../DigitalTwin.
/// </summary>
[InitializeOnLoad]
public static class LocalServerLauncher {
    private const string MENU_ENABLED = "Tools/Local Server/Auto-start on Play";
    private const string MENU_START = "Tools/Local Server/Start Now";

    private const string HOST = "127.0.0.1";
    private const int PORT = 5000;
    private const double WAIT_TIMEOUT_SECONDS = 30;
    private const double POLL_INTERVAL_SECONDS = 0.5;

    private static readonly string[] ServerFolderNames = { "DigitalTwin-Server", "DigitalTwin" };

    private const string PENDING_PLAY_KEY = "LocalServerLauncher.PendingPlay";
    private const string WAIT_START_KEY = "LocalServerLauncher.WaitStart";
    private const string SKIP_CHECK_KEY = "LocalServerLauncher.SkipCheck";

    private static double lastPoll;

    private static bool Enabled {
        get => EditorPrefs.GetBool(MENU_ENABLED, true);
        set => EditorPrefs.SetBool(MENU_ENABLED, value);
    }

    static LocalServerLauncher() {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.delayCall += () => Menu.SetChecked(MENU_ENABLED, Enabled);

        // Survive a domain reload (e.g. script recompile) while waiting.
        if (SessionState.GetBool(PENDING_PLAY_KEY, false)) {
            EditorApplication.update += WaitForServer;
        }
    }

    [MenuItem(MENU_ENABLED)]
    private static void ToggleEnabled() {
        Enabled = !Enabled;
        Menu.SetChecked(MENU_ENABLED, Enabled);
    }

    [MenuItem(MENU_START)]
    private static void StartNow() {
        if (IsServerRunning()) {
            Debug.Log($"[LocalServer] Already running on {HOST}:{PORT}.");
            return;
        }
        StartServer();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state) {
        if (state != PlayModeStateChange.ExitingEditMode) return;

        if (SessionState.GetBool(SKIP_CHECK_KEY, false)) {
            SessionState.SetBool(SKIP_CHECK_KEY, false);
            return;
        }
        if (!Enabled || IsServerRunning()) return;
        if (!StartServer()) return; // couldn't start it, just play without

        // Hold off Play mode until the server answers.
        EditorApplication.isPlaying = false;
        SessionState.SetBool(PENDING_PLAY_KEY, true);
        SessionState.SetFloat(WAIT_START_KEY, (float)EditorApplication.timeSinceStartup);
        lastPoll = 0;
        EditorApplication.update -= WaitForServer;
        EditorApplication.update += WaitForServer;
    }

    private static void WaitForServer() {
        double now = EditorApplication.timeSinceStartup;
        double elapsed = now - SessionState.GetFloat(WAIT_START_KEY, (float)now);

        bool cancelled = EditorUtility.DisplayCancelableProgressBar(
            "Starting local server",
            $"Waiting for {HOST}:{PORT} … ({elapsed:0}s)",
            (float)(elapsed / WAIT_TIMEOUT_SECONDS));

        if (cancelled) {
            FinishWaiting();
            Debug.Log("[LocalServer] Waiting cancelled, not entering Play mode.");
            return;
        }

        if (now - lastPoll < POLL_INTERVAL_SECONDS) return;
        lastPoll = now;

        bool running = IsServerRunning();
        if (!running && elapsed < WAIT_TIMEOUT_SECONDS) return;

        FinishWaiting();
        if (running) {
            Debug.Log($"[LocalServer] Server is up after {elapsed:0.0}s, entering Play mode.");
        } else {
            Debug.LogWarning($"[LocalServer] Server didn't respond within {WAIT_TIMEOUT_SECONDS}s, entering Play mode anyway. Check the server console window.");
        }
        SessionState.SetBool(SKIP_CHECK_KEY, true);
        EditorApplication.isPlaying = true;
    }

    private static void FinishWaiting() {
        EditorApplication.update -= WaitForServer;
        SessionState.SetBool(PENDING_PLAY_KEY, false);
        EditorUtility.ClearProgressBar();
    }

    private static bool IsServerRunning() {
        try {
            using (var client = new TcpClient()) {
                var connect = client.ConnectAsync(HOST, PORT);
                return connect.Wait(250) && client.Connected;
            }
        } catch {
            return false;
        }
    }

    private static string FindServerFolder() {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        string parent = projectRoot != null ? Directory.GetParent(projectRoot)?.FullName : null;
        if (parent == null) return null;

        foreach (string name in ServerFolderNames) {
            string path = Path.Combine(parent, name);
            if (File.Exists(Path.Combine(path, "DigitalTwinWebsite", "code", "App.py"))) return path;
        }
        return null;
    }

    private static bool StartServer() {
        if (Application.platform != RuntimePlatform.WindowsEditor) {
            Debug.LogWarning("[LocalServer] Auto-start is only set up for Windows. Start the server manually.");
            return false;
        }

        string folder = FindServerFolder();
        if (folder == null) {
            Debug.LogWarning($"[LocalServer] Server folder not found next to the project (looked for: {string.Join(", ", ServerFolderNames)}).");
            return false;
        }

        // Prefer run.bat (sets up venv + .env.local); fall back to running App.py directly.
        bool hasRunBat = File.Exists(Path.Combine(folder, "run.bat"));
        var psi = new ProcessStartInfo {
            FileName = "cmd.exe",
            Arguments = hasRunBat
                ? "/c run.bat"
                : "/k cd /d DigitalTwinWebsite\\code && python App.py --local",
            WorkingDirectory = folder,
            UseShellExecute = true, // own console window, keeps running after Unity closes
            WindowStyle = ProcessWindowStyle.Normal,
        };

        try {
            Process.Start(psi);
            Debug.Log($"[LocalServer] Starting server from {folder}");
            return true;
        } catch (System.Exception e) {
            Debug.LogError($"[LocalServer] Failed to start server: {e.Message}");
            return false;
        }
    }
}
#endif
