using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public delegate void OnRecvHandler(ArraySegment<byte> buffer);
    public class Connection
    {
        Socket _socket;
        int _disconnected;

        event OnRecvHandler _OnRecv;
        event EventHandler<SocketAsyncEventArgs> _OnSend;
        event EventHandler<SocketAsyncEventArgs> _OnDisconnected;

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public Connection(Socket socket)
        {
            this._socket = socket;
        }

        // Public

        public void Start(
            OnRecvHandler OnRecv,
            EventHandler<SocketAsyncEventArgs> OnSend,
            EventHandler<SocketAsyncEventArgs> OnDisconnected
            )
        {
            _OnRecv = OnRecv;
            _OnSend = OnSend;
            _OnDisconnected = OnDisconnected;

            _recvArgs.Completed += OnRecvCompleted; 
            _sendArgs.Completed += OnSendCompleted; 
            RegisterRecv();
        }

        public void Stop()
        {
            // Exchange == get and set
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        // Private

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                _OnRecv.Invoke(new ArraySegment<byte>(args.Buffer, 0, args.BytesTransferred));
                RegisterRecv();
            }
            else
            {
                Stop();
            }
        }

        void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
        {
        }
    }
}
