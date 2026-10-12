using Runesmith.Markdown.Editing;

namespace Runesmith.Markdown.Tests.Editing;

public sealed class ListContinuationTests
{
    [Theory]
    [InlineData("- milk", "\n- ")]
    [InlineData("* milk", "\n* ")]
    [InlineData("+ milk", "\n+ ")]
    [InlineData("  - nested", "\n  - ")]
    [InlineData("\t- tabbed", "\n\t- ")]
    [InlineData("-   wide", "\n-   ")]
    [InlineData("1. first", "\n2. ")]
    [InlineData("9. ninth", "\n10. ")]
    [InlineData("3) third", "\n4) ")]
    [InlineData("- [ ] open", "\n- [ ] ")]
    [InlineData("- [x] done", "\n- [ ] ")]
    [InlineData("2. [X] done", "\n3. [ ] ")]
    public void EnterAtTheEndOfAnItemStartsTheNextOne(string line, string inserted) =>
        Assert.Equal(new LineEdit(line.Length, 0, inserted), ListContinuation.At(line, line.Length));

    [Fact]
    public void EnterInsideAnItemMovesTheRestToTheNextOne() =>
        Assert.Equal(new LineEdit(6, 0, "\n- "), ListContinuation.At("- milk and eggs", 6));

    [Theory]
    [InlineData("- ")]
    [InlineData("  1. ")]
    [InlineData("- [ ] ")]
    [InlineData("*   ")]
    public void EnterInAnEmptyItemEndsTheList(string line) =>
        Assert.Equal(new LineEdit(0, line.Length, ""), ListContinuation.At(line, line.Length));

    [Theory]
    [InlineData("Just text", 9)]
    [InlineData("", 0)]
    [InlineData("-not a list", 11)]
    [InlineData("**bold** text", 13)]
    [InlineData("1.5 liters", 10)]
    [InlineData("- - -", 5)]
    [InlineData("* * *", 5)]
    [InlineData("# Heading", 9)]
    public void EnterElsewhereWorksAsUsual(string line, int column) => Assert.Null(ListContinuation.At(line, column));

    [Theory]
    [InlineData("- milk", 0)]
    [InlineData("- milk", 1)]
    [InlineData("  10. milk", 4)]
    [InlineData("- [ ] milk", 4)]
    public void EnterBeforeTheTextOfAnItemWorksAsUsual(string line, int column) => Assert.Null(ListContinuation.At(line, column));
}
