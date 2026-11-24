using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Splits long main thread work over frames (WebGL has no threads): <c>if (budget.ShouldYield()) yield return null;</c>
/// inside a loop lets a frame render, e.g. the loading screen, once the work took longer than the budget.
/// </summary>
public class FrameBudget {
    public const long DefaultMilliseconds = 30;

    readonly long _milliseconds;
    readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public FrameBudget(long milliseconds = DefaultMilliseconds) {
        _milliseconds = milliseconds;
    }

    /// <summary>True once the budget is used up, then it starts again (counting from the next frame).</summary>
    public bool ShouldYield() {
        if (_stopwatch.ElapsedMilliseconds < _milliseconds) return false;
        _stopwatch.Restart();
        return true;
    }
}

public static class CoroutineUtils {
    /// <summary>
    /// Runs a coroutine to the end right away, in this frame, for callers that need the result synchronously.
    /// Nested IEnumerators are run too; any other yielded value (null, WaitUntil...) is skipped.
    /// </summary>
    public static void RunSync(IEnumerator routine) {
        Stack<IEnumerator> stack = new Stack<IEnumerator>();
        stack.Push(routine);
        while (stack.Count > 0) {
            IEnumerator top = stack.Peek();
            if (!top.MoveNext()) {
                stack.Pop();
            } else if (top.Current is IEnumerator nested) {
                stack.Push(nested);
            }
        }
    }

    /// <summary>
    /// Runs <paramref name="routine"/> as part of a coroutine, catching its exceptions (a coroutine can't yield inside try/catch).
    /// Nested IEnumerators are run here too, so their exceptions are caught as well.
    /// An exception stops the routine and is passed to <paramref name="onError"/>.
    /// </summary>
    public static IEnumerator TryCoroutine(IEnumerator routine, Action<Exception> onError) {
        Stack<IEnumerator> stack = new Stack<IEnumerator>();
        stack.Push(routine);
        while (stack.Count > 0) {
            IEnumerator top = stack.Peek();
            object current;
            try {
                if (!top.MoveNext()) {
                    stack.Pop();
                    continue;
                }
                current = top.Current;
            } catch (Exception e) {
                onError?.Invoke(e);
                yield break;
            }
            if (current is IEnumerator nested) stack.Push(nested);
            else yield return current;
        }
    }
}
