using System.Composition;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Languages;
using Runesmith.Text;

namespace Runesmith.Markdown.Editing;

/// <summary>The headings of a Markdown document, for the Outline, the breadcrumbs and Go to Symbol in File.</summary>
[Export(typeof(IDocumentSymbolProvider))]
[Languages(MarkdownLanguage.Id)]
public sealed class MarkdownSymbols : IDocumentSymbolProvider
{
    public Task<IReadOnlyList<DocumentSymbol>?> GetDocumentSymbolsAsync(IDocument document, TextSnapshot snapshot, CancellationToken cancellationToken) =>
        Task.Run<IReadOnlyList<DocumentSymbol>?>(() => HeadingOutline.Build(snapshot.GetText()), cancellationToken);
}
