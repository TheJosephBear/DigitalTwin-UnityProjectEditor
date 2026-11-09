using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// Bold / italic / size buttons for a TMP input field: wraps the selected text in rich text tags,
/// clicking again on the same selection removes them. The field shows the formatting, the tags stay hidden.
/// List buttons turn the selected lines into a bullet, numbered or lettered list, Enter continues the list.
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

    enum ListType { None, Bullet, Number, Letter }

    // A list line: marker, then the text in an indent tag so wrapped lines stay aligned under the text (hanging indent).
    // TMP puts the caret at the end of an item (or in an empty one) behind the hidden closing tag, OnValidateInput moves typing in front of it.
    const string ListIndent = "<indent=1.2em>";
    const string ListIndentClose = "</indent>";
    static readonly Regex ListLine = new Regex(@"^(•|\d+\.|[a-z]+\.)<indent=[^>]*>");

    class ListLineInfo {
        public ListType Type;
        public string Content;
        public int Prefix; // characters in front of the content in the text (marker, indent tag)
    }

    void Awake() {
        if (Input == null) return;
        Input.onFocusSelectAll = false; // refocusing after a button would select everything
        Input.onValidateInput = OnValidateInput;
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
    public void OnBulletList() => ToggleList(ListType.Bullet);
    public void OnNumberList() => ToggleList(ListType.Number);
    public void OnLetterList() => ToggleList(ListType.Letter);

    /// <summary>
    /// Makes the lines the selection touches (or the caret's line, even an empty one) a list of the type.
    /// When they already all are that list, the list is removed. Empty lines in a selection are left as they are.
    /// </summary>
    void ToggleList(ListType type) {
        if (Input == null) return;
        string text = Input.text;
        int start = Mathf.Clamp(_selectionStart, 0, text.Length);
        int end = Mathf.Clamp(_selectionEnd, 0, text.Length);

        List<ListLineInfo> lines = ParseLines(text);
        // A selection ending right after a line break doesn't include the next line
        int first = LineAt(text, start);
        int last = LineAt(text, end > start ? end - 1 : end);
        bool single = first == last;

        bool remove = true;
        for (int i = first; i <= last; i++) {
            if (!single && lines[i].Type == ListType.None && lines[i].Content.Length == 0) continue;
            if (lines[i].Type != type) { remove = false; break; }
        }
        for (int i = first; i <= last; i++) {
            if (!single && lines[i].Type == ListType.None && lines[i].Content.Length == 0) continue;
            lines[i].Type = remove ? ListType.None : type;
        }

        // The caret stays where it was in the line's text
        int caretOffset = Mathf.Clamp(start - LineStart(text, start) - lines[first].Prefix, 0, lines[first].Content.Length);

        text = BuildLines(lines, out int[] contentStarts);
        if (end > start) Apply(text, contentStarts[first], contentStarts[last] + lines[last].Content.Length);
        else Apply(text, contentStarts[first] + caretOffset, contentStarts[first] + caretOffset);
    }

    // Typing at the end of a list item goes in front of its closing tag.
    // Enter in a list item starts the next item (numbered on), Enter in an empty item ends the list.
    char OnValidateInput(string text, int charIndex, char addedChar) {
        bool enter = addedChar == '\n' || addedChar == '\r';
        bool behindClose = charIndex >= ListIndentClose.Length
            && string.CompareOrdinal(text, charIndex - ListIndentClose.Length, ListIndentClose, 0, ListIndentClose.Length) == 0
            && (charIndex == text.Length || text[charIndex] == '\n');
        if (!enter && !behindClose) return addedChar;

        List<ListLineInfo> lines = ParseLines(text);
        int line = LineAt(text, charIndex);
        ListLineInfo current = lines[line];
        if (current.Type == ListType.None) return addedChar;

        if (!enter) {
            int position = charIndex - ListIndentClose.Length;
            Input.text = text.Insert(position, addedChar.ToString());
            Input.stringPosition = position + 1;
            return '\0'; // already in
        }

        int caretLine;
        int offset = Mathf.Clamp(charIndex - LineStart(text, charIndex) - current.Prefix, 0, current.Content.Length);
        if (current.Content.Length == 0) {
            current.Type = ListType.None;
            caretLine = line;
        } else {
            lines.Insert(line + 1, new ListLineInfo { Type = current.Type, Content = current.Content.Substring(offset) });
            current.Content = current.Content.Substring(0, offset);
            caretLine = line + 1;
        }

        string newText = BuildLines(lines, out int[] contentStarts);
        Input.text = newText;
        Input.stringPosition = contentStarts[caretLine];
        return '\0'; // the line break is already in
    }

    static List<ListLineInfo> ParseLines(string text) {
        List<ListLineInfo> lines = new List<ListLineInfo>();
        foreach (string raw in text.Split('\n')) {
            int prefix = raw.StartsWith(ListIndentClose) ? ListIndentClose.Length : 0; // stray closing tag, e.g. from a line break typed with the caret behind one
            string line = raw.Substring(prefix);
            if (line.EndsWith(ListIndentClose)) line = line.Substring(0, line.Length - ListIndentClose.Length);

            Match match = ListLine.Match(line);
            if (match.Success) {
                lines.Add(new ListLineInfo { Type = MarkerType(match.Groups[1].Value), Content = line.Substring(match.Length), Prefix = prefix + match.Length });
            } else {
                lines.Add(new ListLineInfo { Type = ListType.None, Content = line, Prefix = prefix });
            }
        }
        return lines;
    }

    // Numbers each run of items of one type from 1 / a, contentStarts gets where each line's text starts
    static string BuildLines(List<ListLineInfo> lines, out int[] contentStarts) {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        contentStarts = new int[lines.Count];
        ListType previous = ListType.None;
        int number = 0;

        for (int i = 0; i < lines.Count; i++) {
            if (i > 0) builder.Append('\n');

            ListLineInfo line = lines[i];
            if (line.Type != ListType.None) {
                number = line.Type == previous ? number + 1 : 1;
                builder.Append(ListMarker(line.Type, number)).Append(ListIndent);
            }
            contentStarts[i] = builder.Length;
            builder.Append(line.Content);
            if (line.Type != ListType.None) builder.Append(ListIndentClose); // before the line break, TMP indents the next line from its start otherwise
            previous = line.Type;
        }
        return builder.ToString();
    }

    static int LineAt(string text, int index) {
        int line = 0;
        for (int i = 0; i < index && i < text.Length; i++) if (text[i] == '\n') line++;
        return line;
    }

    static int LineStart(string text, int index) => index > 0 ? text.LastIndexOf('\n', index - 1) + 1 : 0;

    static ListType MarkerType(string marker) {
        if (marker == "•") return ListType.Bullet;
        return char.IsDigit(marker[0]) ? ListType.Number : ListType.Letter;
    }

    static string ListMarker(ListType type, int number) {
        switch (type) {
            case ListType.Number: return number + ".";
            case ListType.Letter: return Letters(number) + ".";
            default: return "•";
        }
    }

    // 1 = a, 26 = z, 27 = aa...
    static string Letters(int number) {
        string letters = "";
        while (number > 0) {
            number--;
            letters = (char)('a' + number % 26) + letters;
            number /= 26;
        }
        return letters;
    }

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
