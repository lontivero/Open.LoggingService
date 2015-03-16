using System.Net.Sockets;
using Open.LoggingService.Utils;

namespace Open.LoggingService.Network
{
    public static class Sockets
    {
        internal static readonly BlockingPool<SocketAsyncEventArgs> ConnectSaeaPool =
            new BlockingPool<SocketAsyncEventArgs>(() => new SocketAsyncEventArgs());
    }
}