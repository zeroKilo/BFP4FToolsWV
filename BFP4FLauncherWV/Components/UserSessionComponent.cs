using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class UserSessionComponent
    {

        public static void HandlePacket(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            switch (p.Command)
            {
                case 0x14:
                    UpdateNetworkInfo(p, pi, ns);
                    break;
            }
        }

        public static void UpdateNetworkInfo(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> input = Blaze.ReadPacketContent(p);
            Blaze.TdfUnion addr = (Blaze.TdfUnion)input[0];
            Blaze.TdfStruct valu = (Blaze.TdfStruct)addr.UnionContent;
            Blaze.TdfStruct exip = (Blaze.TdfStruct)valu.Values[0];
            Blaze.TdfStruct inip = (Blaze.TdfStruct)valu.Values[1];
            pi.inIp = ((Blaze.TdfInteger)inip.Values[0]).Value;
            pi.exIp = ((Blaze.TdfInteger)exip.Values[0]).Value;
            pi.exPort = pi.inPort = (uint)((Blaze.TdfInteger)inip.Values[1]).Value;
            Blaze.TdfStruct nqos = (Blaze.TdfStruct)input[2];
            pi.nat = ((Blaze.TdfInteger)nqos.Values[1]).Value;
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, new List<Blaze.Tdf>());
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
            AsyncUserSessions.UserSessionExtendedDataUpdateNotification(pi, p, pi, ns);
        }
    }
}
