using System.Composition;
using Runesmith.Sdk.Settings;

namespace Runesmith.Markdown;

/// <summary>The plugin's settings, in the Markdown category of the settings page.</summary>
[Export(typeof(ISettingContributor))]
public sealed class MarkdownSettings : ISettingContributor
{
    /// <summary>Whether the preview scrolls to the block the caret is in.</summary>
    public const string SyncScroll = "markdown.preview.syncScroll";

    /// <summary>How long typing pauses, in milliseconds, before the preview shows the change.</summary>
    public const string RefreshDelay = "markdown.preview.refreshDelay";

    /// <summary>Whether Enter in a list item starts the next item.</summary>
    public const string ContinueLists = "markdown.continueLists";

    public IEnumerable<SettingDefinition> Settings =>
    [
        new(SyncScroll, Strings.SettingSyncScroll, Strings.MarkdownCategory, true) { Description = Strings.SettingSyncScrollDescription },
        new(RefreshDelay, Strings.SettingRefreshDelay, Strings.MarkdownCategory, 200)
        {
            Description = Strings.SettingRefreshDelayDescription,
            Minimum = 50,
            Maximum = 5000,
        },
        new(ContinueLists, Strings.SettingContinueLists, Strings.MarkdownCategory, true) { Description = Strings.SettingContinueListsDescription },
    ];
}
