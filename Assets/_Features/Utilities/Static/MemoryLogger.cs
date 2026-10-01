using UnityEngine;
using UnityEngine.Profiling;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Logs memory usage to the console. Used to find which step grows the WASM heap in WebGL builds.
/// </summary>
public static class MemoryLogger {
    // The function comes from the JavaScript plugin defined in Assets/Plugins/WebGLMemoryStats.jslib
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern double GetWasmHeapSizeMB();
#else
    private static double GetWasmHeapSizeMB() => 0;
#endif

    private const float MB = 1024f * 1024f;

    /// <summary>
    /// Turns memory logging on/off. Set by <see cref="Initializer"/> from its inspector toggle or the "memoryLog" URL parameter.
    /// </summary>
    public static bool Enabled = false;

    /// <summary>
    /// Logs WASM heap size, Unity reserved/used memory and managed (C#) memory.
    /// </summary>
    /// <param name="label">Describes the point in the code where the log is made.</param>
    public static void Log(string label) {
        if (!Enabled) return;

        Debug.Log($"[Memory] {label}: " +
            $"WASM heap {GetWasmHeapSizeMB():F0} MB | " +
            $"Unity reserved {Profiler.GetTotalReservedMemoryLong() / MB:F0} MB | " +
            $"Unity used {Profiler.GetTotalAllocatedMemoryLong() / MB:F0} MB | " +
            $"C# managed {System.GC.GetTotalMemory(false) / MB:F0} MB");
    }
}
