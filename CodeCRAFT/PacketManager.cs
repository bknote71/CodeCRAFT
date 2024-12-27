using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Server
{
    internal class PacketManager
    {
        public readonly static PacketManager Instance = new PacketManager();

        Dictionary<ushort, Action<Client, ArraySegment<byte>, ushort>> _packetFactory = new Dictionary<ushort, Action<Client, ArraySegment<byte>, ushort>>();
        Dictionary<ushort, Action<Client, IMessage>> _packetHandlers = new Dictionary<ushort, Action<Client, IMessage>>();
        
        public void Register()
        {
            _packetFactory.Add(0, MakePacket<CEnterBattle>);
            _packetHandlers.Add(0, PacketHandler.CEnterBattleHandler);
        }

        public void OnRecvPacket(Client client, ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            Action<Client, ArraySegment<byte>, ushort> action;
            if (_packetFactory.TryGetValue(id, out action))
                action.Invoke(client, buffer, id);
        }

        void MakePacket<T>(Client client, ArraySegment<byte> buffer, ushort packetId) where T : IMessage, new()
        {
            T packet = new T();
            packet.MergeFrom(buffer);

            Action<Client, IMessage> action;
            if (_packetHandlers.TryGetValue(packetId, out action))
                action.Invoke(client, packet);
        }
    }
}
