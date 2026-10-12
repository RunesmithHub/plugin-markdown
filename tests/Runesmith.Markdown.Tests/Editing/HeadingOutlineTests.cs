using Runesmith.Markdown.Editing;
using Runesmith.Sdk.Languages;
using Runesmith.Text;

namespace Runesmith.Markdown.Tests.Editing;

public sealed class HeadingOutlineTests
{
    [Fact]
    public void NestsHeadingsUnderTheHeadingAboveThem()
    {
        var outline = HeadingOutline.Build("# Guide\n\n## Install\n\n### Linux\n\n## Use\n\n# Reference\n");

        Assert.Equal(["Guide", "Reference"], outline.Select(symbol => symbol.Name));
        Assert.Equal(["Install", "Use"], outline[0].Children.Select(symbol => symbol.Name));
        Assert.Equal(["Linux"], outline[0].Children[0].Children.Select(symbol => symbol.Name));
        Assert.Empty(outline[1].Children);
    }

    [Fact]
    public void AHeadingCoversItsSectionAndSelectsItsLine()
    {
        const string Text = "# Guide\n\nIntro.\n\n## Install\n\nSteps.\n\n# Reference\n";

        var outline = HeadingOutline.Build(Text);

        var guide = outline[0];
        Assert.Equal(TextSpan.FromBounds(0, Text.IndexOf("# Reference", StringComparison.Ordinal)), guide.Range);
        Assert.Equal("# Guide", Text.Substring(guide.SelectionRange.Start, guide.SelectionRange.Length));
        var install = guide.Children[0];
        Assert.Equal("## Install\n\nSteps.\n\n", Text.Substring(install.Range.Start, install.Range.Length));
        Assert.Equal(TextSpan.FromBounds(Text.IndexOf("# Reference", StringComparison.Ordinal), Text.Length), outline[1].Range);
    }

    [Fact]
    public void AHeadingThatSkipsALevelStillNests()
    {
        var outline = HeadingOutline.Build("### Deep\n\n# Top\n\n#### Deeper\n");

        Assert.Equal(["Deep", "Top"], outline.Select(symbol => symbol.Name));
        Assert.Equal(["Deeper"], outline[1].Children.Select(symbol => symbol.Name));
    }

    [Fact]
    public void ListsAHeadingByItsTextWithoutMarkup()
    {
        var heading = Assert.Single(HeadingOutline.Build("## The `Run` *menu* and [links](https://runesmith.dev) ##"));

        Assert.Equal("The Run menu and links", heading.Name);
        Assert.Equal("##", heading.Detail);
        Assert.Equal(SymbolKind.String, heading.Kind);
    }

    [Fact]
    public void ReadsUnderlinedHeadings() =>
        Assert.Equal([("Title", "#"), ("Section", "##")], Flatten(HeadingOutline.Build("Title\n=====\n\nSection\n-------\n")));

    [Fact]
    public void LeavesOutWhatOnlyLooksLikeAHeading() =>
        Assert.Equal([("Real", "#")], Flatten(HeadingOutline.Build("```sh\n# a comment\n```\n\n    # indented code\n\n#hashtag\n\n#\n\n# Real\n")));

    [Fact]
    public void ATextWithoutHeadingsHasNoOutline() => Assert.Empty(HeadingOutline.Build("Just a paragraph.\n"));

    private static List<(string Name, string? Detail)> Flatten(IReadOnlyList<DocumentSymbol> symbols) =>
        [.. symbols.SelectMany(symbol => Flatten(symbol.Children).Prepend((symbol.Name, symbol.Detail)))];
}
