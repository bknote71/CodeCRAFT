using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public delegate void OnRecvHandler(ArraySegment<byte> buffer);
    public delegate void OnSendHandler(int numOfBytes);
    public delegate void OnDisconnectedHandler(EndPoint endPoint);

    public class Connection
    {
        Socket _socket;
        RecvBuffer _recvBuffer;
        int _disconnected;

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        object _lock = new object();

        event OnRecvHandler _OnRecv;
        event OnSendHandler _OnSend;
        event OnDisconnectedHandler _OnDisconnected;

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public Connection(Socket socket)
        {
            this._socket = socket;
            this._recvBuffer = new RecvBuffer(4096);
            this._disconnected = 0;
        }

        // Public

        public void Start(
            OnRecvHandler OnRecv,
            OnSendHandler OnSend,
            OnDisconnectedHandler OnDisconnected
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

            _OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        // Private

        void RegisterRecv()
        {
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                // update _recvBuffer
                if (_recvBuffer.OnWrite(args.BytesTransferred))
                {
                    // TODO: Exception Handling
                    Stop();
                    return;
                }

                int handled = -1;

                if ((handled = OnRecvBuffer(_recvBuffer.ReadSegment)) > _recvBuffer.DataSize)
                {
                    // TODO: Exception Handling
                    Stop();
                    return;    
                }

                if (_recvBuffer.OnRead(handled) == false)
                {
                    // TODO: Exception Handling
                    Stop();
                    return;
                }

                RegisterRecv();
            }
            else
            {
                Stop();
            }
        }

        // [size(2)][[packetId(2)][payload(size - 4)]...
        int OnRecvBuffer(ArraySegment<byte> buffer)
        {
            int handled = 0;
            
            // 패킷 조립이 안되면 처리 X(handled = 0)
            while (true)
            {
                if (buffer.Count < 2)
                    break;

                ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);

                if (buffer.Count < size)
                    break;

                _OnRecv(new ArraySegment<byte>(buffer.Array, buffer.Offset, size));

                handled += size;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + size, buffer.Count - size);
            }

            return handled;
        }

        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {   // _sendQueue to _pendingList
                _pendingList.Add(_sendQueue.Dequeue());
            }

            _sendArgs.BufferList = _pendingList;
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {

                    _sendArgs.BufferList = null;
                    _pendingList.Clear();

                    _OnSend(_sendArgs.BytesTransferred);

                    if (_sendQueue.Count > 0)
                        RegisterSend();
                }
                else
                {
                    Console.WriteLine("OnSendCompleted Failed");
                    Stop();
                }
            }
        }
    }
}
