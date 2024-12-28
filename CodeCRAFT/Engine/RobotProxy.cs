using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Engine
{
    public class RobotProxy
    {
        private RobotSpec _spec;

        public RobotProxy(RobotSpec spec)
        {
            this._spec = spec;
        }
    }
}
