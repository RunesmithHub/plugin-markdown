using System.Text.RegularExpressions;
using Runesmith.Markdown.Preview;

namespace Runesmith.Markdown.Tests.Preview;

/// <summary>Checks the preview's page in the repository's web folder, which the build copies into the plugin as it is.</summary>
public sealed partial class WebFolderTests
{
    private static readonly string WebFolder = Path.Combine(RepositoryRoot(), "web");

    [Fact]
    public void ThePageThePanelShowsExists() => Assert.True(File.Exists(Path.Combine(WebFolder, PreviewPage.FileName)));

    [Fact]
    public void ThePageLoadsOnlyFilesOfTheWebFolderAndRunesmithsMessageScript()
    {
        var references = Reference().Matches(File.ReadAllText(Path.Combine(WebFolder, PreviewPage.FileName))).Select(match => match.Groups[1].Value).ToList();

        Assert.Contains("/runesmith/webview.js", references);
        Assert.All(references.Where(reference => reference != "/runesmith/webview.js"), reference => Assert.True(File.Exists(Path.Combine(WebFolder, reference)), reference));
    }

    [Fact]
    public void ThePageHasNoInlineScriptWhichRunesmithWouldBlock() =>
        Assert.DoesNotMatch(InlineScript(), File.ReadAllText(Path.Combine(WebFolder, PreviewPage.FileName)));

    private static string RepositoryRoot()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "plugin.json")))
            folder = folder.Parent;
        return folder?.FullName ?? throw new InvalidOperationException("plugin.json was not found above the tests.");
    }

    [GeneratedRegex(@"(?:src|href)=""([^""]+)""")]
    private static partial Regex Reference();

    [GeneratedRegex(@"<script(?![^>]*\bsrc=)|\son\w+=")]
    private static partial Regex InlineScript();
}
