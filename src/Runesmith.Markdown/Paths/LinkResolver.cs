using System.Text.RegularExpressions;

namespace Runesmith.Markdown.Paths;

/// <summary>What a link or an image address in a Markdown file points at.</summary>
internal abstract record LinkTarget;

/// <summary>A page or a file on the web.</summary>
internal sealed record WebLink(Uri Address) : LinkTarget;

/// <summary>A file or a folder on this computer, by its full path; it may not exist.</summary>
internal sealed record FileLink(string Path) : LinkTarget;

/// <summary>An address that names neither, such as <c>mailto:</c>, or a relative path in a document that was never saved.</summary>
internal sealed record UnsupportedLink(string Href) : LinkTarget;

/// <summary>Resolves the addresses a Markdown file writes, relative to the folder the file is in.</summary>
internal static partial class LinkResolver
{
    /// <summary>Resolves an address as written in the Markdown, such as <c>../img/logo.png</c>, <c>guide.md#install</c> or
    /// <c>https://example.com</c>.</summary>
    /// <param name="baseDirectory">The folder of the Markdown file, or null for a document that has no file yet.</param>
    public static LinkTarget Resolve(string href, string? baseDirectory)
    {
        href = href.Trim();
        if (Scheme().Match(href) is { Success: true } scheme)
        {
            if (!Uri.TryCreate(href, UriKind.Absolute, out var address))
                return new UnsupportedLink(href);

            return scheme.Groups[1].Value.ToUpperInvariant() switch
            {
                "HTTP" or "HTTPS" => new WebLink(address),
                "FILE" => new FileLink(Path.GetFullPath(address.LocalPath)),
                _ => new UnsupportedLink(href),
            };
        }

        var path = Uri.UnescapeDataString(WithoutQueryAndFragment(href));
        if (path.Length == 0 || path.Contains('\0') || (baseDirectory is null && !Path.IsPathRooted(path)))
            return new UnsupportedLink(href);

        return new FileLink(Path.GetFullPath(path, baseDirectory ?? Path.GetPathRoot(path)!));
    }

    private static string WithoutQueryAndFragment(string href)
    {
        var end = href.IndexOfAny(['?', '#']);
        return end < 0 ? href : href[..end];
    }

    // Two letters at least, so the drive of a Windows path such as C:\notes is not taken for a scheme.
    [GeneratedRegex(@"^([A-Za-z][A-Za-z0-9+.\-]+):")]
    private static partial Regex Scheme();
}
