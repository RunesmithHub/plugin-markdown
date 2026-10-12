using Runesmith.Sdk.Documents;
using Runesmith.Sdk.Shell;

namespace Runesmith.Markdown.Paths;

/// <summary>Opens the links the user clicks in the preview.</summary>
internal interface ILinkOpener
{
    /// <summary>Opens a link by its address as written in the Markdown, relative to <paramref name="baseDirectory"/>, and tells the user
    /// when it cannot be opened.</summary>
    Task OpenAsync(string href, string? baseDirectory);
}

/// <summary>Opens web links in the browser, files in an editor and folders in the file manager.</summary>
internal sealed class LinkOpener(ILauncher launcher, IEditorService editors, INotificationService notifications) : ILinkOpener
{
    public async Task OpenAsync(string href, string? baseDirectory)
    {
        try
        {
            switch (LinkResolver.Resolve(href, baseDirectory))
            {
                case WebLink web:
                    launcher.OpenUrl(web.Address);
                    break;
                case FileLink file when File.Exists(file.Path):
                    await editors.OpenAsync(file.Path);
                    break;
                case FileLink file when Directory.Exists(file.Path):
                    launcher.Reveal(file.Path);
                    break;
                case FileLink file:
                    notifications.Notify(NotificationKind.Warning, Strings.LinkNotFound, file.Path);
                    break;
                default:
                    notifications.Notify(NotificationKind.Warning, Strings.LinkUnsupported, href);
                    break;
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            notifications.Notify(NotificationKind.Error, Strings.LinkFailed, exception.Message);
        }
    }
}
