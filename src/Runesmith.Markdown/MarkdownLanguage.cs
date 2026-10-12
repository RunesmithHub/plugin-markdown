using System.Composition;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Languages;

namespace Runesmith.Markdown;

/// <summary>The Markdown language: its files, highlighting, comments and brackets.</summary>
public sealed class MarkdownLanguage
{
    /// <summary>The language id of Markdown documents.</summary>
    public const string Id = "markdown";

    private const string Scope = "text.html.markdown";

    [Export(typeof(LanguageDefinition))]
    public LanguageDefinition Definition { get; } = new(Id, "Markdown")
    {
        Extensions = [".md", ".markdown", ".mkd", ".mdwn", ".mdown", ".markdn", ".mdtxt", ".mdtext"],
        ScopeName = Scope,
        BlockComment = ("<!--", "-->"),
        Brackets = [('{', '}'), ('[', ']'), ('(', ')')],
        Icon = "file-text",
    };

    /// <summary>The TextMate grammar that highlights Markdown. The code in a fence is highlighted by the grammar of its language, when a
    /// plugin brings one.</summary>
    [Export(typeof(GrammarDefinition))]
    public GrammarDefinition Grammar { get; } = new(Scope,
        Path.Combine(Path.GetDirectoryName(typeof(MarkdownLanguage).Assembly.Location) ?? "", "grammars", "markdown.tmLanguage.json"));

    internal static bool IsMarkdown(IDocument document) => string.Equals(document.LanguageId, Id, StringComparison.OrdinalIgnoreCase);
}
