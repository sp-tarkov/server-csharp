using System.Runtime.InteropServices;

namespace SPTarkov.Server.Helpers;

public sealed class StartupCancellation : IDisposable
{
    private readonly CancellationTokenSource _source = new();
    private readonly List<IDisposable> _registrations = [];
    private int _signalCount;

    public StartupCancellation()
    {
        Console.CancelKeyPress += OnCancelKeyPress;
        _registrations.Add(PosixSignalRegistration.Create(PosixSignal.SIGTERM, OnPosixSignal));
    }

    public CancellationToken Token
    {
        get { return _source.Token; }
    }

    public bool IsCancellationRequested
    {
        get { return _source.IsCancellationRequested; }
    }

    public void LinkTo(IHostApplicationLifetime lifetime)
    {
        _registrations.Add(lifetime.ApplicationStopping.Register(Cancel));
    }

    public void Dispose()
    {
        Console.CancelKeyPress -= OnCancelKeyPress;

        foreach (var registration in _registrations)
        {
            registration.Dispose();
        }

        _registrations.Clear();
        _source.Dispose();
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs args)
    {
        args.Cancel = HandleSignal();
    }

    private void OnPosixSignal(PosixSignalContext context)
    {
        context.Cancel = HandleSignal();
    }

    private bool HandleSignal()
    {
        if (Interlocked.Increment(ref _signalCount) != 1)
        {
            return false;
        }

        Cancel();
        return true;
    }

    private void Cancel()
    {
        if (!_source.IsCancellationRequested)
        {
            _source.Cancel();
        }
    }
}
