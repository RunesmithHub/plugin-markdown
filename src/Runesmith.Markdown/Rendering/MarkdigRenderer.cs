using System.Globalization;
using System.Net;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Runesmith.Markdown.Paths;

namespace Runesmith.Markdown.Rendering;

/// <summary>Renders CommonMark with the GitHub Flavored Markdown extensions: tables, task lists, strikethrough and autolinks.</summary>
/// <remarks>Raw HTML in the Markdown is shown as text rather than rendered, so a file cannot put markup of its own in the preview's
/// page.</remarks>
internal sealed class MarkdigRenderer(IImageSource images) : IMarkdownRenderer
{
    /// <summary>The attribute that holds the line a block starts on, which the page scrolls by.</summary>
    public const string LineAttribute = "data-line";

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .UsePipeTables()
        .UseTaskLists()
        .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
        .UseAutoLinks()
        .DisableHtml()
        .Build();

    public string Render(string markdown, string? baseDirectory)
    {
        var document = Markdig.Markdown.Parse(markdown, Pipeline);
        foreach (var block in document.Descendants<Block>())
            block.GetAttributes().AddProperty(LineAttribute, block.Line.ToString(CultureInfo.InvariantCulture));
        foreach (var image in document.Descendants<LinkInline>().Where(link => link.IsImage).ToList())
            Inline(image, baseDirectory);
        return document.ToHtml(Pipeline);
    }

    private void Inline(LinkInline image, string? baseDirectory)
    {
        var loaded = images.Load(image.Url ?? "", baseDirectory);
        if (loaded.Source is not null)
            image.Url = loaded.Source;
        else
            image.ReplaceBy(new HtmlInline($"<span class=\"problem\">{WebUtility.HtmlEncode(loaded.Problem)}</span>"), copyChildren: false);
    }
}
