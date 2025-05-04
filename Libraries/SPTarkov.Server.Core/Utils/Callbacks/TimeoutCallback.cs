namespace SPTarkov.Server.Core.Utils.Callbacks;

public static class TimeoutCallback
{
    public static Task RunInTimespan(Action action, TimeSpan timeSpan)
    {
        return Task.Factory.StartNew(() =>
        {
            Thread.Sleep(timeSpan);
            action();
        });
    }
}
