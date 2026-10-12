using Avalonia.Input;
using Runesmith.Markdown.Editing;
using Runesmith.Sdk.Documents;

namespace Runesmith.Markdown.Tests.Editing;

public sealed class ListContinuationHookTests
{
    private static readonly EditorKeyPress Enter = new(Key.Enter, KeyModifiers.None);

    private readonly FakeSettings settings = new();
    private readonly ListContinuationHook hook;

    public ListContinuationHookTests() => hook = new ListContinuationHook(settings);

    [Fact]
    public void EnterStartsTheNextItemAndPutsTheCaretInIt()
    {
        var editor = EditorAtEnd("# Shopping\n\n1. milk");

        Assert.True(hook.OnKeyDown(editor, Enter));

        Assert.Equal("# Shopping\n\n1. milk\n2. ", editor.Text);
        Assert.Equal(editor.Text.Length, editor.CaretOffset);
    }

    [Fact]
    public void EnterInTheMiddleOfADocumentChangesOnlyItsLine()
    {
        var editor = EditorAtEnd("- one\n- two\n\nAfter.");
        editor.CaretOffset = "- one".Length;

        Assert.True(hook.OnKeyDown(editor, Enter));

        Assert.Equal("- one\n- \n- two\n\nAfter.", editor.Text);
        Assert.Equal("- one\n- ".Length, editor.CaretOffset);
    }

    [Fact]
    public void EnterInAnEmptyItemEndsTheList()
    {
        var editor = EditorAtEnd("- milk\n- ");

        Assert.True(hook.OnKeyDown(editor, Enter));

        Assert.Equal("- milk\n", editor.Text);
        Assert.Equal(editor.Text.Length, editor.CaretOffset);
    }

    [Fact]
    public void TheChangeIsOneUndoStep()
    {
        var editor = EditorAtEnd("- milk");
        hook.OnKeyDown(editor, Enter);

        editor.Document.History.Undo(editor.Document.Buffer);

        Assert.Equal("- milk", editor.Text);
        Assert.False(editor.Document.History.CanUndo);
    }

    [Fact]
    public void LeavesEnterToTheEditorOutsideAList()
    {
        var editor = EditorAtEnd("A paragraph.");

        Assert.False(hook.OnKeyDown(editor, Enter));
        Assert.Equal("A paragraph.", editor.Text);
    }

    [Fact]
    public void LeavesOtherKeysToTheEditor()
    {
        var editor = EditorAtEnd("- milk");

        Assert.False(hook.OnKeyDown(editor, new EditorKeyPress(Key.Enter, KeyModifiers.Shift)));
        Assert.False(hook.OnKeyDown(editor, new EditorKeyPress(Key.Enter, KeyModifiers.Control)));
        Assert.False(hook.OnKeyDown(editor, new EditorKeyPress(Key.Tab, KeyModifiers.None)));
        Assert.Equal("- milk", editor.Text);
    }

    [Fact]
    public void LeavesOtherLanguagesAlone()
    {
        var editor = EditorAtEnd("- name: build", "yaml");

        Assert.False(hook.OnKeyDown(editor, Enter));
    }

    [Fact]
    public void LeavesASelectionToTheEditor()
    {
        var editor = EditorAtEnd("- milk");
        editor.Select(new Text.TextSpan(2, 4));

        Assert.False(hook.OnKeyDown(editor, Enter));
    }

    [Fact]
    public void LeavesAReadOnlyDocumentAlone()
    {
        var document = new FakeDocument("- milk") { IsReadOnly = true };
        var editor = new FakeEditorView(document) { CaretOffset = 6 };

        Assert.False(hook.OnKeyDown(editor, Enter));
        Assert.Equal("- milk", editor.Text);
    }

    [Fact]
    public void DoesNothingWhileTheSettingIsOff()
    {
        settings.Set(MarkdownSettings.ContinueLists, false);
        var editor = EditorAtEnd("- milk");

        Assert.False(hook.OnKeyDown(editor, Enter));
        Assert.Equal("- milk", editor.Text);
    }

    private static FakeEditorView EditorAtEnd(string text, string languageId = MarkdownLanguage.Id) => new(text, languageId) { CaretOffset = text.Length };
}
