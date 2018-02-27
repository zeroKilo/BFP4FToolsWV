using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlazeLibWV;

namespace BFP4FLauncherWV
{
    public static class BlazeServer
    {
        public static readonly object _sync = new object();
        public static bool _exit;
        public static RichTextBox box = null;
        public static TcpListener lBlaze = null;
        public static int nextClientID;
        
        public static void Start()
        {
            SetExit(false);
            Log("Starting Blaze...");
            new Thread(tBlazeMain).Start();
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
            nextClientID = 1;
        }

        public static void Stop()
        {
            Log("Backend stopping...");
            if (lBlaze != null) lBlaze.Stop();
            SetExit(true);
            Log("Done.");
        }

        public static void tBlazeMain(object obj)
        {
            try
            {
                Log("[MAIN] Blaze starting...");
                lBlaze = new TcpListener(IPAddress.Parse("127.0.0.1"), 30001);
                Log("[MAIN] Blaze bound to port 30001");
                lBlaze.Start();
                Log("[MAIN] Blaze listening...");
                TcpClient client;
                while (!GetExit())
                {
                    client = lBlaze.AcceptTcpClient();
                    Log("[MAIN] Client connected");
                    new Thread(tBlazeClientHandler).Start(client);
                }
            }
            catch (Exception ex)
            {
                LogError("MAIN", ex);
            }
        }

        public static void tBlazeClientHandler(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream ns = client.GetStream();
            PlayerInfo pi = new PlayerInfo();
            pi.id = nextClientID;
            pi.name = "Soldier";
            pi.userId = 1;
            nextClientID++;
            Log("[CLNT] #" + pi.id + " Handler started");
            try
            {
                while (!GetExit())
                {
                    byte[] data = Helper.ReadContentTCP(ns);
                    if (data != null && data.Length != 0)
                    {
                        Log("[CLNT] #" + pi.id + " Received " + data.Length + " bytes of data");
                        ProcessPackets(data, pi, ns);
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                LogError("CLNT", ex, "Handler " + pi.id);
            }
            client.Close();
            Log("[CLNT] #" + pi.id + " Client disconnected");
        }

        public static void ProcessPackets(byte[] data, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Packet> packets = Blaze.FetchAllBlazePackets(new MemoryStream(data));
            foreach (Blaze.Packet p in packets)
            {
                Log("[CLNT] #" + pi.id + " " + Blaze.PacketToDescriber(p));
                switch (p.Component)
                {
                    case 0x1://Authentication Component
                        AuthenticationComponent.HandlePacket(p, pi, ns);
                        break;
                    case 0x9://Util Component
                        UtilComponent.HandlePacket(p, pi, ns);
                        break;
                }
            }
        }

        public static void SetExit(bool state)
        {
            lock (_sync)
            {
                _exit = state;
            }
        }

        public static bool GetExit()
        {
            bool result;
            lock (_sync)
            {
                result = _exit;
            }
            return result;
        }

        public static void Log(string s)
        {
            if (box == null) return;
            try
            {
                box.Invoke(new Action(delegate
                {
                    string stamp = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " : ";
                    box.Text += stamp + s + "\n";
                    box.SelectionStart = box.Text.Length;
                    box.ScrollToCaret();
                }));
            }
            catch { }
        }

        public static void LogError(string who, Exception e, string cName = "")
        {
            string result = "";
            if (who != "") result = "[" + who + "] " + cName + " ERROR: ";
            result += e.Message;
            if (e.InnerException != null)
                result += " - " + e.InnerException.Message;
            Log(result);
        }
    }
}
