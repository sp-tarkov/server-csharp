using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace SPTarkov.Server.Core.Servers.Ws;

/// <summary>
/// Handles the lifecycle of WebSocket connections whose request path contains <see cref="GetHookUrl"/>.
/// </summary>
public interface IWebSocketConnectionHandler
{
    /// <summary>
    /// The URL fragment this handler responds to; a connection is routed here when its request path contains this value.
    /// </summary>
    /// <returns>The hook URL fragment (e.g. "/notifierServer/getwebsocket/").</returns>
    string GetHookUrl();

    /// <summary>
    /// A human-readable identifier for this handler, used for logging.
    /// </summary>
    /// <returns>The handler's display name.</returns>
    string GetSocketId();

    /// <summary>
    /// Called once when a new WebSocket connection is opened.
    /// </summary>
    /// <param name="ws">The newly opened WebSocket.</param>
    /// <param name="context">The HTTP context the connection was upgraded from.</param>
    /// <param name="sessionIdContext">A unique reference identifying this specific connection.</param>
    Task OnConnectionAsync(WebSocket ws, HttpContext context, string sessionIdContext);

    /// <summary>
    /// Called for each complete message received from the client.
    /// </summary>
    /// <param name="rawData">The full message payload.</param>
    /// <param name="messageType">The type of the received message (text or binary).</param>
    /// <param name="ws">The WebSocket the message arrived on.</param>
    /// <param name="context">The HTTP context the connection was upgraded from.</param>
    Task OnMessageAsync(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context);

    /// <summary>
    /// Called once when the WebSocket connection closes; the socket should already be assumed closed here.
    /// </summary>
    /// <param name="ws">The closed WebSocket.</param>
    /// <param name="context">The HTTP context the connection was upgraded from.</param>
    /// <param name="sessionIdContext">The unique reference identifying the connection, matching the one passed to <see cref="OnConnectionAsync"/>.</param>
    Task OnCloseAsync(WebSocket ws, HttpContext context, string sessionIdContext);
}
