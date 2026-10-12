using System.Globalization;
using System.Text.RegularExpressions;

namespace Runesmith.Markdown.Editing;

/// <summary>A change to one line: the text from <paramref name="Start"/>, <paramref name="Length"/> characters long, becomes
/// <paramref name="Text"/>.</summary>
internal readonly record struct LineEdit(int Start, int Length, string Text);

/// <summary>Decides what Enter does in a list item: it starts the next item, or ends the list when the item is empty.</summary>
internal static partial class ListContinuation
{
    /// <summary>Gets what Enter changes with the caret at a column of a line; null when the line is not a list item or the caret is before
    /// the item's text, so Enter works as usual.</summary>
    public static LineEdit? At(string line, int column)
    {
        var item = ListItem().Match(line);
        if (!item.Success || column < item.Length)
            return null;

        var content = line.AsSpan(item.Length);
        if (content.IsWhiteSpace())
            return new LineEdit(0, line.Length, "");

        // "* * *" and "- - -" are thematic breaks, not list items.
        if (content.IndexOfAnyExcept("*-_ \t") < 0)
            return null;

        var marker = item.Groups["number"].Success
            ? (int.Parse(item.Groups["number"].ValueSpan, CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture) + item.Groups["delimiter"].Value
            : item.Groups["bullet"].Value;
        var task = item.Groups["task"].Success ? "[ ] " : "";
        return new LineEdit(column, 0, $"\n{item.Groups["indent"].Value}{marker}{item.Groups["space"].Value}{task}");
    }

    [GeneratedRegex(@"^(?<indent>[ \t]*)(?:(?<bullet>[-*+])|(?<number>\d{1,9})(?<delimiter>[.)]))(?<space>[ \t]+)(?<task>\[[ xX]\][ \t]+)?")]
    private static partial Regex ListItem();
}
