using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class AsyncUserSessions
    {
        public static void UserSessionExtendedDataUpdateNotification(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(BlazeHelper.CreateUserDataStruct(pi));
            Result.Add(Blaze.TdfInteger.Create("USID", pi.userId));
            byte[] buff = Blaze.CreatePacket(0x7802, 1, 0, 0x2000, 0, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyUserAdded(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(BlazeHelper.CreateUserDataStruct(pi));
            result.Add(BlazeHelper.CreateUserStruct(pi));
            byte[] buff = Blaze.CreatePacket(0x7802, 0x2, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyUserStatus(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("FLGS", 3));
            result.Add(Blaze.TdfInteger.Create("ID\0\0", pi.userId));
            byte[] buff = Blaze.CreatePacket(0x7802, 0x5, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
