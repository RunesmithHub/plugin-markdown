using Runesmith.Markdown.Paths;

namespace Runesmith.Markdown.Tests.Paths;

public sealed class ImageInlinerTests : IDisposable
{
    private readonly TemporaryFolder folder = new();
    private readonly ImageInliner images = new();

    public void Dispose() => folder.Dispose();

    [Theory]
    [InlineData("logo.png", "image/png")]
    [InlineData("photo.JPG", "image/jpeg")]
    [InlineData("drawing.svg", "image/svg+xml")]
    public void ReadsAnImageIntoADataAddress(string name, string mediaType)
    {
        folder.Write(name, [1, 2, 3, 250]);

        Assert.Equal(new ImageResult($"data:{mediaType};base64,AQID+g==", null), images.Load(name, folder.Path));
    }

    [Fact]
    public void ReadsAnImageRelativeToTheMarkdownFile()
    {
        folder.Write("img/logo.png", [7]);

        Assert.Equal("data:image/png;base64,Bw==", images.Load("../img/logo.png", Path.Combine(folder.Path, "docs")).Source);
    }

    [Fact]
    public void SaysWhenTheFileDoesNotExist() =>
        Assert.Equal(new ImageResult(null, "Image not found: gone.png"), images.Load("gone.png", folder.Path));

    [Fact]
    public void SaysWhenTheDocumentHasNoFileToStartFrom() =>
        Assert.Equal(new ImageResult(null, "Image not found: logo.png"), images.Load("logo.png", null));

    [Fact]
    public void DoesNotLoadImagesFromTheWeb() => Assert.Equal(
        new ImageResult(null, "Images from the web are not loaded: https://example.com/badge.svg"), images.Load("https://example.com/badge.svg", folder.Path));

    [Fact]
    public void DoesNotReadAFileThatIsNotAnImage()
    {
        folder.Write("secrets.txt", [1]);

        Assert.Equal(new ImageResult(null, "This kind of image cannot be shown: secrets.txt"), images.Load("secrets.txt", folder.Path));
    }

    [Fact]
    public void DoesNotReadAnImageThatIsTooLarge()
    {
        folder.Write("huge.png", new byte[(ImageInliner.MaxMegabytes * 1024 * 1024) + 1]);

        Assert.Equal(new ImageResult(null, "Image larger than 5 MB: huge.png"), images.Load("huge.png", folder.Path));
    }

    [Fact]
    public void SaysWhenTheImageIsAFolder()
    {
        Directory.CreateDirectory(Path.Combine(folder.Path, "folder.png"));

        Assert.Equal(new ImageResult(null, "Image not found: folder.png"), images.Load("folder.png", folder.Path));
    }
}
