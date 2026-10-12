using Runesmith.Markdown.Preview;

namespace Runesmith.Markdown.Tests.Preview;

public sealed class PreviewControllerTests
{
    private const string Empty = "message:Open a Markdown file to preview it.";

    private readonly FakePage page = new();
    private readonly FakeEditorService editors = new();
    private readonly FakeRenderer renderer = new();
    private readonly FakeLinkOpener links = new();
    private readonly FakeSettings settings = new();
    private readonly FakeThemes themes = new();
    private readonly FakeScheduler scheduler = new();

    [Fact]
    public void SaysWhatToDoWhileNoMarkdownFileIsOpen()
    {
        editors.Activate(new FakeEditorView("class C;", "csharp"));

        Start();

        Assert.Equal(["theme:light", Empty], page.Calls);
    }

    [Fact]
    public void ShowsTheActiveMarkdownEditorWhenThePanelOpens()
    {
        editors.Activate(new FakeEditorView("# Title"));

        Start();

        Assert.Equal(["theme:light", Empty, Rendered("# Title"), "scroll:0"], page.Calls);
    }

    [Fact]
    public void ShowsAMarkdownEditorWhenItBecomesActive()
    {
        Start();
        page.Calls.Clear();

        editors.Activate(new FakeEditorView("First"));
        editors.Activate(new FakeEditorView("Second"));

        Assert.Equal([Rendered("First"), "scroll:0", Rendered("Second"), "scroll:0"], page.Calls);
    }

    [Fact]
    public void KeepsTheMarkdownEditorWhileAnotherKindOfFileIsActive()
    {
        var markdown = new FakeEditorView("# Title");
        editors.Activate(markdown);
        Start();
        page.Calls.Clear();

        editors.Activate(new FakeEditorView("class C;", "csharp"));
        Type(markdown, " again");

        Assert.Equal([Rendered("# Title again")], page.Calls);
    }

    [Fact]
    public void EmptiesWhenTheMarkdownEditorCloses()
    {
        var markdown = new FakeEditorView("# Title");
        editors.Activate(markdown);
        Start();
        page.Calls.Clear();

        editors.Close(markdown, new FakeEditorView("class C;", "csharp"));
        Type(markdown, " again");

        Assert.Equal([Empty], page.Calls);
        Assert.Equal(0, scheduler.PendingCount);
    }

    [Fact]
    public void RendersOnceAfterTypingPauses()
    {
        var markdown = new FakeEditorView("a");
        editors.Activate(markdown);
        Start();
        page.Calls.Clear();

        markdown.Document.Buffer.Insert(1, "b");
        markdown.Document.Buffer.Insert(2, "c");

        Assert.Empty(page.Calls);
        Assert.Equal(1, scheduler.PendingCount);
        Assert.Equal(TimeSpan.FromMilliseconds(200), scheduler.LastDelay);

        scheduler.Elapse();

        Assert.Equal([Rendered("abc")], page.Calls);
    }

    [Fact]
    public void WaitsAsLongAsTheSettingSays()
    {
        var markdown = new FakeEditorView("a");
        editors.Activate(markdown);
        Start();
        settings.Set(MarkdownSettings.RefreshDelay, 750);

        markdown.Document.Buffer.Insert(1, "b");

        Assert.Equal(TimeSpan.FromMilliseconds(750), scheduler.LastDelay);
    }

    [Fact]
    public void StopsFollowingTheEditorItLeft()
    {
        var first = new FakeEditorView("First");
        editors.Activate(first);
        Start();
        editors.Activate(new FakeEditorView("Second"));
        page.Calls.Clear();

        first.Document.Buffer.Insert(0, "!");
        first.CaretOffset = 1;

        Assert.Empty(page.Calls);
        Assert.Equal(0, scheduler.PendingCount);
    }

    [Fact]
    public void RendersADocumentWithoutAFileWithNoFolderForItsImages()
    {
        editors.Activate(new FakeEditorView(new FakeDocument("Draft", hasFile: false)));

        Start();

        Assert.Contains("document:|Draft", page.Calls);
    }

    [Fact]
    public void SaysWhyThePreviewCouldNotBeRendered()
    {
        renderer.Failure = new InvalidOperationException("The parser gave up.");
        editors.Activate(new FakeEditorView("# Title"));

        Start();

        Assert.Contains("error:The preview could not be rendered: The parser gave up.", page.Calls);
    }

    [Fact]
    public void RendersAgainAfterAFailure()
    {
        var markdown = new FakeEditorView("# Title");
        renderer.Failure = new InvalidOperationException("The parser gave up.");
        editors.Activate(markdown);
        Start();
        page.Calls.Clear();

        renderer.Failure = null;
        Type(markdown, "!");

        Assert.Equal([Rendered("# Title!")], page.Calls);
    }

    [Fact]
    public void ScrollsToTheLineOfTheCaret()
    {
        var markdown = new FakeEditorView("# Title\n\nFirst.\n\nSecond.");
        editors.Activate(markdown);
        Start();
        page.Calls.Clear();

        markdown.CaretOffset = markdown.Text.IndexOf("Second", StringComparison.Ordinal) + 3;

        Assert.Equal(["scroll:4"], page.Calls);
    }

    [Fact]
    public void DoesNotScrollWhileTheSettingIsOff()
    {
        var markdown = new FakeEditorView("# Title\n\nFirst.");
        settings.Set(MarkdownSettings.SyncScroll, false);
        editors.Activate(markdown);
        Start();

        markdown.CaretOffset = 10;

        Assert.DoesNotContain(page.Calls, call => call.StartsWith("scroll:", StringComparison.Ordinal));
    }

    [Fact]
    public void FollowsTheTheme()
    {
        Start();
        page.Calls.Clear();

        themes.Toggle();

        Assert.Equal(["theme:dark"], page.Calls);
    }

    [Fact]
    public void OpensAClickedLinkFromTheFolderOfTheMarkdownFile()
    {
        editors.Activate(new FakeEditorView("[Guide](guide.md)"));
        Start();

        page.Click("guide.md");

        Assert.Equal([("guide.md", FakeDocument.Folder)], links.Opened);
    }

    [Fact]
    public void NeverChangesTheDocument()
    {
        var markdown = new FakeEditorView("| a | b |\n|---|---|\n\n- item");
        editors.Activate(markdown);
        Start();

        markdown.CaretOffset = 3;
        Type(markdown, "!");
        themes.Toggle();

        Assert.Equal("| a | b |\n|---|---|\n\n- item!", markdown.Text);
        Assert.False(markdown.Document.History.CanUndo);
    }

    // What the page is told to show for a document in the folder the stand-in documents are in.
    private static string Rendered(string markdown) => $"document:{FakeDocument.Folder}|{markdown}";

    private void Start() => _ = new PreviewController(page, editors, renderer, links, settings, themes, scheduler);

    private void Type(FakeEditorView editor, string text)
    {
        editor.Document.Buffer.Insert(editor.Text.Length, text);
        scheduler.Elapse();
    }
}
