using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Runesmith.Sdk.Languages;
using Runesmith.Text;

namespace Runesmith.Markdown.Editing;

/// <summary>Reads the headings of a Markdown text as a tree: each heading holds the headings of the levels below it that follow it.</summary>
internal static class HeadingOutline
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePreciseSourceLocation().Build();

    /// <summary>Gets the top headings, each with its children. A heading's range runs to the next heading of its level or above.</summary>
    public static IReadOnlyList<DocumentSymbol> Build(string text)
    {
        var headings = Markdig.Markdown.Parse(text, Pipeline).Descendants<HeadingBlock>().Where(heading => TitleOf(heading).Length > 0).ToList();
        var next = 0;
        return Children(headings, ref next, 0, text.Length);
    }

    private static List<DocumentSymbol> Children(List<HeadingBlock> headings, ref int next, int parentLevel, int textLength)
    {
        var symbols = new List<DocumentSymbol>();
        while (next < headings.Count && headings[next].Level > parentLevel)
        {
            var heading = headings[next++];
            var children = Children(headings, ref next, heading.Level, textLength);
            var end = next < headings.Count ? headings[next].Span.Start : textLength;
            symbols.Add(new DocumentSymbol(TitleOf(heading), SymbolKind.String, TextSpan.FromBounds(heading.Span.Start, end),
                TextSpan.FromBounds(heading.Span.Start, heading.Span.End + 1))
            {
                Detail = new string('#', heading.Level),
                Children = children,
            });
        }

        return symbols;
    }

    // The heading's text without its markup, so "# The `Run` *menu*" is listed as "The Run menu".
    private static string TitleOf(HeadingBlock heading)
    {
        var title = new StringBuilder();
        foreach (var inline in heading.Inline?.Descendants() ?? [])
        {
            title.Append(inline switch
            {
                LiteralInline literal => literal.Content.ToString(),
                CodeInline code => code.Content,
                _ => "",
            });
        }

        return title.ToString().Trim();
    }
}
