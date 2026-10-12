using System.Text.RegularExpressions;
using Runesmith.Text;

namespace Runesmith.Markdown.Editing;

/// <summary>Aligns the columns of pipe tables, so every cell of a column is as wide as its widest one.</summary>
/// <remarks>It changes only the spaces around cells and the dashes of the delimiter row. A table with a row that has more cells than its
/// header is left as it is, since the extra cells are not part of the table.</remarks>
internal static partial class TableFormatter
{
    private const int MinimumWidth = 3;

    private enum Alignment
    {
        None,
        Left,
        Center,
        Right,
    }

    /// <summary>Gets the changes that format every table of a text whose line breaks are <c>\n</c>; empty when they are formatted
    /// already.</summary>
    public static IReadOnlyList<TextChange> Format(string text)
    {
        var lines = text.Split('\n');
        var starts = new int[lines.Length];
        for (var i = 1; i < lines.Length; i++)
            starts[i] = starts[i - 1] + lines[i - 1].Length + 1;

        var changes = new List<TextChange>();
        var inCodeBlock = false;
        for (var i = 0; i < lines.Length; i++)
        {
            if (Fence().IsMatch(lines[i]))
                inCodeBlock = !inCodeBlock;
            if (inCodeBlock || i + 1 >= lines.Length || !IsTableStart(lines[i], lines[i + 1]))
                continue;

            var end = i + 2;
            while (end < lines.Length && lines[end].Contains('|') && !string.IsNullOrWhiteSpace(lines[end]))
                end++;

            var formatted = FormatTable(lines[i..end]);
            for (var row = 0; row < formatted.Length; row++)
            {
                if (formatted[row] != lines[i + row])
                    changes.Add(new TextChange(new TextSpan(starts[i + row], lines[i + row].Length), formatted[row]));
            }

            i = end - 1;
        }

        return changes;
    }

    private static bool IsTableStart(string header, string delimiter)
    {
        if (!header.Contains('|') || !delimiter.Contains('|'))
            return false;

        var cells = SplitCells(delimiter);
        return cells.Count == SplitCells(header).Count && cells.All(cell => DelimiterCell().IsMatch(cell));
    }

    private static string[] FormatTable(string[] lines)
    {
        var rows = lines.Select(SplitCells).ToList();
        var columns = rows[0].Count;
        if (rows.Any(row => row.Count > columns))
            return lines;

        var alignments = rows[1].Select(AlignmentOf).ToList();
        var widths = Enumerable.Range(0, columns)
            .Select(column => Math.Max(MinimumWidth, rows.Where((row, index) => index != 1 && column < row.Count).Max(row => row[column].Length)))
            .ToList();

        return [.. lines.Select((line, index) =>
        {
            var cells = Enumerable.Range(0, columns).Select(column => index == 1
                ? Delimiter(alignments[column], widths[column])
                : Pad(column < rows[index].Count ? rows[index][column] : "", alignments[column], widths[column]));
            return $"{line[..(line.Length - line.TrimStart().Length)]}| {string.Join(" | ", cells)} |";
        })];
    }

    // The cells between the pipes, without the pipes at the row's ends; "\|" is a pipe inside a cell.
    private static List<string> SplitCells(string line)
    {
        var row = line.Trim();
        if (row.StartsWith('|'))
            row = row[1..];
        if (row.EndsWith('|') && !row.EndsWith(@"\|", StringComparison.Ordinal))
            row = row[..^1];
        return [.. CellSeparator().Split(row).Select(cell => cell.Trim())];
    }

    private static Alignment AlignmentOf(string delimiterCell) => (delimiterCell.StartsWith(':'), delimiterCell.EndsWith(':')) switch
    {
        (true, true) => Alignment.Center,
        (true, false) => Alignment.Left,
        (false, true) => Alignment.Right,
        _ => Alignment.None,
    };

    private static string Delimiter(Alignment alignment, int width) => alignment switch
    {
        Alignment.Center => $":{new string('-', width - 2)}:",
        Alignment.Left => $":{new string('-', width - 1)}",
        Alignment.Right => $"{new string('-', width - 1)}:",
        _ => new string('-', width),
    };

    private static string Pad(string cell, Alignment alignment, int width) => alignment switch
    {
        Alignment.Right => cell.PadLeft(width),
        Alignment.Center => cell.PadLeft(cell.Length + ((width - cell.Length) / 2)).PadRight(width),
        _ => cell.PadRight(width),
    };

    [GeneratedRegex(@"^ {0,3}(```|~~~)")]
    private static partial Regex Fence();

    [GeneratedRegex(@"^:?-+:?$")]
    private static partial Regex DelimiterCell();

    [GeneratedRegex(@"(?<!\\)\|")]
    private static partial Regex CellSeparator();
}
