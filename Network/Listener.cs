using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    public class Listener
    {
        Socket _listenSocket;

        // event
        public event EventHandler<ClientAcceptedEventArgs> OnConnected;

        public void Start(IPEndPoint endPoint, int register = 1, int backlog = 1)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(backlog);

            for (int i = 0; i < register; ++i)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptedCompleted);
                RegisterAccept(args);
            }
        }

        // Register
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            if (_listenSocket == null)
                return;

            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
            {
                OnAcceptedCompleted(null, args);
            }
        }

        // Completed
        void OnAcceptedCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                // connection 생성
                var connection = new Connection(args.AcceptSocket);
                OnConnected.Invoke(null, new ClientAcceptedEventArgs(connection));
            }
            else
                Console.WriteLine(args.SocketError);

            // IOCP = 작업 단위.
            // 작업이 끝났으면 다시 감시할 작업을 등록해야 함.
            RegisterAccept(args);
        }
    }    
}
