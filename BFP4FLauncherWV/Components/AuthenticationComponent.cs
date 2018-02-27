using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class AuthenticationComponent
    {

        public static void HandlePacket(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            switch (p.Command)
            {
                case 0x28://Login
                    Login(p, pi, ns);
                    break;
                case 0x6E:
                    LoginPersona(p, pi, ns);
                    break;
            }
        }

        public static void Login(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            uint t = Blaze.GetUnixTimeStamp();
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(Blaze.TdfString.Create("LDHT", ""));
            Result.Add(Blaze.TdfInteger.Create("NTOS", 0));
            Result.Add(Blaze.TdfString.Create("PCTK", "SIxmvSLJSOwKPq5WZ3FL5KIRNJVCLp4Jgs_3mJcY2yJahXxR5mTRGUsi6PKhA4X1jpuVMxHJQv3WQ3HnQfvKeG60hRugA"));
            List<Blaze.TdfStruct> playerentries = new List<Blaze.TdfStruct>();
            List<Blaze.Tdf> PlayerEntry = new List<Blaze.Tdf>();
            PlayerEntry.Add(Blaze.TdfString.Create("DSNM", pi.name));
            PlayerEntry.Add(Blaze.TdfInteger.Create("LAST", t));
            PlayerEntry.Add(Blaze.TdfInteger.Create("PID\0", pi.id));
            PlayerEntry.Add(Blaze.TdfInteger.Create("STAS", 0));
            PlayerEntry.Add(Blaze.TdfInteger.Create("XREF", 0));
            PlayerEntry.Add(Blaze.TdfInteger.Create("XTYP", 0));
            playerentries.Add(Blaze.TdfStruct.Create("0", PlayerEntry));
            Result.Add(Blaze.TdfList.Create("PLST", 3, 1, playerentries));
            Result.Add(Blaze.TdfString.Create("PRIV", ""));
            Result.Add(Blaze.TdfString.Create("SKEY", "11229301_9b171d92cc562b293e602ee8325612e7"));
            Result.Add(Blaze.TdfInteger.Create("SPAM", 0));
            Result.Add(Blaze.TdfString.Create("THST", ""));
            Result.Add(Blaze.TdfString.Create("TSUI", ""));
            Result.Add(Blaze.TdfString.Create("TURI", ""));
            Result.Add(Blaze.TdfInteger.Create("UID\0", pi.userId));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
        public static void LoginPersona(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            uint t = Blaze.GetUnixTimeStamp();
            List<Blaze.Tdf> SESS = new List<Blaze.Tdf>();
            SESS.Add(Blaze.TdfInteger.Create("BUID", pi.userId));
            SESS.Add(Blaze.TdfInteger.Create("FRST", 0));
            SESS.Add(Blaze.TdfString.Create("KEY\0", "11229301_9b171d92cc562b293e602ee8325612e7"));
            SESS.Add(Blaze.TdfInteger.Create("LLOG", t));
            SESS.Add(Blaze.TdfString.Create("MAIL", ""));
            List<Blaze.Tdf> PDTL = new List<Blaze.Tdf>();
            PDTL.Add(Blaze.TdfString.Create("DSNM", pi.name));
            PDTL.Add(Blaze.TdfInteger.Create("LAST", t));
            PDTL.Add(Blaze.TdfInteger.Create("PID\0", pi.userId));
            PDTL.Add(Blaze.TdfInteger.Create("STAS", 0));
            PDTL.Add(Blaze.TdfInteger.Create("XREF", 0));
            PDTL.Add(Blaze.TdfInteger.Create("XTYP", 0));
            SESS.Add(Blaze.TdfStruct.Create("PDTL", PDTL));
            SESS.Add(Blaze.TdfInteger.Create("UID\0", pi.userId));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, SESS);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
            //CreateAuthPacketA(p, pi, ns);
        }
        //public static void CreateAuthPacketA(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        //{
        //    List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
        //    List<Blaze.Tdf> DATA = new List<Blaze.Tdf>();
        //    DATA.Add(Blaze.TdfUnion.Create("ADDR"));
        //    DATA.Add(Blaze.TdfString.Create("BPS\0", ""));
        //    DATA.Add(Blaze.TdfDoubleList.Create("CMAP", 0, 0, null, null, 0));
        //    DATA.Add(Blaze.TdfString.Create("CTY\0", ""));
        //    DATA.Add(Blaze.TdfDoubleList.Create("DMAP", 0, 0, null, null, 0));
        //    DATA.Add(Blaze.TdfInteger.Create("HWFG", 0));
        //    List<Blaze.Tdf> QDAT = new List<Blaze.Tdf>();
        //    QDAT.Add(Blaze.TdfInteger.Create("DBPS", 0));
        //    QDAT.Add(Blaze.TdfInteger.Create("NATT", 0));
        //    QDAT.Add(Blaze.TdfInteger.Create("UBPS", 0));
        //    DATA.Add(Blaze.TdfStruct.Create("QDAT", QDAT));
        //    DATA.Add(Blaze.TdfInteger.Create("UATT", 0));
        //    DATA.Add(Blaze.TdfDoubleList.Create("ULST", 0, 0, null, null, 0));
        //    Result.Add(Blaze.TdfStruct.Create("DATA", DATA));
        //    List<Blaze.Tdf> USER = new List<Blaze.Tdf>();
        //    USER.Add(Blaze.TdfInteger.Create("AID\0", pi.userId));
        //    USER.Add(Blaze.TdfInteger.Create("ALOC", 0x64654445));
        //    USER.Add(Blaze.TdfBlob.Create("EXBB", new byte[0]));
        //    USER.Add(Blaze.TdfInteger.Create("EXID", 0));
        //    USER.Add(Blaze.TdfInteger.Create("ID\0\0", pi.userId));
        //    USER.Add(Blaze.TdfString.Create("NAME", pi.name));
        //    Result.Add(Blaze.TdfStruct.Create("USER", USER));
        //    byte[] buff = Blaze.CreatePacket(0x7802, 2, 0, 0x2000, 0, Result);
        //    ns.Write(buff, 0, buff.Length);
        //    ns.Flush();
        //    Result = new List<Blaze.Tdf>();
        //    Result.Add(Blaze.TdfInteger.Create("FLGS", 3));
        //    Result.Add(Blaze.TdfInteger.Create("ID\0\0", pi.userId));
        //    buff = Blaze.CreatePacket(0x7802, 5, 0, 0x2000, 0, Result);
        //    ns.Write(buff, 0, buff.Length);
        //    ns.Flush();
        //}

    }
}
