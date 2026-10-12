using System.Text.Json.Nodes;
using Runesmith.Markdown.Preview;

namespace Runesmith.Markdown.Tests.Preview;

public sealed class PreviewPageTests : IDisposable
{
    private readonly FakeWebView view = new();
    private readonly PreviewPage page;
    private readonly List<string> clicked = [];

    public PreviewPageTests()
    {
        page = new PreviewPage(view);
        page.LinkClicked += (_, e) => clicked.Add(e.Href);
    }

    public void Dispose() => view.Dispose();

    [Fact]
    public void SendsTheMessagesThePageUnderstands()
    {
        page.ShowDocument("<p data-line=\"0\">\"Hi\"</p>");
        page.ShowMessage("Open a file.");
        page.ShowError("It failed.");
        page.ScrollToLine(12);
        page.SetTheme(true);

        Assert.Equal(
        [
            ("document", "html", "<p data-line=\"0\">\"Hi\"</p>"),
            ("message", "text", "Open a file."),
            ("error", "text", "It failed."),
            ("scroll", "line", "12"),
            ("theme", "dark", "true"),
        ], view.Posted.Select(Read));
    }

    [Fact]
    public void TellsWhichLinkWasClicked()
    {
        view.Receive("""{"kind":"link","href":"../docs/guide.md#install"}""");

        Assert.Equal(["../docs/guide.md#install"], clicked);
    }

    [Theory]
    [InlineData("""{"kind":"link"}""")]
    [InlineData("""{"kind":"link","href":""}""")]
    [InlineData("""{"kind":"link","href":42}""")]
    [InlineData("""{"kind":"other","href":"guide.md"}""")]
    [InlineData("""["link","guide.md"]""")]
    [InlineData("null")]
    [InlineData("not json")]
    public void IgnoresMessagesItDoesNotUnderstand(string json)
    {
        view.Receive(json);

        Assert.Empty(clicked);
    }

    // A message as the page reads it: its kind, and the name and value of what it carries.
    private static (string Kind, string Name, string Value) Read(string json)
    {
        var message = JsonNode.Parse(json)!.AsObject();
        var (name, value) = Assert.Single(message, property => property.Key != "kind");
        return ((string)message["kind"]!, name, value!.ToString());
    }
}
