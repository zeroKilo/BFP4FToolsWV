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
                case 0x28:
                    Login(p, pi, ns);
                    break;
                case 0x64:
                    ListPersonas(p, pi, ns);
                    break;
                case 0x6E:
                    LoginPersona(p, pi, ns);
                    break;
                case 0x78:
                    LogoutPersona(p, pi, ns);
                    break;
            }
        }

        public static void Login(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            if (!pi.isServer)
            {
                List<Blaze.Tdf> input = Blaze.ReadPacketContent(p);
                Blaze.TdfString TOKN = (Blaze.TdfString)input[3];
                long id = Convert.ToInt32(TOKN.Value);
                foreach (Profile profile in Profiles.profiles)
                    if (profile.id == id)
                    {
                        pi.profile = profile;
                        break;
                    }
                if (pi.profile == null)
                {
                    BlazeServer.Log("[CLNT] #" + pi.userId + " Could not find player profile for token 0x" + id.ToString("X") + "!", System.Drawing.Color.Red);
                    pi.userId = 0;
                    return;
                }
                else
                {
                    for(int i=0;i<BlazeServer.allClients.Count;i++)
                        if (BlazeServer.allClients[i].userId == id)
                        {
                            BlazeServer.allClients.RemoveAt(i);
                            i--;
                        }
                    pi.userId = id;
                    BlazeServer.Log("[CLNT] New ID #" + pi.userId + " Client Playername = \"" + pi.profile.name + "\"", System.Drawing.Color.Blue);
                }
                
            }
            uint t = Blaze.GetUnixTimeStamp();
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(Blaze.TdfString.Create("LDHT", ""));
            Result.Add(Blaze.TdfInteger.Create("NTOS", 0));
            Result.Add(Blaze.TdfString.Create("PCTK", ""));
            List<Blaze.TdfStruct> playerentries = new List<Blaze.TdfStruct>();
            List<Blaze.Tdf> PlayerEntry = new List<Blaze.Tdf>();
            PlayerEntry.Add(Blaze.TdfString.Create("DSNM", pi.profile.name));
            PlayerEntry.Add(Blaze.TdfInteger.Create("LAST", t));
            PlayerEntry.Add(Blaze.TdfInteger.Create("PID\0", pi.userId));
            PlayerEntry.Add(Blaze.TdfInteger.Create("STAS", 2));
            PlayerEntry.Add(Blaze.TdfInteger.Create("XREF", 0));
            PlayerEntry.Add(Blaze.TdfInteger.Create("XTYP", 0));
            playerentries.Add(Blaze.TdfStruct.Create("0", PlayerEntry));
            Result.Add(Blaze.TdfList.Create("PLST", 3, 1, playerentries));
            Result.Add(Blaze.TdfString.Create("PRIV", ""));
            Result.Add(Blaze.TdfString.Create("SKEY", "some_client_key"));
            Result.Add(Blaze.TdfInteger.Create("SPAM", 1));
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
            SESS.Add(Blaze.TdfString.Create("KEY\0", "some_client_key"));
            SESS.Add(Blaze.TdfInteger.Create("LLOG", t));
            SESS.Add(Blaze.TdfString.Create("MAIL", ""));
            List<Blaze.Tdf> PDTL = new List<Blaze.Tdf>();
            PDTL.Add(Blaze.TdfString.Create("DSNM", pi.profile.name));
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

            AsyncUserSessions.NotifyUserAdded(pi, p, pi, ns);
            AsyncUserSessions.NotifyUserStatus(pi, p, pi, ns);
        }

        public static void LogoutPersona(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush(); 

            AsyncUserSessions.NotifyUserRemoved(pi, p, pi.userId, ns);
            AsyncUserSessions.NotifyUserStatus(pi, p, pi, ns);
        }

        public static void ListPersonas(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            List<Blaze.TdfStruct> entries = new List<Blaze.TdfStruct>();
                List<Blaze.Tdf> e = new List<Blaze.Tdf>();
                    e.Add(Blaze.TdfString.Create("DSNM", pi.profile.name));
                    e.Add(Blaze.TdfInteger.Create("LAST", Blaze.GetUnixTimeStamp()));
                    e.Add(Blaze.TdfInteger.Create("PID\0", pi.profile.id));
                    e.Add(Blaze.TdfInteger.Create("STAS", 2));
                    e.Add(Blaze.TdfInteger.Create("XREF", 0));
                    e.Add(Blaze.TdfInteger.Create("XTYP", 0));
                entries.Add(Blaze.TdfStruct.Create("0", e));
            result.Add(Blaze.TdfList.Create("PINF", 3, 1, entries));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
