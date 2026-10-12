# Contributing

Thanks for helping. For a bug, open an issue with the steps to reproduce it; for a security problem, follow [SECURITY.md](SECURITY.md)
instead. For a larger change, open an issue first, so the approach is agreed on before you write the code.

## Build and test

You need the .NET SDK version in `global.json`.

```sh
dotnet build plugin-markdown.slnx -warnaserror
dotnet format plugin-markdown.slnx --verify-no-changes
dotnet test --solution plugin-markdown.slnx
```

`dotnet format plugin-markdown.slnx` fixes the formatting. When you change a package reference, commit the updated `packages.lock.json`
files.

## How the code is divided

Each part does one thing and knows the others by an interface, so it can be tested alone:

- `Rendering` turns Markdown into HTML and knows nothing of Runesmith. It gets images through `IImageSource`.
- `Paths` decides where a link or an image path leads (`LinkResolver`), reads images (`ImageInliner`) and opens links (`LinkOpener`).
- `Preview` decides what the panel shows and when (`PreviewController`), and talks to the page (`PreviewPage`). The controller knows
  the renderer, the page, the link opener and the timer only by their interfaces.
- `Editing` holds what changes or reads the text in the editor. The rules are plain functions over text (`HeadingOutline`,
  `ListContinuation`, `TableFormatter`); the classes Runesmith loads (`MarkdownSymbols`, `ListContinuationHook`,
  `MarkdownFormatting`) only connect them to the editor.

The page in `web` has no text of its own and makes no decisions: the plugin sends it HTML and it shows it. Text users see goes in
`locales/en`. `web/highlight` holds highlight.js as its release ships it; to update it, replace the three files from the
`@highlightjs/cdn-assets` package of the new version and update [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Check by hand

The tests cover the rules. Runesmith's editor and the web view are checked by hand before a release: install your build with
`dotnet build src/Runesmith.Markdown -t:InstallPlugin`, restart Runesmith and open a folder with a Markdown file that has an image
beside it.

Commands and the panel:

- [ ] **Tools › Plugins** lists **Markdown** as loaded, and the **Plugins** output has no error from it.
- [ ] The command palette lists **Markdown: Open Preview** with <kbd>Ctrl+Shift+V</kbd>; with a C# file active it cannot run.
- [ ] <kbd>Ctrl+Shift+V</kbd> in a Markdown file shows the **Markdown Preview** panel on the right, beside the editor.
- [ ] **Open Preview** is in the editor's context menu of a Markdown file and not of another file.
- [ ] **Open Preview** is in the Explorer's context menu of a `.md` and a `.markdown` file, opens the file and shows the panel; it is
      not there on a folder or another file.
- [ ] **View › Markdown Preview** and the stripe button show and hide the panel.
- [ ] With no Markdown file open, the panel says "Open a Markdown file to preview it."

Rendering:

- [ ] Headings of all six levels, bold, italic, inline code, a block quote, a horizontal rule, and bulleted and numbered lists with a
      nested list show as they should.
- [ ] A table shows with its borders and its left, centered and right columns.
- [ ] `- [ ]` and `- [x]` show as an empty and a checked checkbox that cannot be clicked.
- [ ] `~~text~~` shows struck through, and a bare `https://` address is a link.
- [ ] A fenced block marked `csharp`, `json` or `bash` shows colored; one marked with an unknown language, and one without a
      language, show plain.
- [ ] `<script>alert(1)</script>` and `<b>bold</b>` show as that text, and nothing runs.
- [ ] Switch between a dark and a light theme: the preview and its code follow at once.

Live updates:

- [ ] Typing shows in the preview shortly after you stop, and the preview keeps its scroll position.
- [ ] Set `markdown.preview.refreshDelay` to 2000: the preview now waits two seconds.
- [ ] Switch to another Markdown tab: the preview shows that file. Switch to a C# file: the preview stays. Close the Markdown file:
      the preview empties.
- [ ] A new untitled document set to Markdown shows in the preview.
- [ ] After all of the above the Markdown file's tab shows no unsaved changes that you did not type.

Scrolling:

- [ ] In a long file, move the caret to a heading far down: the preview scrolls to it. Move the caret inside what the preview already
      shows: the preview stays still.
- [ ] Turn `markdown.preview.syncScroll` off: the preview no longer scrolls with the caret.

Links:

- [ ] A link to `#a-heading` scrolls the preview to that heading.
- [ ] A link to another Markdown file, to a file in a parent folder and to a file with a space in its name (`my%20notes.md`) each open
      the file in an editor.
- [ ] A link to a folder shows it in the file manager.
- [ ] A link to a missing file shows the notification "The linked file does not exist" with its path.
- [ ] An `https://` link opens in the browser, and the preview stays on the document.
- [ ] A `mailto:` link shows "This kind of link cannot be opened".

Images:

- [ ] `![](logo.png)`, `![](img/logo.png)`, `![](../logo.png)` and an SVG each show.
- [ ] An image outside the open folder shows.
- [ ] A missing image, an image on the web and a `.txt` file as an image each show a note that says why, and the rest of the document
      renders.

Editing:

- [ ] The **Outline** and the breadcrumbs list the headings as a tree; clicking one goes to it. **Go to Symbol in File** finds them.
- [ ] <kbd>Enter</kbd> at the end of `- item`, `3. item` and `- [x] item` starts `- `, `4. ` and `- [ ] `. <kbd>Enter</kbd> on the empty
      item removes it. One <kbd>Ctrl+Z</kbd> undoes each.
- [ ] <kbd>Enter</kbd> in a paragraph, and with the completion list open, works as usual.
- [ ] Turn `markdown.continueLists` off: <kbd>Enter</kbd> in a list item only starts a new line.
- [ ] **Format Document** aligns the columns of an untidy table, changes nothing else, and is one undo step.

## Check the release

The plugin is released through the Runesmith Plugin Hub, which builds, packages and analyses it. Run the same checks before you open a
pull request:

```sh
dotnet tool install --global RunesmithHub.Tool --version 0.1.8
rshub check .
```

## Commits

Write each commit message as a [Conventional Commit](https://www.conventionalcommits.org/en/v1.0.0/), such as
`fix(preview): keep the scroll position when the document renders again`. The types are `feat`, `fix`, `perf`, `refactor`, `test`,
`docs`, `ci`, `chore` and `revert`.

## License

Contributions you submit are licensed under the [Apache License 2.0](LICENSE), as section 5 of the license describes.
