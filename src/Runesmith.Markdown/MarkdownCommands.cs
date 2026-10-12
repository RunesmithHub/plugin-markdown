using System.Composition;
using Runesmith.Markdown.Preview;
using Runesmith.Sdk.Commands;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Languages;
using Runesmith.Sdk.ToolWindows;

namespace Runesmith.Markdown;

/// <summary>The plugin's commands: Open Preview, in the command palette and in the context menus of the editor and the Explorer.</summary>
[Export(typeof(ICommandContributor))]
[method: ImportingConstructor]
public sealed class MarkdownCommands(IEditorService editors, IToolWindowManager toolWindows, ILanguageRegistry languages) : ICommandContributor
{
    public const string OpenPreviewId = "markdown.openPreview";

    private const string MenuGroup = "markdown";

    public void Contribute(ICommandRegistry registry)
    {
        var openPreview = new CommandDefinition(OpenPreviewId, Strings.OpenPreviewTitle, Strings.MarkdownCategory)
        {
            Icon = "eye",
            KeyBinding = "Ctrl+Shift+V",
            Description = Strings.OpenPreviewDescription,
        };
        registry.Add(openPreview, OpenPreviewAsync, CanOpenPreview);
        registry.AddMenuItem(new MenuItemDefinition(ContextMenus.Editor, OpenPreviewId, MenuGroup));
        registry.AddMenuItem(new MenuItemDefinition(ContextMenus.Explorer, OpenPreviewId, MenuGroup));
    }

    // From a context menu the command is about the file the menu was opened on; from the palette or its key, about the active editor.
    private bool CanOpenPreview(object? argument) => argument is ContextMenuTarget target
        ? !target.IsDirectory && languages.GetLanguageForFile(target.Path).Id == MarkdownLanguage.Id
        : editors.ActiveEditor is { } editor && MarkdownLanguage.IsMarkdown(editor.Document);

    private async Task OpenPreviewAsync(object? argument)
    {
        if (argument is ContextMenuTarget target)
            await editors.OpenAsync(target.Path);
        toolWindows.Show(PreviewToolWindow.Id);
    }
}
