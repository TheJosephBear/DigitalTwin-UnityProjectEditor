using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Collects what each upload test loaded and writes screenshots plus report.html / report.md,
/// so the results can be looked at after the run.
/// </summary>
public static class ModelUploadReport {

    public class Entry {
        public string Name;
        public string Notes;
        public List<string> InputFiles = new List<string>();
        public double InputMB;
        public bool Loaded;
        public string Failure;

        public double PrepareSeconds;
        public double BuildSeconds;
        public double MemoryDeltaMB;
        public double ParseAllocatedMB;   // C# memory allocated in the frame of the build (all of it is live at once on WebGL)
        public double ManagedRetainedMB;  // C# memory still used after the build, after a collection
        public double NativeRetainedMB;   // Unity (native) memory the loaded model keeps: meshes, textures, objects

        public int Renderers;
        public int Meshes;
        public long Vertices;
        public long Triangles;
        public int SubMeshes;
        public int Materials;
        public int MaterialsWithMainTexture;
        public int Textures;
        public Vector3 BoundsSize;
        public List<string> TextureNames = new List<string>();

        public List<string> Warnings = new List<string>();
        public List<string> Errors = new List<string>();
        public List<string> Screenshots = new List<string>(); // file names inside the run folder
    }

    static readonly List<Entry> _entries = new List<Entry>();
    static string _runFolder;
    static string _commit;

    static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    /// <summary>
    /// TestResults/ModelUpload/&lt;time of the run&gt;_&lt;commit&gt;/ in the project folder.
    /// </summary>
    public static string RunFolder {
        get {
            if (_runFolder == null) {
                _runFolder = Path.Combine(ProjectRoot, "TestResults", "ModelUpload", $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Commit}");
                Directory.CreateDirectory(_runFolder);
            }
            return _runFolder;
        }
    }

    /// <summary>
    /// The checked out commit (short hash, "+changes" when tracked files are modified), so runs on different commits can be told apart.
    /// </summary>
    public static string Commit {
        get {
            if (_commit == null) {
                string hash = RunGit("rev-parse --short HEAD");
                string dirty = RunGit("status --porcelain --untracked-files=no");
                _commit = string.IsNullOrEmpty(hash) ? "unknown" : hash + (string.IsNullOrEmpty(dirty) ? "" : "+changes");
            }
            return _commit;
        }
    }

    static string RunGit(string args) {
        try {
            var info = new System.Diagnostics.ProcessStartInfo("git", args) {
                WorkingDirectory = ProjectRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var p = System.Diagnostics.Process.Start(info)) {
                string output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit(5000);
                return p.ExitCode == 0 ? output : null;
            }
        } catch {
            return null;
        }
    }

    public static void Add(Entry entry) {
        _entries.RemoveAll(e => e.Name == entry.Name);
        _entries.Add(entry);
        Write(); // after every model, so a crash or a stopped run still leaves a report
    }

    /// <summary>
    /// Starts a new run folder the next time something is written.
    /// </summary>
    public static void BeginRun() {
        _entries.Clear();
        _runFolder = null;
        _commit = null;
    }

    #region Screenshots

    /// <summary>
    /// Renders the model from above at an angle and straight from the top. The model must be active.
    /// </summary>
    public static void TakeScreenshots(GameObject model, Entry entry) {
        if (!FileLoadingManager.TryCalculateBounds(model, out Bounds bounds)) return;

        var camGo = new GameObject("ModelUploadTests Camera");
        var lightGo = new GameObject("ModelUploadTests Light");
        try {
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.82f, 0.85f, 0.88f);
            cam.fieldOfView = 40f;

            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var oldAmbientMode = RenderSettings.ambientMode;
            var oldAmbient = RenderSettings.ambientLight;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);

            Render(cam, bounds, new Vector3(-1f, 0.8f, -1f), entry, "iso");
            Render(cam, bounds, Vector3.up, entry, "top");

            RenderSettings.ambientMode = oldAmbientMode;
            RenderSettings.ambientLight = oldAmbient;
        } finally {
            UnityEngine.Object.Destroy(camGo);
            UnityEngine.Object.Destroy(lightGo);
        }
    }

    static void Render(Camera cam, Bounds bounds, Vector3 viewDirection, Entry entry, string suffix) {
        const int width = 1280, height = 800;

        float radius = Mathf.Max(bounds.extents.magnitude, 0.01f);
        // Fit the bounding sphere into the smaller of the two fields of view
        float vFov = cam.fieldOfView * Mathf.Deg2Rad;
        float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov / 2f) * width / height);
        float distance = radius / Mathf.Sin(Mathf.Min(vFov, hFov) / 2f) * 1.05f;

        Vector3 dir = viewDirection.normalized;
        cam.transform.position = bounds.center + dir * distance;
        // Looking straight down needs another up vector
        Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
        cam.transform.LookAt(bounds.center, up);
        cam.aspect = (float)width / height;

        // The sphere is loose for flat models (a city is wide and low), so move closer until the box corners fill the view
        Vector3[] corners = Corners(bounds);
        for (int i = 0; i < 4; i++) {
            float fill = 0f;
            bool cornerBehind = false;
            foreach (Vector3 c in corners) {
                Vector3 vp = cam.WorldToViewportPoint(c);
                if (vp.z <= distance * 0.05f) { cornerBehind = true; break; }
                fill = Mathf.Max(fill, Mathf.Abs(vp.x - 0.5f) * 2f, Mathf.Abs(vp.y - 0.5f) * 2f);
            }
            if (cornerBehind || fill <= 0f) break;

            float newDistance = distance * fill * 1.04f;
            if (Mathf.Abs(newDistance - distance) < distance * 0.01f) break;
            distance = newDistance;
            cam.transform.position = bounds.center + dir * distance;
        }

        cam.nearClipPlane = Mathf.Max(distance - radius * 1.5f, distance * 0.001f);
        cam.farClipPlane = distance + radius * 1.5f;

        var rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try {
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            string fileName = $"{Sanitize(entry.Name)}_{suffix}.png";
            File.WriteAllBytes(Path.Combine(RunFolder, fileName), tex.EncodeToPNG());
            entry.Screenshots.Add(fileName);
        } finally {
            cam.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            UnityEngine.Object.Destroy(tex);
        }
    }

    static Vector3[] Corners(Bounds b) {
        Vector3 min = b.min, max = b.max;
        return new[] {
            new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z),
            new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z),
        };
    }

    #endregion

    #region Writing the report

    static void Write() {
        var csv = new StringBuilder("model,loaded,input_mb,parse_allocated_mb,managed_retained_mb,native_retained_mb,vertices,triangles,textures,build_s\n");
        foreach (var e in _entries) {
            csv.AppendLine(string.Join(",", e.Name, e.Loaded, F(e.InputMB, "0.00"), F(e.ParseAllocatedMB, "0.00"), F(e.ManagedRetainedMB, "0.00"),
                F(e.NativeRetainedMB, "0.00"), e.Vertices, e.Triangles, e.Textures, F(e.BuildSeconds, "0.00")));
        }
        File.WriteAllText(Path.Combine(RunFolder, "memory.csv"), csv.ToString(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(RunFolder, "report.md"), BuildMarkdown(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(RunFolder, "report.html"), BuildHtml(), Encoding.UTF8);
    }

    static string F(double value, string format = "0.##") => value.ToString(format, CultureInfo.InvariantCulture);

    static string Size(Vector3 v) => $"{F(v.x, "0.#")} × {F(v.y, "0.#")} × {F(v.z, "0.#")} m";

    static string BuildMarkdown() {
        var sb = new StringBuilder();
        sb.AppendLine($"# Model upload test – {DateTime.Now:yyyy-MM-dd HH:mm} – commit {Commit}");
        sb.AppendLine();
        sb.AppendLine("| Model | Result | Input MB | Prepare s | Build s | Mem +MB | Renderers | Vertices | Triangles | Materials | Textured mats | Textures | Size | Warnings | Errors |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (var e in _entries) {
            sb.AppendLine($"| {e.Name} | {(e.Loaded ? "loaded" : "FAILED")} | {F(e.InputMB, "0.#")} | {F(e.PrepareSeconds)} | {F(e.BuildSeconds)} | " +
                $"{F(e.MemoryDeltaMB, "0")} | {e.Renderers} | {e.Vertices:N0} | {e.Triangles:N0} | {e.Materials} | {e.MaterialsWithMainTexture} | " +
                $"{e.Textures} | {Size(e.BoundsSize)} | {e.Warnings.Count} | {e.Errors.Count} |");
        }
        foreach (var e in _entries) {
            sb.AppendLine();
            sb.AppendLine($"## {e.Name}");
            sb.AppendLine(e.Notes);
            if (!string.IsNullOrEmpty(e.Failure)) sb.AppendLine($"\n**Failure:** {e.Failure}");
            sb.AppendLine($"\nFiles: {string.Join(", ", e.InputFiles.Take(12))}{(e.InputFiles.Count > 12 ? $" … (+{e.InputFiles.Count - 12})" : "")}");
            foreach (string shot in e.Screenshots) sb.AppendLine($"\n![{shot}]({Uri.EscapeDataString(shot)})");
            foreach (string err in e.Errors.Take(20)) sb.AppendLine($"- ERROR: {err}");
            foreach (string warn in e.Warnings.Take(20)) sb.AppendLine($"- warning: {warn}");
            if (e.Warnings.Count > 20) sb.AppendLine($"- … {e.Warnings.Count - 20} more warnings");
        }
        return sb.ToString();
    }

    static string H(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");

    static string BuildHtml() {
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html lang=\"cs\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        sb.AppendLine("<title>Model upload test</title><style>");
        sb.AppendLine(@"
:root { --bg:#f6f7f9; --card:#fff; --text:#1d2330; --muted:#677084; --line:#e2e5eb; --ok:#1a7f4b; --bad:#c62828; --warn:#a86200; }
body { margin:0; padding:24px 16px; background:var(--bg); color:var(--text); font:14px/1.45 system-ui,'Segoe UI',sans-serif; }
main { max-width:1200px; margin:0 auto; }
h1 { font-size:22px; margin:0 0 4px; } .sub { color:var(--muted); margin:0 0 20px; }
.table-wrap { overflow-x:auto; background:var(--card); border:1px solid var(--line); border-radius:8px; margin-bottom:28px; }
table { border-collapse:collapse; width:100%; font-variant-numeric:tabular-nums; }
th, td { padding:7px 10px; border-bottom:1px solid var(--line); text-align:right; white-space:nowrap; }
th:first-child, td:first-child { text-align:left; } th { font-weight:600; color:var(--muted); font-size:12px; }
tr:last-child td { border-bottom:0; }
a { color:inherit; }
.ok { color:var(--ok); font-weight:600; } .bad { color:var(--bad); font-weight:600; } .warn { color:var(--warn); }
section { background:var(--card); border:1px solid var(--line); border-radius:8px; padding:16px; margin-bottom:16px; }
section h2 { font-size:17px; margin:0 0 4px; } .notes { color:var(--muted); margin:0 0 10px; }
.shots { display:grid; grid-template-columns:repeat(auto-fit,minmax(280px,1fr)); gap:10px; margin:10px 0; }
.shots img { width:100%; border-radius:6px; border:1px solid var(--line); display:block; }
.stats { display:flex; flex-wrap:wrap; gap:6px 18px; color:var(--muted); } .stats b { color:var(--text); font-weight:600; }
details { margin-top:8px; } summary { cursor:pointer; color:var(--muted); }
pre { white-space:pre-wrap; word-break:break-word; font-size:12px; background:var(--bg); padding:8px; border-radius:6px; }
");
        sb.AppendLine("</style></head><body><main>");
        sb.AppendLine($"<h1>Model upload test</h1><p class=\"sub\">{DateTime.Now:yyyy-MM-dd HH:mm} · commit {H(Commit)} · {_entries.Count} models · {_entries.Count(e => e.Loaded)} loaded</p>");

        sb.AppendLine("<div class=\"table-wrap\"><table><tr><th>Model</th><th>Result</th><th>Input MB</th><th>Prepare s</th><th>Build s</th><th>Mem +MB</th>" +
            "<th>Renderers</th><th>Vertices</th><th>Triangles</th><th>Materials</th><th>Textured mats</th><th>Textures</th><th>Warnings</th><th>Errors</th></tr>");
        foreach (var e in _entries) {
            sb.AppendLine($"<tr><td><a href=\"#{Sanitize(e.Name)}\">{H(e.Name)}</a></td><td class=\"{(e.Loaded ? "ok" : "bad")}\">{(e.Loaded ? "loaded" : "failed")}</td>" +
                $"<td>{F(e.InputMB, "0.#")}</td><td>{F(e.PrepareSeconds)}</td><td>{F(e.BuildSeconds)}</td><td>{F(e.MemoryDeltaMB, "0")}</td>" +
                $"<td>{e.Renderers:N0}</td><td>{e.Vertices:N0}</td><td>{e.Triangles:N0}</td><td>{e.Materials}</td><td>{e.MaterialsWithMainTexture}</td><td>{e.Textures}</td>" +
                $"<td class=\"{(e.Warnings.Count > 0 ? "warn" : "")}\">{e.Warnings.Count}</td><td class=\"{(e.Errors.Count > 0 ? "bad" : "")}\">{e.Errors.Count}</td></tr>");
        }
        sb.AppendLine("</table></div>");

        foreach (var e in _entries) {
            sb.AppendLine($"<section id=\"{Sanitize(e.Name)}\"><h2>{H(e.Name)} <span class=\"{(e.Loaded ? "ok" : "bad")}\">{(e.Loaded ? "loaded" : "failed")}</span></h2>");
            sb.AppendLine($"<p class=\"notes\">{H(e.Notes)}</p>");
            if (!string.IsNullOrEmpty(e.Failure)) sb.AppendLine($"<p class=\"bad\">{H(e.Failure)}</p>");
            if (e.Screenshots.Count > 0) {
                sb.AppendLine("<div class=\"shots\">");
                foreach (string shot in e.Screenshots) sb.AppendLine($"<a href=\"{H(Uri.EscapeDataString(shot))}\"><img loading=\"lazy\" src=\"{H(Uri.EscapeDataString(shot))}\" alt=\"{H(shot)}\"></a>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("<div class=\"stats\">" +
                $"<span>size <b>{H(Size(e.BoundsSize))}</b></span><span>meshes <b>{e.Meshes:N0}</b></span><span>submeshes <b>{e.SubMeshes:N0}</b></span>" +
                $"<span>input <b>{e.InputFiles.Count} files, {F(e.InputMB, "0.#")} MB</b></span></div>");
            sb.AppendLine($"<details><summary>Files ({e.InputFiles.Count})</summary><pre>{H(string.Join("\n", e.InputFiles))}</pre></details>");
            if (e.TextureNames.Count > 0)
                sb.AppendLine($"<details><summary>Textures on materials ({e.TextureNames.Count})</summary><pre>{H(string.Join("\n", e.TextureNames))}</pre></details>");
            if (e.Errors.Count > 0)
                sb.AppendLine($"<details open><summary class=\"bad\">Errors ({e.Errors.Count})</summary><pre>{H(string.Join("\n\n", e.Errors.Take(50)))}</pre></details>");
            if (e.Warnings.Count > 0)
                sb.AppendLine($"<details><summary class=\"warn\">Warnings ({e.Warnings.Count})</summary><pre>{H(string.Join("\n\n", e.Warnings.Take(100)))}</pre></details>");
            sb.AppendLine("</section>");
        }
        sb.AppendLine("</main></body></html>");
        return sb.ToString();
    }

    static string Sanitize(string name) {
        var sb = new StringBuilder();
        foreach (char c in name) sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
        return sb.ToString();
    }

    #endregion
}
