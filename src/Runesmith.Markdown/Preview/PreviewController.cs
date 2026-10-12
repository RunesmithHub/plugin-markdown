using Runesmith.Markdown.Paths;
using Runesmith.Markdown.Rendering;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Settings;
using Runesmith.Sdk.Shell;

namespace Runesmith.Markdown.Preview;

/// <summary>Keeps the preview showing the Markdown editor the user works in: it follows the active editor, renders again once typing
/// pauses, scrolls with the caret and opens clicked links.</summary>
/// <remarks>The preview stays on the last Markdown editor while another kind of file is active, and empties when that editor closes. It
/// only reads the document; nothing here changes the text.</remarks>
internal sealed class PreviewController
{
    private readonly IPreviewPage page;
    private readonly IEditorService editors;
    private readonly IMarkdownRenderer renderer;
    private readonly ILinkOpener links;
    private readonly ISettingsService settings;
    private readonly IThemeService themes;
    private readonly IRefreshScheduler scheduler;
    private IEditorView? editor;
    private IDisposable? pendingRefresh;

    public PreviewController(IPreviewPage page, IEditorService editors, IMarkdownRenderer renderer, ILinkOpener links, ISettingsService settings,
        IThemeService themes, IRefreshScheduler scheduler)
    {
        this.page = page;
        this.editors = editors;
        this.renderer = renderer;
        this.links = links;
        this.settings = settings;
        this.themes = themes;
        this.scheduler = scheduler;

        page.LinkClicked += OnLinkClicked;
        editors.ActiveEditorChanged += OnActiveEditorChanged;
        themes.Changed += OnThemeChanged;

        page.SetTheme(themes.IsDark);
        page.ShowMessage(Strings.PreviewEmpty);
        OnActiveEditorChanged(editors, EventArgs.Empty);
    }

    private string? BaseDirectory => Path.GetDirectoryName(editor?.Document.FilePath);

    private void OnActiveEditorChanged(object? sender, EventArgs e)
    {
        if (editors.ActiveEditor is { } active && MarkdownLanguage.IsMarkdown(active.Document))
            Follow(active);
        else if (editor is not null && !editors.Editors.Contains(editor))
            Follow(null);
    }

    private void Follow(IEditorView? next)
    {
        if (ReferenceEquals(next, editor))
            return;

        pendingRefresh?.Dispose();
        pendingRefresh = null;
        if (editor is not null)
        {
            editor.Document.Buffer.Changed -= OnTextChanged;
            editor.CaretMoved -= OnCaretMoved;
        }

        editor = next;
        if (editor is null)
        {
            page.ShowMessage(Strings.PreviewEmpty);
            return;
        }

        editor.Document.Buffer.Changed += OnTextChanged;
        editor.CaretMoved += OnCaretMoved;
        Render();
        ScrollToCaret();
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        pendingRefresh?.Dispose();
        pendingRefresh = scheduler.Schedule(TimeSpan.FromMilliseconds(settings.Get<int>(MarkdownSettings.RefreshDelay)), Render);
    }

    private void OnCaretMoved(object? sender, EventArgs e) => ScrollToCaret();

    private void OnThemeChanged(object? sender, EventArgs e) => page.SetTheme(themes.IsDark);

    private async void OnLinkClicked(object? sender, LinkClickedEventArgs e) => await links.OpenAsync(e.Href, BaseDirectory);

    private void Render()
    {
        if (editor is null)
            return;

        try
        {
            page.ShowDocument(renderer.Render(editor.Document.Buffer.Current.GetText(), BaseDirectory));
        }
#pragma warning disable CA1031 // Whatever the renderer fails on, the editor goes on and the preview says why it is empty.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            page.ShowError(Strings.PreviewFailed(exception.Message));
        }
    }

    private void ScrollToCaret()
    {
        if (editor is not null && settings.Get<bool>(MarkdownSettings.SyncScroll))
            page.ScrollToLine(editor.Document.Buffer.Current.GetPosition(editor.CaretOffset).Line);
    }
}
