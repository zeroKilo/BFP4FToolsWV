using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public class PlayerInfo
    {
        public string name;
        public string version;
        public long userId;
        public long exIp, exPort;
        public long inIp, inPort;
        public long nat = 0;
        public long loc;
        public long slot;
        public long stat;
        public bool isServer;
        public GameInfo game;
        public NetworkStream ns;
    }
}
