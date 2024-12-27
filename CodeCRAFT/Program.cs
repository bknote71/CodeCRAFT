using System.Net.Sockets;
using Network;

namespace Server
{
    class Program
    {
        public void Start()
        {
            Listener listener = new Listener();
            listener.OnConnected += this.OnConnected;
        }

        void OnConnected(object? sender, ClientAcceptedEventArgs args)
        {
            args.connection.Start(OnRecv, OnSend, OnDisconnected);
        }

        void OnRecv(ArraySegment<byte> buffer)
        {
        }

        void OnSend(object? sender, SocketAsyncEventArgs args)
        {
        }

        void OnDisconnected(object? sender, SocketAsyncEventArgs args)
        {
        }
    }

    
}