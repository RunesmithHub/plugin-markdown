# Markdown for Runesmith

The official Markdown plugin for [Runesmith](https://github.com/RunesmithHub/Runesmith), `runesmith.markdown`. It adds:

- Markdown as a language, for `.md` and `.markdown` files, with its highlighting; the code in a fence is highlighted when a plugin for
  its language is installed;
- a **Markdown Preview** panel that docks beside the editor and shows the file as it renders, a moment after you stop typing;
- headings, emphasis, lists, links, images, inline code, block quotes, horizontal rules and fenced code blocks with their code colored;
- the GitHub Flavored Markdown extensions: tables, task lists, strikethrough and autolinks;
- images by paths relative to the Markdown file, and links that open the linked file in an editor or the web page in your browser;
- a preview that scrolls to the block the caret is in;
- the headings in the **Outline**, the breadcrumbs and **Go to Symbol in File**;
- lists that continue as you press <kbd>Enter</kbd>, with the next number or an empty checkbox;
- **Format Document**, which aligns the columns of tables.

The preview only reads your file. The plugin changes text when you ask it to: <kbd>Enter</kbd> in a list item, and **Format Document**.

## Install

Install **Markdown** from Runesmith's plugin manager (**Tools › Plugins**), or from its page on the plugin hub,
https://hub.runesmith.dev. It needs Runesmith with plugin API 0.2.0 or newer. The plugin loads when Runesmith starts; there is nothing
to turn on.

The preview is a web page, shown by the system's browser engine: WebView2 on Windows, WebKit on macOS, and WebKitGTK or WPE WebKit on
Linux. Where it is missing, the panel says what to install.

## Use

Open a `.md` or `.markdown` file and run **Markdown: Open Preview**:

| Where | How |
| --- | --- |
| Command palette | **Markdown: Open Preview** (`markdown.openPreview`) |
| Keyboard | <kbd>Ctrl+Shift+V</kbd> |
| Editor | Right-click in a Markdown file, **Open Preview** |
| Explorer | Right-click a Markdown file, **Open Preview**; this opens the file too |
| Panel | The **Markdown Preview** button on the right stripe, or **View › Markdown Preview** |

The panel follows the Markdown editor you work in. It stays on that file while you look at a file of another kind, and empties when
you close it. Drag the panel where you want it, as any panel.

| In the preview | What happens |
| --- | --- |
| A link to a heading, such as `#install` | The preview scrolls to it. |
| A link to a file, such as `docs/guide.md` | The file opens in an editor. A folder shows in your file manager. |
| A link to a web page | The page opens in your browser. |

## Settings

The settings are in the **Markdown** category of the settings page.

| Setting | Default | What it does |
| --- | --- | --- |
| `markdown.preview.syncScroll` | on | The preview scrolls to the heading, paragraph or other block the caret is in. |
| `markdown.preview.refreshDelay` | 200 | How long typing pauses, in milliseconds, before the preview shows the change; 50 to 5000. |
| `markdown.continueLists` | on | <kbd>Enter</kbd> in a list item starts the next item; <kbd>Enter</kbd> in an empty item ends the list. |

`editor.disabledKeyHooks`, a setting of Runesmith's own, turns off the plugin's handling of <kbd>Enter</kbd> too.

## Capabilities

| Capability | Why |
| --- | --- |
| `filesystem` | Reads the images a Markdown file shows when they are outside the open folder. |
| `process` | Opens the web links you click in the preview in your browser. |
| `native` | The Markdown parser it ships, Markdig, reads text with unsafe code. The plugin ships no native library. |

The plugin does not use the network.

## Limitations

- **Images from the web are not loaded.** The preview shows a note with the image's address instead. Loading them would need the
  `network` capability for every host, which would also let a page in the preview load scripts from the web.
- **Raw HTML is shown as text**, not rendered, so a Markdown file cannot put markup or scripts of its own in the preview.
- **Scrolling goes one way**: the preview follows the caret, by block. Scrolling the preview does not move the editor, and the preview
  does not follow the editor's scroll bar, because the plugin API tells plugins where the caret is but not what the editor shows.
- **Images** are PNG, JPEG, GIF, WebP, AVIF, BMP, ICO and SVG files of at most 5 MB. Another file, a larger one or a missing one shows
  as a note that says why.
- **A path that starts with `/`** is a path on your computer, not one from the top of the repository as on GitHub.
- **A link to a heading of another file**, such as `guide.md#install`, opens the file at its start.
- **Code** is colored for the languages of [highlight.js](https://highlightjs.org)'s common set, about 35 of them; code in another
  language shows without colors.
- **Lists continue inside fenced code blocks too**, since <kbd>Enter</kbd> looks only at the line the caret is on.
- **Table columns** are aligned by counting characters, so a table with wide characters, such as Chinese text or emoji, lines up in
  the preview but not exactly in the editor. A table with a row that has more cells than its header is left as it is.
- Math, diagrams, footnotes and other extensions outside GitHub Flavored Markdown's core are shown as the text they are written in.

## Build and test

You need the .NET SDK version in `global.json`. The Runesmith packages come from the plugin hub's feed, which `nuget.config` sets up.

```sh
dotnet build plugin-markdown.slnx -warnaserror
dotnet format plugin-markdown.slnx --verify-no-changes
dotnet test --solution plugin-markdown.slnx
```

`dotnet build src/Runesmith.Markdown -t:InstallPlugin` installs your build into Runesmith's plugins folder; restart Runesmith to load
it. Set `RUNESMITH_HOME` to install into a Runesmith home you test with.

| Folder | What is there |
| --- | --- |
| `src/Runesmith.Markdown/Rendering` | Markdown to HTML, with [Markdig](https://github.com/xoofx/markdig). |
| `src/Runesmith.Markdown/Paths` | Where links and image paths lead, images for the preview, and opening links. |
| `src/Runesmith.Markdown/Preview` | The panel, what it shows and when, and the messages to its page. |
| `src/Runesmith.Markdown/Editing` | The outline of headings, list continuation and table formatting. |
| `web` | The preview's page: `preview.html`, its style and script, and highlight.js in `web/highlight`. |
| `src/Runesmith.Markdown/grammars` | The TextMate grammar that highlights Markdown. |
| `tests/Runesmith.Markdown.Tests` | The tests, in the same folders. |

The tests run without Runesmith. What needs Runesmith, its editor and a web view is in the checklist of
[CONTRIBUTING.md](CONTRIBUTING.md#check-by-hand).

## Translations

The plugin's text is in English in `src/Runesmith.Markdown/locales/en/runesmith.markdown.ftl`, a [Fluent](https://projectfluent.org)
file. Text for a new message goes there, and the code reads it from the generated `Strings` class. A language pack translates the
file; see [Language packs](https://docs.runesmith.dev/plugins/language-packs).

## Documentation

Runesmith's documentation, including how plugins work and how to write one, is at https://docs.runesmith.dev.

## License

Apache-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE); [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) covers Markdig,
highlight.js and the Markdown grammar, which the plugin ships.
