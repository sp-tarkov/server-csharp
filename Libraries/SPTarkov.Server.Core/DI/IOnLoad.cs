using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.DI;

public interface IOnLoad
{
    /// <summary>
    /// Called when the server is loading through its various <see cref="Injectable.TypePriority">load order</see> stages.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that is cancelled when the server is shutting down gracefully, such as from Ctrl+C,
    /// a termination signal, or another controlled shutdown request.
    /// </param>
    /// <returns>
    /// A task that completes when the load operation has finished.
    /// </returns>
    /// <remarks>
    /// Implementations should observe <paramref name="cancellationToken"/> and stop work as soon as
    /// reasonably possible when cancellation is requested.
    ///
    /// Pass the token to any asynchronous APIs that accept one, such as file I/O, HTTP requests,
    /// database calls, or delays. For long-running synchronous work, periodically call
    /// <see cref="CancellationToken.ThrowIfCancellationRequested"/>.
    ///
    /// Do not treat cancellation as an error. If cancellation is requested, allow
    /// <see cref="OperationCanceledException"/> to propagate unless cleanup is required.
    /// </remarks>
    Task OnLoadAsync(CancellationToken cancellationToken);
}
