using System.Net;
using System.Net.Sockets;

namespace Open.LoggingService.Network
{
    public enum ListenerStatus
    {
        Listening,
        Stopped
    }

    public abstract class ListenerBase
    {
        protected readonly IPEndPoint EndPoint;
        protected Socket Listener;
        private readonly int _port;
        private ListenerStatus _status;

        protected ListenerBase(int port)
        {
            _port = port;
            EndPoint = new IPEndPoint(IPAddress.Any, port);
            _status = ListenerStatus.Stopped;
        }

        public ListenerStatus Status
        {
            get { return _status; }
        }

        public EndPoint Endpoint
        {
            get { return EndPoint; }
        }

        public int Port
        {
            get { return _port; }
        }

        public void Start()
        {
            try
            {
                Listener = CreateSocket();
                _status = ListenerStatus.Listening;

                Listen();
            }
            catch (SocketException)
            {
                if (Listener == null) return;
                Stop();
                throw;
            }
        }

        protected abstract Socket CreateSocket();
        protected abstract void Notify(SocketAsyncEventArgs saea);
        protected abstract bool ListenAsync(SocketAsyncEventArgs saea);

        private void Listen()
        {
            if (_status == ListenerStatus.Stopped) return;

            var saea = Sockets.ConnectSaeaPool.Take();
            saea.AcceptSocket = null;
            saea.Completed += IOCompleted;

            var async = ListenAsync(saea);

            if (!async)
            {
                IOCompleted(null, saea);
            }
        }

        private void IOCompleted(object sender, SocketAsyncEventArgs saea)
        {
            try
            {
                if (saea.SocketError == SocketError.Success)
                {
                    Notify(saea);
                }
            }
            finally
            {
                saea.Completed -= IOCompleted;
                Sockets.ConnectSaeaPool.Add(saea);
                if(Listener!=null) Listen();
            }
        }

        public void Stop()
        {
            _status = ListenerStatus.Stopped;
            if (Listener != null)
            {
                Listener.Close();
                Listener = null;
            }
        }
    }
}