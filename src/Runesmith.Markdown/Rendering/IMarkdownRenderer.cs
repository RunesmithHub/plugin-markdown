namespace Runesmith.Markdown.Rendering;

/// <summary>Turns Markdown into the HTML the preview shows.</summary>
internal interface IMarkdownRenderer
{
    /// <summary>Renders Markdown to an HTML fragment. Each block carries the line it starts on, counted from zero, in a <c>data-line</c>
    /// attribute, and each image is inside the fragment or replaced by a message saying why it cannot be shown.</summary>
    /// <param name="baseDirectory">The folder relative image addresses start from, or null for a document that has no file yet.</param>
    string Render(string markdown, string? baseDirectory);
}
