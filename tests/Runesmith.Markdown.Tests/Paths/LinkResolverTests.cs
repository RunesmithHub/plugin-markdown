using Runesmith.Markdown.Paths;

namespace Runesmith.Markdown.Tests.Paths;

public sealed class LinkResolverTests
{
    private static readonly string Root = OperatingSystem.IsWindows() ? @"C:\" : "/";
    private static readonly string Docs = Path.Combine(Root, "repo", "docs");

    [Theory]
    [InlineData("guide.md", "repo/docs/guide.md")]
    [InlineData("./guide.md", "repo/docs/guide.md")]
    [InlineData("img/logo.png", "repo/docs/img/logo.png")]
    [InlineData("../README.md", "repo/README.md")]
    [InlineData("../../outside.md", "outside.md")]
    [InlineData("guide.md#install", "repo/docs/guide.md")]
    [InlineData("img/logo.png?raw=true", "repo/docs/img/logo.png")]
    [InlineData("my%20notes/to%20do.md", "repo/docs/my notes/to do.md")]
    [InlineData("  guide.md  ", "repo/docs/guide.md")]
    public void ResolvesARelativePathFromTheFolderOfTheMarkdownFile(string href, string expected) =>
        Assert.Equal(new FileLink(Path.Combine(Root, expected.Replace('/', Path.DirectorySeparatorChar))), LinkResolver.Resolve(href, Docs));

    [Fact]
    public void KeepsARootedPath()
    {
        var rooted = Path.Combine(Root, "etc", "notes.md");

        Assert.Equal(new FileLink(rooted), LinkResolver.Resolve(rooted, Docs));
        Assert.Equal(new FileLink(rooted), LinkResolver.Resolve(rooted, null));
    }

    [Theory]
    [InlineData("https://runesmith.dev/docs?page=2#top")]
    [InlineData("http://localhost:8080")]
    [InlineData("HTTPS://RUNESMITH.DEV")]
    public void ResolvesAWebAddress(string href) => Assert.Equal(new WebLink(new Uri(href)), LinkResolver.Resolve(href, Docs));

    [Fact]
    public void ResolvesAFileAddress()
    {
        var path = Path.Combine(Root, "repo", "a b.md");

        Assert.Equal(new FileLink(path), LinkResolver.Resolve(new Uri(path).AbsoluteUri, Docs));
    }

    [Theory]
    [InlineData("mailto:someone@example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/png;base64,AAAA")]
    [InlineData("")]
    [InlineData("#install")]
    [InlineData("?raw=true")]
    public void DoesNotResolveOtherAddresses(string href) => Assert.Equal(new UnsupportedLink(href), LinkResolver.Resolve(href, Docs));

    [Fact]
    public void DoesNotResolveARelativePathInADocumentWithoutAFile() =>
        Assert.Equal(new UnsupportedLink("guide.md"), LinkResolver.Resolve("guide.md", null));
}
