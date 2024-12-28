using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Network;
using Server.Engine;

namespace Server
{
    public class Client
    {
        Connection _connection;

        // Robot (find from db)
        RobotSpec[] _specs;
        int _specIndex;

        public string Username { get; }
        public int CurrentSpecIndex { get; }

        public Client(Connection connection)
        {
            _connection = connection;
            connection.Start(OnRecv, OnSend, OnDisconnected);
        }

        // Public
        public void send(IMessage msg)
        {
            ushort size = (ushort)msg.CalculateSize();
            ushort id = getIdFromMsg(msg);

            var buffer = new byte[size + 4];

            BitConverter.TryWriteBytes(buffer.AsSpan(0, 2), size);
            BitConverter.TryWriteBytes(buffer.AsSpan(2, 4), id);
            msg.WriteTo(buffer.AsSpan(4));

            _connection.Send(buffer);
        }

        ushort getIdFromMsg(IMessage msg)
        {
            var msgName = msg.GetType().Name.ToUpper();
            return (ushort)Enum.Parse(typeof(MsgId), msgName);
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
