using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// Bold / italic / size buttons for a TMP input field: wraps the selected text in rich text tags,
/// clicking again on the same selection removes them. The field shows the formatting, the tags stay hidden.
/// </summary>
public class RichTextToolbar : MonoBehaviour {

    public TMP_InputField Input;

    [Tooltip("Size steps in % of the field's font size, A- / A+ move the selection one step")]
    public int[] SizeSteps = { 75, 100, 125, 150, 200 };

    // The buttons take the focus away from the field, so the selection is remembered while it is focused
    int _selectionStart;
    int _selectionEnd;

    static readonly Regex SizeOpen = new Regex(@"^<size=(\d+)%>$");
    static readonly Regex SizeWrapped = new Regex(@"^<size=(\d+)%>(.*)</size>$", RegexOptions.Singleline);

    void Awake() {
        if (Input != null) Input.onFocusSelectAll = false; // refocusing after a button would select everything
    }

    void LateUpdate() {
        if (Input == null || !Input.isFocused) return;
        _selectionStart = Mathf.Min(Input.selectionStringAnchorPosition, Input.selectionStringFocusPosition);
        _selectionEnd = Mathf.Max(Input.selectionStringAnchorPosition, Input.selectionStringFocusPosition);
    }

    public void OnBold() => ToggleTag("b");
    public void OnItalic() => ToggleTag("i");
    public void OnSmaller() => StepSize(-1);
    public void OnBigger() => StepSize(1);

    void ToggleTag(string tag) {
        if (!TryGetSelection(out string text, out int start, out int end)) return;

        string open = "<" + tag + ">";
        string close = "</" + tag + ">";

        if (FindWrappingTags(text, start, end, t => t == open, close, out int openIndex, out int closeIndex)) {
            // Tags around the selection (possibly with other tags between): remove them
            text = text.Remove(closeIndex, close.Length).Remove(openIndex, open.Length);
            start -= open.Length;
            end -= open.Length;
        } else if (text.Substring(start, end - start).StartsWith(open) && text.Substring(start, end - start).EndsWith(close) && end - start >= open.Length + close.Length) {
            // The selection includes the tags
            text = text.Remove(end - close.Length, close.Length).Remove(start, open.Length);
            end -= open.Length + close.Length;
        } else {
            text = text.Insert(end, close).Insert(start, open);
            start += open.Length;
            end += open.Length;
        }

        Apply(text, start, end);
    }

    void StepSize(int direction) {
        if (!TryGetSelection(out string text, out int start, out int end)) return;

        const string close = "</size>";
        int current = 100;

        // Unwrap the size tag already around (or included in) the selection, remembering its size
        if (FindWrappingTags(text, start, end, t => SizeOpen.IsMatch(t), close, out int openIndex, out int closeIndex)) {
            string open = TagAt(text, openIndex);
            current = int.Parse(SizeOpen.Match(open).Groups[1].Value);
            text = text.Remove(closeIndex, close.Length).Remove(openIndex, open.Length);
            start -= open.Length;
            end -= open.Length;
        } else {
            Match wrapped = SizeWrapped.Match(text.Substring(start, end - start));
            if (wrapped.Success) {
                current = int.Parse(wrapped.Groups[1].Value);
                string inner = wrapped.Groups[2].Value;
                text = text.Remove(start, end - start).Insert(start, inner);
                end = start + inner.Length;
            }
        }

        int size = NextStep(current, direction);
        if (size != 100) {
            string open = $"<size={size}%>";
            text = text.Insert(end, close).Insert(start, open);
            start += open.Length;
            end += open.Length;
        }

        Apply(text, start, end);
    }

    // The step after the current size, also for sizes that aren't one of the steps
    int NextStep(int current, int direction) {
        if (direction > 0) {
            foreach (int step in SizeSteps) if (step > current) return step;
            return SizeSteps[SizeSteps.Length - 1];
        }
        for (int i = SizeSteps.Length - 1; i >= 0; i--) if (SizeSteps[i] < current) return SizeSteps[i];
        return SizeSteps[0];
    }

    bool TryGetSelection(out string text, out int start, out int end) {
        text = Input != null ? Input.text : "";
        start = Mathf.Clamp(_selectionStart, 0, text.Length);
        end = Mathf.Clamp(_selectionEnd, 0, text.Length);
        return Input != null && end > start;
    }

    /// <summary>
    /// Finds an opening tag among the tags directly in front of the selection (like "&lt;b&gt;&lt;i&gt;")
    /// and its closing tag among the tags directly behind it, so formatting can be removed whatever else is applied.
    /// </summary>
    static bool FindWrappingTags(string text, int start, int end, System.Func<string, bool> isOpen, string close, out int openIndex, out int closeIndex) {
        openIndex = -1;
        closeIndex = -1;

        // Nearest first: the innermost matching tag pairs with the innermost closing tag
        List<int> before = new List<int>();
        for (int pos = start; pos > 0 && text[pos - 1] == '>';) {
            int tagStart = text.LastIndexOf('<', pos - 1);
            if (tagStart < 0) break;
            before.Add(tagStart);
            pos = tagStart;
        }
        foreach (int index in before) {
            if (isOpen(TagAt(text, index))) { openIndex = index; break; }
        }
        if (openIndex < 0) return false;

        for (int pos = end; pos < text.Length && text[pos] == '<';) {
            string tag = TagAt(text, pos);
            if (tag == null) break;
            if (tag == close) { closeIndex = pos; return true; }
            pos += tag.Length;
        }
        return false;
    }

    // The whole "<...>" tag starting at index, null when there is none
    static string TagAt(string text, int index) {
        int tagEnd = text.IndexOf('>', index);
        return tagEnd < 0 ? null : text.Substring(index, tagEnd - index + 1);
    }

    void Apply(string text, int start, int end) {
        Input.text = text; // notifies, so the owner sees the change like typed text
        _selectionStart = start;
        _selectionEnd = end;
        StartCoroutine(Reselect(start, end));
    }

    // Focus comes back a frame after activating, setting the selection before that gets overwritten
    IEnumerator Reselect(int start, int end) {
        Input.ActivateInputField();
        yield return null;
        Input.selectionStringAnchorPosition = start;
        Input.selectionStringFocusPosition = end;
    }
}
