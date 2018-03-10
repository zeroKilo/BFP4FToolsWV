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
            Blaze.TdfStruct inip = (Blaze.TdfStruct)valu.Values[1];
            pi.inIp = (uint)((Blaze.TdfInteger)inip.Values[0]).Value;
            pi.exPort = pi.inPort = (uint)((Blaze.TdfInteger)inip.Values[1]).Value;
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, new List<Blaze.Tdf>());
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
            LookupUserInformation(p, pi, ns);
        }

        public static void LookupUserInformation(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            List<Blaze.Tdf> ADDR = new List<Blaze.Tdf>();
            List<Blaze.Tdf> EXIP = new List<Blaze.Tdf>();
            EXIP.Add(Blaze.TdfInteger.Create("IP", 0x7F000001));
            EXIP.Add(Blaze.TdfInteger.Create("PORT", pi.inPort));
            ADDR.Add(Blaze.TdfStruct.Create("EXIP", EXIP));
            List<Blaze.Tdf> INIP = new List<Blaze.Tdf>();
            INIP.Add(Blaze.TdfInteger.Create("IP", pi.inIp));
            INIP.Add(Blaze.TdfInteger.Create("PORT", pi.inPort));
            ADDR.Add(Blaze.TdfStruct.Create("INIP", INIP));
            Result.Add(Blaze.TdfStruct.Create("ADDR", ADDR, true));
            List<Blaze.Tdf> NQOS = new List<Blaze.Tdf>();
            NQOS.Add(Blaze.TdfInteger.Create("DBPS", 0));
            NQOS.Add(Blaze.TdfInteger.Create("NATT", 4));
            NQOS.Add(Blaze.TdfInteger.Create("UBPS", 0));
            Result.Add(Blaze.TdfStruct.Create("NQOS", NQOS)); 
            byte[] buff = Blaze.CreatePacket(p.Component, 1, 0, 0x2000, 0, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();

        }
    }
}
