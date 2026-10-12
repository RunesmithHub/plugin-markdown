using System.Text.Json;
using System.Text.Json.Nodes;
using Runesmith.Sdk.Web;

namespace Runesmith.Markdown.Preview;

/// <summary>The page that shows the preview, as the plugin sees it: what it can be told to show, and what the user does in it.</summary>
internal interface IPreviewPage
{
    /// <summary>Shows a rendered document, keeping the scroll position.</summary>
    void ShowDocument(string html);

    /// <summary>Shows a line of text instead of a document, such as while no Markdown file is open.</summary>
    void ShowMessage(string message);

    /// <summary>Shows why the document cannot be shown.</summary>
    void ShowError(string message);

    /// <summary>Scrolls to the block that holds a line of the Markdown, counted from zero, unless it is in view already.</summary>
    void ScrollToLine(int line);

    void SetTheme(bool isDark);

    /// <summary>Raised when the user clicks a link that leads out of the document.</summary>
    event EventHandler<LinkClickedEventArgs>? LinkClicked;
}

/// <summary>A link the user clicked in the preview.</summary>
/// <param name="href">The link's address as the Markdown writes it.</param>
internal sealed class LinkClickedEventArgs(string href) : EventArgs
{
    public string Href { get; } = href;
}

/// <summary>Talks to <c>web/preview.html</c> in a web view, in the JSON messages <c>web/preview.js</c> understands.</summary>
internal sealed class PreviewPage : IPreviewPage
{
    /// <summary>The page in the plugin's web folder.</summary>
    public const string FileName = "preview.html";

    private readonly IWebView view;

    public PreviewPage(IWebView view)
    {
        this.view = view;
        view.MessageReceived += OnMessageReceived;
    }

    public event EventHandler<LinkClickedEventArgs>? LinkClicked;

    public void ShowDocument(string html) => Post(new JsonObject { ["kind"] = "document", ["html"] = html });

    public void ShowMessage(string message) => Post(new JsonObject { ["kind"] = "message", ["text"] = message });

    public void ShowError(string message) => Post(new JsonObject { ["kind"] = "error", ["text"] = message });

    public void ScrollToLine(int line) => Post(new JsonObject { ["kind"] = "scroll", ["line"] = line });

    public void SetTheme(bool isDark) => Post(new JsonObject { ["kind"] = "theme", ["dark"] = isDark });

    private void Post(JsonObject message) => view.PostMessage(message.ToJsonString());

    // The page's only message is a clicked link; anything else, such as a message from a newer page, is ignored.
    private void OnMessageReceived(object? sender, WebViewMessageEventArgs e)
    {
        try
        {
            if (JsonNode.Parse(e.Json) is JsonObject message && (string?)message["kind"] == "link" && (string?)message["href"] is { Length: > 0 } href)
                LinkClicked?.Invoke(this, new LinkClickedEventArgs(href));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
        }
    }
}
