using Runesmith.Markdown.Paths;
using Runesmith.Sdk.Shell;

namespace Runesmith.Markdown.Tests.Paths;

public sealed class LinkOpenerTests : IDisposable
{
    private readonly TemporaryFolder folder = new();
    private readonly FakeLauncher launcher = new();
    private readonly FakeEditorService editors = new();
    private readonly FakeNotifications notifications = new();
    private readonly LinkOpener opener;

    public LinkOpenerTests() => opener = new LinkOpener(launcher, editors, notifications);

    public void Dispose() => folder.Dispose();

    [Fact]
    public async Task OpensAWebLinkInTheBrowser()
    {
        await opener.OpenAsync("https://runesmith.dev/docs", folder.Path);

        Assert.Equal([new Uri("https://runesmith.dev/docs")], launcher.OpenedUrls);
        Assert.Empty(notifications.Shown);
    }

    [Fact]
    public async Task OpensALinkedFileInAnEditor()
    {
        var guide = folder.Write("docs/guide.md", []);

        await opener.OpenAsync("docs/guide.md#install", folder.Path);

        Assert.Equal([guide], editors.OpenedFiles);
    }

    [Fact]
    public async Task ShowsALinkedFolderInTheFileManager()
    {
        folder.Write("docs/guide.md", []);

        await opener.OpenAsync("docs", folder.Path);

        Assert.Equal([Path.Combine(folder.Path, "docs")], launcher.Revealed);
    }

    [Fact]
    public async Task SaysWhenTheLinkedFileDoesNotExist()
    {
        await opener.OpenAsync("gone.md", folder.Path);

        Assert.Empty(editors.OpenedFiles);
        Assert.Equal([(NotificationKind.Warning, "The linked file does not exist", Path.Combine(folder.Path, "gone.md"))], notifications.Shown);
    }

    [Fact]
    public async Task SaysWhenTheKindOfLinkCannotBeOpened()
    {
        await opener.OpenAsync("mailto:someone@example.com", folder.Path);

        Assert.Equal([(NotificationKind.Warning, "This kind of link cannot be opened", "mailto:someone@example.com")], notifications.Shown);
    }

    [Fact]
    public async Task SaysWhenTheLinkCannotBeOpened()
    {
        launcher.Failure = new UnauthorizedAccessException("The plugin may not open links.");

        await opener.OpenAsync("https://runesmith.dev", folder.Path);

        Assert.Equal([(NotificationKind.Error, "The link could not be opened", "The plugin may not open links.")], notifications.Shown);
    }
}
