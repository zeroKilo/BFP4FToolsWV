using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class AsyncGameManager
    {
        public static void NotifyPlatformHostInitialized(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
            result.Add(Blaze.TdfInteger.Create("PHID", pi.userId));
            result.Add(Blaze.TdfInteger.Create("PHST", 0));
            byte[] buff = Blaze.CreatePacket(4, 0x47, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyGameStateChange(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
            result.Add(Blaze.TdfInteger.Create("GSTA", pi.game.GSTA));
            byte[] buff = Blaze.CreatePacket(4, 0x64, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyServerGameSetup(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> input = Blaze.ReadPacketContent(p);
            uint t = Blaze.GetUnixTimeStamp();
            pi.game.GNAM = ((Blaze.TdfString)input[2]).Value;
            pi.game.GSET = ((Blaze.TdfInteger)input[3]).Value;
            pi.game.VOIP = ((Blaze.TdfInteger)input[21]).Value;
            pi.game.VSTR = ((Blaze.TdfString)input[22]).Value;
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            List<Blaze.Tdf> GAME = new List<Blaze.Tdf>();
                GAME.Add(Blaze.TdfList.Create("ADMN", 0, 1, new List<long>(new long[] { pi.userId })));
                GAME.Add(Blaze.TdfList.Create("CAP\0", 0, 2, new List<long>(new long[] { 0x20, 0 })));
                GAME.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
                GAME.Add(Blaze.TdfString.Create("GNAM", pi.game.GNAM));
                GAME.Add(Blaze.TdfInteger.Create("GPVH", 666));
                GAME.Add(Blaze.TdfInteger.Create("GSET", pi.game.GSET));
                GAME.Add(Blaze.TdfInteger.Create("GSID", 1));
                GAME.Add(Blaze.TdfInteger.Create("GSTA", pi.game.GSTA));
                GAME.Add(Blaze.TdfString.Create("GTYP", "AssaultStandard"));
                GAME.Add(BlazeHelper.CreateNETField(pi, "HNET"));
                GAME.Add(Blaze.TdfInteger.Create("HSES", 13666));
                GAME.Add(Blaze.TdfInteger.Create("IGNO", 0));
                GAME.Add(Blaze.TdfInteger.Create("MCAP", 0x20));
                GAME.Add(BlazeHelper.CreateNQOSField(pi, "NQOS"));
                GAME.Add(Blaze.TdfInteger.Create("NRES", 0));
                GAME.Add(Blaze.TdfInteger.Create("NTOP", 1));
                GAME.Add(Blaze.TdfString.Create("PGID", ""));
                List<Blaze.Tdf> PHST = new List<Blaze.Tdf>();
                    PHST.Add(Blaze.TdfInteger.Create("HPID", pi.userId));
                GAME.Add(Blaze.TdfStruct.Create("PHST", PHST));
                GAME.Add(Blaze.TdfInteger.Create("PRES", 1));
                GAME.Add(Blaze.TdfString.Create("PSAS", "wv"));
                GAME.Add(Blaze.TdfInteger.Create("QCAP", 0x10));
                GAME.Add(Blaze.TdfInteger.Create("SEED", 0x2CF2048F));
                GAME.Add(Blaze.TdfInteger.Create("TCAP", 0x10));
                List<Blaze.Tdf> THST = new List<Blaze.Tdf>();
                    THST.Add(Blaze.TdfInteger.Create("HPID", pi.userId));
                GAME.Add(Blaze.TdfStruct.Create("THST", THST));
                GAME.Add(Blaze.TdfList.Create("TIDS", 0, 2, new List<long>(new long[] { 1, 2 })));
                GAME.Add(Blaze.TdfString.Create("UUID", "f5193367-c991-4429-aee4-8d5f3adab938"));
                GAME.Add(Blaze.TdfInteger.Create("VOIP", pi.game.VOIP));
                GAME.Add(Blaze.TdfString.Create("VSTR", pi.game.VSTR));
            result.Add(Blaze.TdfStruct.Create("GAME", GAME));
            List<Blaze.TdfStruct> PROS = new List<Blaze.TdfStruct>();
                List<Blaze.Tdf> ee0 = new List<Blaze.Tdf>();
                    ee0.Add(Blaze.TdfInteger.Create("EXID", pi.userId));
                    ee0.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
                    ee0.Add(Blaze.TdfInteger.Create("LOC\0", pi.loc));
                    ee0.Add(Blaze.TdfString.Create("NAME", pi.name));
                    ee0.Add(Blaze.TdfInteger.Create("PID\0", pi.userId));
                    ee0.Add(BlazeHelper.CreateNETFieldUnion(pi, "PNET"));
                    ee0.Add(Blaze.TdfInteger.Create("SID\0", pi.slot));
                    ee0.Add(Blaze.TdfInteger.Create("SLOT", 0));
                    ee0.Add(Blaze.TdfInteger.Create("STAT", 2));
                    ee0.Add(Blaze.TdfInteger.Create("TIDX", 0xFFFF));
                    ee0.Add(Blaze.TdfInteger.Create("TIME", t));
                    ee0.Add(Blaze.TdfInteger.Create("UID\0", pi.userId));
                PROS.Add(Blaze.TdfStruct.Create("0", ee0));
            result.Add(Blaze.TdfList.Create("PROS", 3, 1, PROS));
            List<Blaze.Tdf> VALU = new List<Blaze.Tdf>();
                VALU.Add(Blaze.TdfInteger.Create("DCTX", 0));
            result.Add(Blaze.TdfUnion.Create("REAS", 0, Blaze.TdfStruct.Create("VALU", VALU)));
            byte[] buff = Blaze.CreatePacket(p.Component, 0x14, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyGameSettingsChange(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("ATTR", pi.game.GSET));
            result.Add(Blaze.TdfInteger.Create("GID", pi.game.id));
            byte[] buff = Blaze.CreatePacket(p.Component, 0x6E, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyClientGameSetup(Blaze.Packet p, PlayerInfo pi, PlayerInfo srv, NetworkStream ns, long reas = 1)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            List<Blaze.Tdf> GAME = new List<Blaze.Tdf>();
                GAME.Add(Blaze.TdfList.Create("ADMN", 0, 1, new List<long>(new long[] { srv.userId })));
                GAME.Add(srv.game.ATTR);
                GAME.Add(Blaze.TdfList.Create("CAP\0", 0, 2, new List<long>(new long[] { 0x20, 0 })));
                GAME.Add(Blaze.TdfInteger.Create("GID\0", srv.game.id));
                GAME.Add(Blaze.TdfString.Create("GNAM", srv.game.GNAM));
                GAME.Add(Blaze.TdfInteger.Create("GPVH", 666));
                GAME.Add(Blaze.TdfInteger.Create("GSET", srv.game.GSET));
                GAME.Add(Blaze.TdfInteger.Create("GSID", 1));
                GAME.Add(Blaze.TdfInteger.Create("GSTA", srv.game.GSTA));
                GAME.Add(Blaze.TdfString.Create("GTYP", "AssaultStandard"));
                GAME.Add(BlazeHelper.CreateNETField(srv, "HNET"));
                GAME.Add(Blaze.TdfInteger.Create("HSES", 13666));
                GAME.Add(Blaze.TdfInteger.Create("IGNO", 0));
                GAME.Add(Blaze.TdfInteger.Create("MCAP", 0x20));
                GAME.Add(BlazeHelper.CreateNQOSField(srv, "NQOS"));
                GAME.Add(Blaze.TdfInteger.Create("NRES", 0));
                GAME.Add(Blaze.TdfInteger.Create("NTOP", 1));
                GAME.Add(Blaze.TdfString.Create("PGID", ""));
                List<Blaze.Tdf> PHST = new List<Blaze.Tdf>();
                    PHST.Add(Blaze.TdfInteger.Create("HPID", srv.userId));
                    PHST.Add(Blaze.TdfInteger.Create("HSLT", srv.slot));
                GAME.Add(Blaze.TdfStruct.Create("PHST", PHST));
                GAME.Add(Blaze.TdfInteger.Create("PRES", 1));
                GAME.Add(Blaze.TdfString.Create("PSAS", "wv"));
                GAME.Add(Blaze.TdfInteger.Create("QCAP", 0x10));
                GAME.Add(Blaze.TdfInteger.Create("SEED", 0x2CF2048F));
                GAME.Add(Blaze.TdfInteger.Create("TCAP", 0x10));
                List<Blaze.Tdf> THST = new List<Blaze.Tdf>();
                    THST.Add(Blaze.TdfInteger.Create("HPID", srv.userId));
                    THST.Add(Blaze.TdfInteger.Create("HSLT", srv.slot));
                GAME.Add(Blaze.TdfStruct.Create("THST", THST));
                GAME.Add(Blaze.TdfList.Create("TIDS", 0, 2, new List<long>(new long[] { 1, 2 })));
                GAME.Add(Blaze.TdfString.Create("UUID", "f5193367-c991-4429-aee4-8d5f3adab938"));
                GAME.Add(Blaze.TdfInteger.Create("VOIP", srv.game.VOIP));
                GAME.Add(Blaze.TdfString.Create("VSTR", srv.game.VSTR));
            result.Add(Blaze.TdfStruct.Create("GAME", GAME));
            List<Blaze.TdfStruct> PROS = new List<Blaze.TdfStruct>();
                PROS.Add(BlazeHelper.MakePROSEntry(0, srv));
                PROS.Add(BlazeHelper.MakePROSEntry(1, pi));
            result.Add(Blaze.TdfList.Create("PROS", 3, 2, PROS));
            List<Blaze.Tdf> VALU = new List<Blaze.Tdf>();
                VALU.Add(Blaze.TdfInteger.Create("DCTX", reas));
            result.Add(Blaze.TdfUnion.Create("REAS", 0, Blaze.TdfStruct.Create("VALU", VALU)));            
            ushort id = 0x16;
            if (pi.version == "0.01.217848.3") //2010 client quirk?
                id = 0x14;
            byte[] buff = Blaze.CreatePacket(0x4, id, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyPlayerJoining(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            uint t = Blaze.GetUnixTimeStamp();
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
            List<Blaze.Tdf> PDAT = new List<Blaze.Tdf>();
                PDAT.Add(Blaze.TdfInteger.Create("EXID", pi.userId));
                PDAT.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
                PDAT.Add(Blaze.TdfInteger.Create("LOC\0", pi.loc));
                PDAT.Add(Blaze.TdfString.Create("NAME", pi.name));
                PDAT.Add(Blaze.TdfInteger.Create("PID\0", pi.userId));
                PDAT.Add(BlazeHelper.CreateNETFieldUnion(pi, "PNET"));
                PDAT.Add(Blaze.TdfInteger.Create("SID\0", pi.slot));
                PDAT.Add(Blaze.TdfInteger.Create("STAT", pi.stat));
                PDAT.Add(Blaze.TdfInteger.Create("TIDX", 0xFFFF));
                PDAT.Add(Blaze.TdfInteger.Create("TIME", t));
                PDAT.Add(Blaze.TdfInteger.Create("UID\0", pi.userId));                
            result.Add(Blaze.TdfStruct.Create("PDAT", PDAT));
            byte[] buff = Blaze.CreatePacket(0x4, 0x15, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void NotifyGamePlayerStateChange(Blaze.Packet p, PlayerInfo pi, NetworkStream ns, long stat)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
            result.Add(Blaze.TdfInteger.Create("PID\0", pi.userId));
            result.Add(Blaze.TdfInteger.Create("STAT", stat));
            byte[] buff = Blaze.CreatePacket(0x4, 0x74, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
        
        public static void PlayerJoinCompletedNotification(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
            result.Add(Blaze.TdfInteger.Create("PID\0", pi.userId));
            byte[] buff = Blaze.CreatePacket(0x4, 0x1E, 0, 0x2000, 0, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
