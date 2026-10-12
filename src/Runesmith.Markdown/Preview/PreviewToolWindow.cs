using System.Composition;
using Avalonia.Controls;
using Runesmith.Markdown.Paths;
using Runesmith.Markdown.Rendering;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Settings;
using Runesmith.Sdk.Shell;
using Runesmith.Sdk.ToolWindows;
using Runesmith.Sdk.Web;

namespace Runesmith.Markdown.Preview;

/// <summary>The Markdown Preview panel, which docks beside the editors and shows the Markdown file being edited as it renders.</summary>
[Export(typeof(IToolWindowProvider))]
[Shared]
[method: ImportingConstructor]
public sealed class PreviewToolWindow(IWebViewService webViews, IEditorService editors, ISettingsService settings, IThemeService themes,
    ILauncher launcher, INotificationService notifications) : IToolWindowProvider
{
    public const string Id = "markdown.preview";

    // Built each time it is read, so the title follows the language.
    public ToolWindowDefinition Definition => new(Id, Strings.PreviewTitle, "eye", DockSide.Right);

    // The controller lives as long as the panel's content, which Runesmith keeps until it closes: the events it subscribes to hold it.
    public Control CreateContent()
    {
        var view = webViews.Create(PreviewPage.FileName);
        _ = new PreviewController(new PreviewPage(view), editors, new MarkdigRenderer(new ImageInliner()),
            new LinkOpener(launcher, editors, notifications), settings, themes, new DispatcherRefreshScheduler());
        return view.Control;
    }
}
