namespace SPTarkov.Server.Core.DI;

public interface IOnUpdate
{
    /// <summary>
    /// Called repeatedly while the server is running.
    /// </summary>
    /// <param name="secondsSinceLastRun">
    /// The number of seconds since this component last completed successfully.
    /// A successful update is one that returns <see langword="true"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that is cancelled when the server is shutting down gracefully, such as from Ctrl+C,
    /// a termination signal, or another controlled shutdown request.
    /// </param>
    /// <returns>
    /// A task that completes with <see langword="true"/> when the update ran successfully and the
    /// last-run timestamp should be updated; otherwise, <see langword="false"/> to leave the
    /// last-run timestamp unchanged.
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
    ///
    /// Returning <see langword="true"/> indicates that the update completed successfully. Returning
    /// <see langword="false"/> can be used when the update intentionally skipped work and should be
    /// called again later with the same accumulated elapsed time.
    /// </remarks>
    Task<bool> OnUpdateAsync(long secondsSinceLastRun, CancellationToken cancellationToken);
}
