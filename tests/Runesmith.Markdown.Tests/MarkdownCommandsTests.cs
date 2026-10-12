using Runesmith.Markdown.Preview;
using Runesmith.Sdk.Commands;

namespace Runesmith.Markdown.Tests;

public sealed class MarkdownCommandsTests
{
    private readonly FakeEditorService editors = new();
    private readonly FakeToolWindows toolWindows = new();
    private readonly RecordingRegistry registry = new();

    public MarkdownCommandsTests() => new MarkdownCommands(editors, toolWindows, new FakeLanguages()).Contribute(registry);

    [Fact]
    public void RegistersOpenPreviewForThePaletteWithAKey()
    {
        var command = Assert.Single(registry.Commands).Value.Definition;

        Assert.Equal("markdown.openPreview", command.Id);
        Assert.Equal("Open Preview", command.Title);
        Assert.Equal("Markdown", command.Category);
        Assert.Equal("Ctrl+Shift+V", command.KeyBinding);
        Assert.True(command.ShowInPalette);
        Assert.False(string.IsNullOrEmpty(command.Description));
    }

    [Fact]
    public void PlacesOpenPreviewInTheContextMenusOfTheEditorAndTheExplorer() => Assert.Equal(
        [new MenuItemDefinition(ContextMenus.Editor, MarkdownCommands.OpenPreviewId, "markdown"), new MenuItemDefinition(ContextMenus.Explorer, MarkdownCommands.OpenPreviewId, "markdown")],
        registry.MenuItems);

    [Fact]
    public void RunsOnlyWhileAMarkdownEditorIsActive()
    {
        Assert.False(CanOpenPreview(null));

        editors.Activate(new FakeEditorView("class C;", "csharp"));
        Assert.False(CanOpenPreview(null));

        editors.Activate(new FakeEditorView("# Title"));
        Assert.True(CanOpenPreview(null));
    }

    [Theory]
    [InlineData("/repo/README.md", false, true)]
    [InlineData("/repo/notes.MARKDOWN", false, true)]
    [InlineData("/repo/Program.cs", false, false)]
    [InlineData("/repo/docs.md", true, false)]
    public void ShowsInAContextMenuOnlyOnMarkdownFiles(string path, bool isDirectory, bool expected) =>
        Assert.Equal(expected, CanOpenPreview(new ContextMenuTarget(path, isDirectory)));

    [Fact]
    public async Task ShowsThePreviewPanel()
    {
        editors.Activate(new FakeEditorView("# Title"));

        await OpenPreviewAsync(null);

        Assert.Equal([PreviewToolWindow.Id], toolWindows.Shown);
        Assert.Empty(editors.OpenedFiles);
    }

    [Fact]
    public async Task OpensTheFileOfAContextMenuBeforeItShowsThePanel()
    {
        await OpenPreviewAsync(new ContextMenuTarget("/repo/README.md"));

        Assert.Equal(["/repo/README.md"], editors.OpenedFiles);
        Assert.Equal([PreviewToolWindow.Id], toolWindows.Shown);
    }

    private bool CanOpenPreview(object? argument) => registry.Commands[MarkdownCommands.OpenPreviewId].CanExecute!(argument);

    private Task OpenPreviewAsync(object? argument) => registry.Commands[MarkdownCommands.OpenPreviewId].Execute(argument);
}
