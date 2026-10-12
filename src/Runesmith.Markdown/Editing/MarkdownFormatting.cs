using System.Composition;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Languages;
using Runesmith.Text;

namespace Runesmith.Markdown.Editing;

/// <summary>Format Document for Markdown: aligns the columns of its tables.</summary>
[Export(typeof(IDocumentFormattingProvider))]
[Languages(MarkdownLanguage.Id)]
public sealed class MarkdownFormatting : IDocumentFormattingProvider
{
    public Task<IReadOnlyList<TextChange>?> FormatDocumentAsync(IDocument document, TextSnapshot snapshot, FormattingOptions options,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TextChange>?>(TableFormatter.Format(snapshot.GetText()));
}
