namespace Runesmith.Markdown.Paths;

/// <summary>An image for the preview: the address the page loads it from, or why it cannot be shown.</summary>
/// <param name="Source">A <c>data:</c> address holding the image, or null when it cannot be shown.</param>
/// <param name="Problem">What to show in the image's place, or null when it can be shown.</param>
internal sealed record ImageResult(string? Source, string? Problem);

/// <summary>Supplies the images of a Markdown file to the preview.</summary>
internal interface IImageSource
{
    /// <summary>Loads an image by its address as written in the Markdown, relative to <paramref name="baseDirectory"/>.</summary>
    ImageResult Load(string address, string? baseDirectory);
}

/// <summary>Reads image files into <c>data:</c> addresses, because the preview's page cannot open files on the computer or images on
/// the web.</summary>
internal sealed class ImageInliner : IImageSource
{
    /// <summary>The largest image the preview shows; each refresh sends the images to the page again.</summary>
    public const int MaxMegabytes = 5;

    private static readonly Dictionary<string, string> MediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".avif"] = "image/avif",
        [".bmp"] = "image/bmp",
        [".ico"] = "image/x-icon",
        [".svg"] = "image/svg+xml",
    };

    public ImageResult Load(string address, string? baseDirectory) => LinkResolver.Resolve(address, baseDirectory) switch
    {
        WebLink => Problem(Strings.ImageRemote(address)),
        FileLink file => Read(file.Path, address),
        _ => Problem(Strings.ImageNotFound(address)),
    };

    private static ImageResult Read(string path, string address)
    {
        if (!MediaTypes.TryGetValue(Path.GetExtension(path), out var mediaType))
            return Problem(Strings.ImageUnsupported(address));

        try
        {
            var file = new FileInfo(path);
            if (!file.Exists)
                return Problem(Strings.ImageNotFound(address));
            if (file.Length > MaxMegabytes * 1024 * 1024)
                return Problem(Strings.ImageTooLarge(MaxMegabytes, address));

            return new ImageResult($"data:{mediaType};base64,{Convert.ToBase64String(File.ReadAllBytes(path))}", null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return Problem(Strings.ImageUnreadable(address));
        }
    }

    private static ImageResult Problem(string message) => new(null, message);
}
