using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class GameManagerComponent
    {

        public static void HandlePacket(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            switch (p.Command)
            {
                case 0x1:
                    CreateGame(p, pi, ns);
                    break;
                case 0x3:
                    AdvanceGameState(p, pi, ns);
                    break;
                case 0x7:
                    SetGameAttributes(p, pi, ns);
                    break;
                case 0x9:
                    JoinGame(p, pi, ns);
                    break;
                case 0xf:
                    FinalizeGameCreation(p, pi, ns);
                    break;
            }
        }

        public static void CreateGame(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            pi.stat = 4;
            pi.slot = pi.game.getNextSlot();
            pi.game.setNextSlot((int)pi.userId);
            pi.game.id = 1;
            pi.game.isRunning = true;
            pi.game.GSTA = 7;

            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
            result.Add(Blaze.TdfInteger.Create("GSTA", pi.game.GSTA));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();

            AsyncGameManager.NotifyGameStateChange(p, pi, ns);
            AsyncGameManager.NotifyGameSetup(p, pi, ns);
        }
        
        public static void AdvanceGameState(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> input = Blaze.ReadPacketContent(p);
            pi.game.GSTA = (uint)((Blaze.TdfInteger)input[1]).Value;
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, new List<Blaze.Tdf>());
            ns.Write(buff, 0, buff.Length);
            ns.Flush();

            AsyncGameManager.NotifyGameStateChange(p, pi, ns);
        }

        public static void SetGameAttributes(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> input = Blaze.ReadPacketContent(p);
            pi.game.ATTR = (Blaze.TdfDoubleList)input[0];
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();

            AsyncGameManager.NotifyGameSettingsChange(p, pi, ns);
        }

        public static void JoinGame(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            PlayerInfo srv = null;
            foreach(PlayerInfo info in BlazeServer.allClients)
                if (info.isServer)
                {
                    srv = info;
                    break;
                }
            if (srv == null)
            {
                BlazeServer.Log("[CLNT] #" + pi.userId + " : cant find game to join!", System.Drawing.Color.Red);
                return;
            }
            pi.game = srv.game;
            pi.slot = srv.game.getNextSlot();
            srv.game.setNextSlot((int)pi.userId);

            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("GID\0", srv.game.id));
            result.Add(Blaze.TdfInteger.Create("JGS\0", 0));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();

            AsyncUserSessions.NotifyUserAdded(p, srv, ns);
            AsyncUserSessions.NotifyUserStatus(p, srv, ns);
            AsyncGameManager.NotifyGameSetup(p, pi, srv, ns);

            AsyncGameManager.NotifyPlayerJoining(p, pi, srv.ns);
            AsyncGameManager.NotifyClaimingReservation(p, pi, srv.ns);
            AsyncUserSessions.UserSessionExtendedDataUpdateNotification(p, pi, srv.ns);
        }

        public static void FinalizeGameCreation(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();

            if (pi.isServer)
                AsyncGameManager.NotifyPlatformHostInitialized(p, pi, ns);
        }
    }
}
