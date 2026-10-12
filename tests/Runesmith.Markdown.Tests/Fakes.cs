using System.ComponentModel;
using System.Composition;
using System.Text;
using Avalonia.Controls;
using Runesmith.Markdown.Paths;
using Runesmith.Markdown.Preview;
using Runesmith.Markdown.Rendering;
using Runesmith.Sdk.Commands;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Languages;
using Runesmith.Sdk.Settings;
using Runesmith.Sdk.Shell;
using Runesmith.Sdk.ToolWindows;
using Runesmith.Sdk.Web;
using Runesmith.Text;

namespace Runesmith.Markdown.Tests;

internal sealed class FakeDocument(string text, bool hasFile = true, string languageId = MarkdownLanguage.Id) : IDocument
{
    /// <summary>The folder of every stand-in document that has a file.</summary>
    public static string Folder { get; } = Path.Combine(Path.GetTempPath(), "notes");

    public string? FilePath { get; set; } = hasFile ? Path.Combine(Folder, "readme.md") : null;

    public string Name => Path.GetFileName(FilePath) ?? "Untitled-1";

    public TextBuffer Buffer { get; } = new(text);

    public UndoHistory History { get; } = new();

    public string LanguageId { get; set; } = languageId;

    public LineEnding LineEnding { get; set; }

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public bool IsModified => !History.IsAtSavePoint;

    public bool IsReadOnly { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add { }
        remove { }
    }
}

internal sealed class FakeEditorView(FakeDocument document) : IEditorView
{
    private int caretOffset;

    public FakeEditorView(string text, string languageId = MarkdownLanguage.Id)
        : this(new FakeDocument(text, languageId: languageId))
    {
    }

    public IDocument Document => document;

    public string Text => document.Buffer.Current.GetText();

    public int CaretOffset
    {
        get => caretOffset;
        set
        {
            caretOffset = value;
            CaretMoved?.Invoke(this, EventArgs.Empty);
        }
    }

    public TextSpan Selection { get; private set; }

    public EditorCaretStyle CaretStyle { get; set; }

    public event EventHandler? CaretMoved;

    public void Select(TextSpan span) => Selection = span;

    public void ScrollTo(int offset)
    {
    }

    public void Focus()
    {
    }
}

[Export(typeof(IEditorService))]
[Shared]
internal sealed class FakeEditorService : IEditorService
{
    private readonly List<IEditorView> editors = [];

    public IEditorView? ActiveEditor { get; private set; }

    public IReadOnlyList<IEditorView> Editors => editors;

    public List<string> OpenedFiles { get; } = [];

    public event EventHandler? ActiveEditorChanged;

    public void Activate(IEditorView? editor)
    {
        if (editor is not null && !editors.Contains(editor))
            editors.Add(editor);
        ActiveEditor = editor;
        ActiveEditorChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Close(IEditorView editor, IEditorView? next = null)
    {
        editors.Remove(editor);
        Activate(next);
    }

    public Task<IEditorView?> OpenAsync(string filePath, TextPosition? position = null, bool activate = true)
    {
        OpenedFiles.Add(filePath);
        return Task.FromResult<IEditorView?>(null);
    }

    public IEditorView Open(IDocument document, bool activate = true) => throw new NotSupportedException();

    public Task<bool> CloseAsync(IEditorView editor) => throw new NotSupportedException();
}

/// <summary>Settings that start at the defaults the plugin contributes.</summary>
[Export(typeof(ISettingsService))]
[Shared]
internal sealed class FakeSettings : ISettingsService
{
    private readonly Dictionary<string, object> values = new MarkdownSettings().Settings.ToDictionary(setting => setting.Key, setting => setting.DefaultValue);

    public IReadOnlyList<SettingDefinition> Definitions => [.. new MarkdownSettings().Settings];

    public event EventHandler<SettingChangedEventArgs>? Changed;

    public T Get<T>(string key) => (T)values[key];

    public object? GetValue(string key, SettingScope scope) => values.GetValueOrDefault(key);

    public SettingScope GetEffectiveScope(string key) => SettingScope.User;

    public void Set(string key, object value, SettingScope scope = SettingScope.User)
    {
        values[key] = value;
        Changed?.Invoke(this, new SettingChangedEventArgs(key));
    }

    public void Reset(string key, SettingScope scope = SettingScope.User) => throw new NotSupportedException();
}

[Export(typeof(IThemeService))]
[Shared]
internal sealed class FakeThemes : IThemeService
{
    public bool IsDark { get; private set; }

    public event EventHandler? Changed;

    public void Toggle()
    {
        IsDark = !IsDark;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

[Export(typeof(ILauncher))]
[Shared]
internal sealed class FakeLauncher : ILauncher
{
    public List<Uri> OpenedUrls { get; } = [];

    public List<string> Revealed { get; } = [];

    public Exception? Failure { get; set; }

    public void OpenUrl(Uri url)
    {
        if (Failure is not null)
            throw Failure;
        OpenedUrls.Add(url);
    }

    public void Reveal(string path) => Revealed.Add(path);
}

[Export(typeof(INotificationService))]
[Shared]
internal sealed class FakeNotifications : INotificationService
{
    public List<(NotificationKind Kind, string Title, string? Message)> Shown { get; } = [];

    public void Notify(NotificationKind kind, string title, string? message = null, string? actionText = null, Action? action = null) =>
        Shown.Add((kind, title, message));

    public Task<bool> ConfirmAsync(string title, string message, string confirmText = "OK", bool isDestructive = false) => throw new NotSupportedException();

    public Task<int?> ChooseAsync(string title, string message, IReadOnlyList<string> choices) => throw new NotSupportedException();
}

[Export(typeof(IToolWindowManager))]
[Shared]
internal sealed class FakeToolWindows : IToolWindowManager
{
    public List<string> Shown { get; } = [];

    public IReadOnlyList<ToolWindowDefinition> ToolWindows => [];

    public void Show(string toolWindowId) => Shown.Add(toolWindowId);

    public void Hide(string toolWindowId) => Shown.Remove(toolWindowId);

    public bool IsVisible(string toolWindowId) => Shown.Contains(toolWindowId);

    public void SetBadge(string toolWindowId, string? badge)
    {
    }

    public void SetAvailable(string toolWindowId, bool available)
    {
    }
}

/// <summary>Knows Markdown by the extensions Runesmith gives it, and nothing else.</summary>
[Export(typeof(ILanguageRegistry))]
[Shared]
internal sealed class FakeLanguages : ILanguageRegistry
{
    private static readonly LanguageDefinition Markdown = new(MarkdownLanguage.Id, "Markdown") { Extensions = [".md", ".markdown"] };
    private static readonly LanguageDefinition PlainText = new(ILanguageRegistry.PlainText, "Plain Text");

    public IReadOnlyList<LanguageDefinition> Languages => [PlainText, Markdown];

    public LanguageDefinition? Find(string languageId) => Languages.FirstOrDefault(language => language.Id == languageId);

    public LanguageDefinition GetLanguageForFile(string filePath) =>
        Markdown.Extensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase) ? Markdown : PlainText;
}

[Export(typeof(IWebViewService))]
[Shared]
internal sealed class FakeWebViews : IWebViewService
{
    public List<string> Pages { get; } = [];

    public IWebView Create(string page = "index.html")
    {
        Pages.Add(page);
        return new FakeWebView();
    }
}

/// <summary>A web view without a page: it keeps what the plugin posts, and <see cref="Receive"/> stands for a message from the page.</summary>
internal sealed class FakeWebView : IWebView
{
    public List<string> Posted { get; } = [];

    public Control Control => null!;

    public bool IsAvailable => true;

    public string? UnavailableReason => null;

    public Uri? Address => null;

    public event EventHandler<WebViewMessageEventArgs>? MessageReceived;

    public event EventHandler<WebViewNavigationBlockedEventArgs>? NavigationBlocked
    {
        add { }
        remove { }
    }

    public event EventHandler? PageLoaded
    {
        add { }
        remove { }
    }

    public void Navigate(string address) => throw new NotSupportedException();

    public void PostMessage(string json) => Posted.Add(json);

    public void Receive(string json) => MessageReceived?.Invoke(this, new WebViewMessageEventArgs(json));

    public void Dispose()
    {
    }
}

/// <summary>Keeps what a command contributor registers.</summary>
internal sealed class RecordingRegistry : ICommandRegistry
{
    public Dictionary<string, (CommandDefinition Definition, Func<object?, Task> Execute, Func<object?, bool>? CanExecute)> Commands { get; } = [];

    public List<MenuItemDefinition> MenuItems { get; } = [];

    public void Add(CommandDefinition command, Func<object?, Task> execute, Func<object?, bool>? canExecute = null) =>
        Commands.Add(command.Id, (command, execute, canExecute));

    public void AddMenuItem(MenuItemDefinition item) => MenuItems.Add(item);

    public void AddMenu(MenuDefinition menu) => throw new NotSupportedException();
}

/// <summary>Keeps what the preview is told to show, as short lines such as <c>document:&lt;p&gt;Hi&lt;/p&gt;</c>.</summary>
internal sealed class FakePage : IPreviewPage
{
    public List<string> Calls { get; } = [];

    public event EventHandler<LinkClickedEventArgs>? LinkClicked;

    public void ShowDocument(string html) => Calls.Add($"document:{html}");

    public void ShowMessage(string message) => Calls.Add($"message:{message}");

    public void ShowError(string message) => Calls.Add($"error:{message}");

    public void ScrollToLine(int line) => Calls.Add($"scroll:{line}");

    public void SetTheme(bool isDark) => Calls.Add($"theme:{(isDark ? "dark" : "light")}");

    public void Click(string href) => LinkClicked?.Invoke(this, new LinkClickedEventArgs(href));
}

/// <summary>Holds scheduled actions until the test lets the delay pass.</summary>
internal sealed class FakeScheduler : IRefreshScheduler
{
    private readonly List<Scheduled> pending = [];

    public int PendingCount => pending.Count;

    public TimeSpan? LastDelay { get; private set; }

    public IDisposable Schedule(TimeSpan delay, Action action)
    {
        LastDelay = delay;
        var scheduled = new Scheduled(action, pending);
        pending.Add(scheduled);
        return scheduled;
    }

    public void Elapse()
    {
        foreach (var scheduled in pending.ToList())
        {
            pending.Remove(scheduled);
            scheduled.Action();
        }
    }

    private sealed class Scheduled(Action action, List<Scheduled> pending) : IDisposable
    {
        public Action Action { get; } = action;

        public void Dispose() => pending.Remove(this);
    }
}

/// <summary>Renders the Markdown as itself, with the folder it was given, or fails.</summary>
internal sealed class FakeRenderer : IMarkdownRenderer
{
    public Exception? Failure { get; set; }

    public string Render(string markdown, string? baseDirectory) => Failure is null ? $"{baseDirectory}|{markdown}" : throw Failure;
}

internal sealed class FakeLinkOpener : ILinkOpener
{
    public List<(string Href, string? BaseDirectory)> Opened { get; } = [];

    public Task OpenAsync(string href, string? baseDirectory)
    {
        Opened.Add((href, baseDirectory));
        return Task.CompletedTask;
    }
}

/// <summary>A folder of its own for a test's files, deleted with the test.</summary>
internal sealed class TemporaryFolder : IDisposable
{
    public string Path { get; } = Directory.CreateTempSubdirectory("runesmith-markdown-").FullName;

    public string Write(string relativePath, byte[] content)
    {
        var path = System.IO.Path.GetFullPath(relativePath, Path);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content);
        return path;
    }

    public void Dispose() => Directory.Delete(Path, recursive: true);
}
