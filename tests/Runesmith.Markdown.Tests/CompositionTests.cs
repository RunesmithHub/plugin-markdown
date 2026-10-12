using System.Composition;
using System.Composition.Hosting;
using Runesmith.Markdown.Editing;
using Runesmith.Markdown.Preview;
using Runesmith.Sdk.Commands;
using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Languages;
using Runesmith.Sdk.Settings;
using Runesmith.Sdk.ToolWindows;

namespace Runesmith.Markdown.Tests;

/// <summary>Loads the plugin's parts the way Runesmith does, with stand-ins for the services Runesmith provides.</summary>
public sealed class CompositionTests
{
    [Fact]
    public void EveryPartIsExportedAndGetsTheServicesItImports()
    {
        using var container = CreateContainer();
        var parts = new Parts();
        container.SatisfyImports(parts);

        Assert.IsType<MarkdownCommands>(Assert.Single(parts.Commands));
        Assert.IsType<MarkdownSettings>(Assert.Single(parts.Settings));
        Assert.IsType<PreviewToolWindow>(Assert.Single(parts.ToolWindows));
        Assert.IsType<ListContinuationHook>(Assert.Single(parts.KeyHooks));
    }

    [Fact]
    public void TheLanguageFeaturesServeMarkdownOnly()
    {
        using var container = CreateContainer();
        var parts = new Parts();
        container.SatisfyImports(parts);

        var symbols = Assert.Single(parts.Symbols);
        var formatting = Assert.Single(parts.Formatting);
        Assert.IsType<MarkdownSymbols>(symbols.Value);
        Assert.IsType<MarkdownFormatting>(formatting.Value);
        Assert.All(new[] { symbols.Metadata, formatting.Metadata }, metadata =>
        {
            Assert.True(metadata.Serves("markdown"));
            Assert.False(metadata.Serves("csharp"));
        });
    }

    [Fact]
    public void ThePanelIsOneInstanceWithTheIdTheCommandShows()
    {
        using var container = CreateContainer();

        var panel = container.GetExport<IToolWindowProvider>();

        Assert.Same(panel, container.GetExport<IToolWindowProvider>());
        Assert.Equal(new ToolWindowDefinition("markdown.preview", "Markdown Preview", "eye", DockSide.Right), panel.Definition);
    }

    [Fact]
    public void EverySettingHasATitleADescriptionAndTheMarkdownCategory()
    {
        var settings = new MarkdownSettings().Settings.ToList();

        Assert.Equal(["markdown.preview.syncScroll", "markdown.preview.refreshDelay", "markdown.continueLists"], settings.Select(setting => setting.Key));
        Assert.Equal([typeof(bool), typeof(int), typeof(bool)], settings.Select(setting => setting.ValueType));
        Assert.All(settings, setting =>
        {
            Assert.Equal("Markdown", setting.Category);
            Assert.False(string.IsNullOrEmpty(setting.Title));
            Assert.False(string.IsNullOrEmpty(setting.Description));
        });
    }

    private static CompositionHost CreateContainer() =>
        new ContainerConfiguration().WithAssembly(typeof(MarkdownCommands).Assembly).WithAssembly(typeof(CompositionTests).Assembly).CreateContainer();

    private sealed class Parts
    {
        [ImportMany]
        public IEnumerable<ICommandContributor> Commands { get; set; } = [];

        [ImportMany]
        public IEnumerable<ISettingContributor> Settings { get; set; } = [];

        [ImportMany]
        public IEnumerable<IToolWindowProvider> ToolWindows { get; set; } = [];

        [ImportMany]
        public IEnumerable<IEditorKeyHook> KeyHooks { get; set; } = [];

        [ImportMany]
        public IEnumerable<Lazy<IDocumentSymbolProvider, LanguageMetadata>> Symbols { get; set; } = [];

        [ImportMany]
        public IEnumerable<Lazy<IDocumentFormattingProvider, LanguageMetadata>> Formatting { get; set; } = [];
    }
}
