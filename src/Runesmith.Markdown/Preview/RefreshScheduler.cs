using Avalonia.Threading;

namespace Runesmith.Markdown.Preview;

/// <summary>Runs the preview's refresh once typing has paused.</summary>
internal interface IRefreshScheduler
{
    /// <summary>Runs an action on the UI thread after a delay, unless the returned handle is disposed first.</summary>
    IDisposable Schedule(TimeSpan delay, Action action);
}

/// <summary>Schedules on Avalonia's UI thread.</summary>
internal sealed class DispatcherRefreshScheduler : IRefreshScheduler
{
    public IDisposable Schedule(TimeSpan delay, Action action) => DispatcherTimer.RunOnce(action, delay);
}
