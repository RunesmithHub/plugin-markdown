using Runesmith.Markdown.Paths;
using Runesmith.Markdown.Rendering;

namespace Runesmith.Markdown.Tests.Rendering;

public sealed class MarkdigRendererTests
{
    private readonly MarkdigRenderer renderer = new(new StubImages());

    [Theory]
    [InlineData("# Title", "<h1 id=\"title\" data-line=\"0\">Title</h1>")]
    [InlineData("###### Small", "<h6 id=\"small\" data-line=\"0\">Small</h6>")]
    [InlineData("*soft* and **strong**", "<em>soft</em> and <strong>strong</strong>")]
    [InlineData("Use `dotnet build`.", "<code>dotnet build</code>")]
    [InlineData("[Runesmith](https://runesmith.dev)", "<a href=\"https://runesmith.dev\">Runesmith</a>")]
    [InlineData("> quoted", "<blockquote data-line=\"0\">")]
    [InlineData("---", "<hr data-line=\"0\" />")]
    [InlineData("- one\n- two", "<li data-line=\"1\">two</li>")]
    [InlineData("1. one\n2. two", "<ol data-line=\"0\">")]
    public void RendersStandardMarkdown(string markdown, string expected) => Assert.Contains(expected, Render(markdown), StringComparison.Ordinal);

    [Fact]
    public void RendersAFencedCodeBlockWithItsLanguageForThePageToHighlight()
    {
        var html = Render("```csharp\nvar x = 1 < 2;\n```");

        Assert.Contains("<code class=\"language-csharp\"", html, StringComparison.Ordinal);
        Assert.Contains("var x = 1 &lt; 2;", html, StringComparison.Ordinal);
    }

    [Fact]
    public void RendersGitHubTables()
    {
        var html = Render("| Name | Size |\n| :--- | ---: |\n| a | 1 |");

        Assert.Contains("<table", html, StringComparison.Ordinal);
        Assert.Contains("<th style=\"text-align: left;\" data-line=\"0\">Name</th>", html, StringComparison.Ordinal);
        Assert.Contains("<td style=\"text-align: right;\" data-line=\"2\">1</td>", html, StringComparison.Ordinal);
    }

    [Fact]
    public void RendersGitHubTaskLists()
    {
        var html = Render("- [x] done\n- [ ] open");

        Assert.Contains("class=\"contains-task-list\"", html, StringComparison.Ordinal);
        Assert.Contains("<input disabled=\"disabled\" type=\"checkbox\" checked=\"checked\" /> done", html, StringComparison.Ordinal);
        Assert.Contains("<input disabled=\"disabled\" type=\"checkbox\" /> open", html, StringComparison.Ordinal);
    }

    [Fact]
    public void RendersGitHubStrikethroughAndAutolinks()
    {
        Assert.Contains("<del>gone</del>", Render("~~gone~~"), StringComparison.Ordinal);
        Assert.Contains("<a href=\"https://runesmith.dev\">https://runesmith.dev</a>", Render("See https://runesmith.dev for more."), StringComparison.Ordinal);
    }

    [Fact]
    public void GivesHeadingsTheIdsGitHubGivesThem() =>
        Assert.Contains("<h2 id=\"build-and-test\"", Render("## Build and test"), StringComparison.Ordinal);

    [Fact]
    public void MarksEachBlockWithTheLineItStartsOn()
    {
        var html = Render("# Title\n\nFirst.\n\nSecond.");

        Assert.Contains("<p data-line=\"2\">First.</p>", html, StringComparison.Ordinal);
        Assert.Contains("<p data-line=\"4\">Second.</p>", html, StringComparison.Ordinal);
    }

    [Fact]
    public void ShowsRawHtmlAsTextInsteadOfRunningIt()
    {
        var html = Render("<script>alert(1)</script>\n\nA <b onclick=\"x()\">bold</b> word.");

        Assert.DoesNotContain("<script", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<b ", html, StringComparison.Ordinal);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html, StringComparison.Ordinal);
    }

    [Fact]
    public void PutsALoadedImageInThePage()
    {
        var html = renderer.Render("![Logo](img/logo.png \"The logo\")", "/notes");

        Assert.Contains($"<img src=\"{StubImages.DataOf("/notes", "img/logo.png")}\" alt=\"Logo\" title=\"The logo\" />", html, StringComparison.Ordinal);
    }

    [Fact]
    public void PutsAReferenceStyleImageInThePage() =>
        Assert.Contains("<img src=\"data:image/png;base64,", renderer.Render("![Logo][logo]\n\n[logo]: img/logo.png", "/notes"), StringComparison.Ordinal);

    [Fact]
    public void SaysWhyAnImageCannotBeShownInItsPlace()
    {
        var html = Render("Before ![Chart](missing<1>.png) after.");

        Assert.DoesNotContain("<img", html, StringComparison.Ordinal);
        Assert.Equal("<p data-line=\"0\">Before <span class=\"problem\">No image at missing&lt;1&gt;.png</span> after.</p>\n", html);
    }

    [Fact]
    public void RendersAnEmptyDocumentAsNothing() => Assert.Equal("", Render(""));

    private string Render(string markdown) => renderer.Render(markdown, null);

    /// <summary>Loads every image whose name does not start with "missing"; the data names the folder and the address it was asked for,
    /// with the characters of base64 that an address must keep.</summary>
    private sealed class StubImages : IImageSource
    {
        public ImageResult Load(string address, string? baseDirectory) => address.StartsWith("missing", StringComparison.Ordinal)
            ? new ImageResult(null, $"No image at {address}")
            : new ImageResult(DataOf(baseDirectory, address), null);

        public static string DataOf(string? baseDirectory, string address) =>
            $"data:image/png;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{baseDirectory}:{address}"))}+/==";
    }
}
