using System.Net.Sockets;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Exceptions;

public sealed class WebServerPortUnavailableException(HttpConfig httpConfig, SocketException innerException) : Exception
{
    public string Ip { get; } = httpConfig.Ip;
    public int Port { get; } = httpConfig.Port;
    public SocketError SocketErrorCode { get; } = innerException.SocketErrorCode;
    public int NativeErrorCode { get; } = innerException.NativeErrorCode;
}
