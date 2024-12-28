using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Engine
{
    public class BulletPeer
    {
        public int Id { get; internal set; }
        public double X { get; internal set; }
        public double Y { get; internal set; }
        public BulletState State { get; internal set; }

        internal void Update()
        {
            throw new NotImplementedException();
        }
    }
}
