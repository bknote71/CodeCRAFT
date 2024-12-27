using System.Net;
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
            var clinet = new Client(args.connection);
        }
    }
}
