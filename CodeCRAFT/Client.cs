using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Network;

namespace Server
{
    internal class Client
    {
        Connection _connection;

        public Client(Connection connection)
        {
            _connection = connection;
            connection.Start(OnRecv, OnSend, OnDisconnected);
        }

        void OnRecv(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        void OnSend(int numOfBytess)
        {
        }

        void OnDisconnected(EndPoint endPoint)
        {
        }
    }
}
