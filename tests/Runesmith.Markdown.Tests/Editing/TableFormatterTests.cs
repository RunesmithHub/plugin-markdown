using Runesmith.Markdown.Editing;
using Runesmith.Text;

namespace Runesmith.Markdown.Tests.Editing;

public sealed class TableFormatterTests
{
    [Fact]
    public void MakesEveryCellOfAColumnAsWideAsItsWidest() => Assert.Equal(
        """
        | Name      | Size |
        | --------- | ---- |
        | readme.md | 12   |
        | a         | 1024 |
        """,
        Format(
        """
        |Name|Size|
        |-|-|
        |readme.md|12|
        |a|1024|
        """));

    [Fact]
    public void KeepsAndFollowsTheAlignmentOfEachColumn() => Assert.Equal(
        """
        | Left  | Center |  Right | Plain |
        | :---- | :----: | -----: | ----- |
        | a     |   b    |      c | d     |
        | apple | banana | cherry | date  |
        """,
        Format(
        """
        | Left | Center | Right | Plain |
        |:--|:-:|--:|---|
        | a | b | c | d |
        | apple | banana | cherry | date |
        """));

    [Fact]
    public void AddsThePipesAtTheEndsAndTheCellsARowLeavesOut() => Assert.Equal(
        """
        | a   | b   | c   |
        | --- | --- | --- |
        | 1   |     |     |
        | 1   | 2   | 3   |
        """,
        Format(
        """
        a | b | c
        --- | --- | ---
        1 |
        1 | 2 | 3
        """));

    [Fact]
    public void KeepsAnEscapedPipeInsideItsCell() => Assert.Equal(
        """
        | Operator | Meaning |
        | -------- | ------- |
        | a \| b   | or      |
        """,
        Format(
        """
        | Operator | Meaning |
        |---|---|
        | a \| b | or |
        """));

    [Fact]
    public void KeepsTheIndentationOfATableInAList() => Assert.Equal(
        "- Sizes:\n\n  | a   | b   |\n  | --- | --- |\n  | 1   | 2   |\n",
        Format("- Sizes:\n\n  | a | b |\n  |---|---|\n  | 1 | 2 |\n"));

    [Fact]
    public void FormatsEveryTableAndNothingBetweenThem()
    {
        const string Text = "# Sizes\n\n|a|b|\n|-|-|\n|1|2|\n\nText with a | pipe.\n\n|c|\n|-|\n|3|\n";

        Assert.Equal("# Sizes\n\n| a   | b   |\n| --- | --- |\n| 1   | 2   |\n\nText with a | pipe.\n\n| c   |\n| --- |\n| 3   |\n", Format(Text));
    }

    [Theory]
    [InlineData("| a   | b   |\n| --- | --- |\n| 1   | 2   |\n")]
    [InlineData("Just a paragraph with a | pipe.\n\nAnother | one.\n")]
    [InlineData("Title | not a table\n---\n")]
    [InlineData("| a | b |\n| --- |\n| 1 | 2 |\n")]
    [InlineData("```\n|a|b|\n|-|-|\n|1|2|\n```\n")]
    [InlineData("~~~text\n|a|b|\n|-|-|\n~~~\n")]
    [InlineData("")]
    public void ChangesNothingWhereThereIsNothingToFormat(string text) => Assert.Empty(TableFormatter.Format(text));

    [Fact]
    public void LeavesATableWhoseRowHasMoreCellsThanItsHeader() =>
        Assert.Empty(TableFormatter.Format("|a|b|\n|-|-|\n|1|2|3|\n"));

    [Fact]
    public void ChangesOnlyTheLinesThatDiffer()
    {
        var changes = TableFormatter.Format("| a   | b   |\n| --- | --- |\n|1|2|\n");

        Assert.Equal([new TextChange(new TextSpan(28, 5), "| 1   | 2   |")], changes);
    }

    private static string Format(string text) => TextSnapshot.Create(text).Apply(TableFormatter.Format(text)).GetText();
}
