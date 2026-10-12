using System.Composition;
using Avalonia.Input;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Settings;
using Runesmith.Text;

namespace Runesmith.Markdown.Editing;

/// <summary>Continues Markdown lists as the user presses Enter, while the Continue lists setting is on.</summary>
[Export(typeof(IEditorKeyHook))]
[Shared]
[method: ImportingConstructor]
public sealed class ListContinuationHook(ISettingsService settings) : IEditorKeyHook
{
    public bool OnKeyDown(IEditorView editor, EditorKeyPress key)
    {
        var document = editor.Document;
        if (key != new EditorKeyPress(Key.Enter, KeyModifiers.None) || !MarkdownLanguage.IsMarkdown(document) || document.IsReadOnly
            || !editor.Selection.IsEmpty || !settings.Get<bool>(MarkdownSettings.ContinueLists))
        {
            return false;
        }

        var caret = editor.CaretOffset;
        var line = document.Buffer.Current.GetLineFromPosition(caret);
        if (ListContinuation.At(document.Buffer.Current.GetText(line.Span), caret - line.Start) is not { } edit)
            return false;

        // One undo step, as the editor's own typing is.
        var changes = document.Buffer.Replace(new TextSpan(line.Start + edit.Start, edit.Length), edit.Text);
        document.History.Push(changes, null, null);
        editor.CaretOffset = line.Start + edit.Start + edit.Text.Length;
        return true;
    }
}
