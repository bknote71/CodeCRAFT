using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class ClientAcceptedEventArgs : EventArgs
    {
        public readonly Connection connection;

        public ClientAcceptedEventArgs(Connection connection)
        {
            this.connection = connection;
            //this.connection.Start();
        }
    }
}
