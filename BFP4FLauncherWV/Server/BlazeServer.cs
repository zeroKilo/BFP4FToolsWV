using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Drawing;
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
        public static int idCounter;
        public static List<PlayerInfo> allClients = new List<PlayerInfo>();
        
        public static void Start()
        {
            SetExit(false);
            Log("Starting Blaze...");
            new Thread(tBlazeMain).Start();
            idCounter = 1;
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
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
                Profiles.Refresh();
                lBlaze = new TcpListener(IPAddress.Parse(ProviderInfo.ip), 30001);
                Log("[MAIN] Blaze bound to " + ProviderInfo.ip + ":30001");
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
            allClients.Add(pi);
            pi.userId = idCounter++;
            pi.exIp = 0;
            pi.ns = ns;
            Log("[CLNT] #" + pi.userId + " Handler started");
            try
            {
                while (!GetExit())
                {
                    byte[] data = Helper.ReadContentTCP(ns);
                    if (data != null && data.Length != 0)
                        ProcessPackets(data, pi, ns);
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                LogError("CLNT", ex, "Handler " + pi.userId);
            }
            client.Close();
            Log("[CLNT] #" + pi.userId + " Client disconnected");
        }

        public static void ProcessPackets(byte[] data, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Packet> packets = Blaze.FetchAllBlazePackets(new MemoryStream(data));
            foreach (Blaze.Packet p in packets)
            {
                Log("[CLNT] #" + pi.userId + " " + Blaze.PacketToDescriber(p));
                switch (p.Component)
                {
                    case 0x1:
                        AuthenticationComponent.HandlePacket(p, pi, ns);
                        break;
                    case 0x4:
                        GameManagerComponent.HandlePacket(p, pi, ns);
                        break;
                    case 0x7:
                        StatsComponent.HandlePacket(p, pi, ns);
                        break;
                    case 0x9:
                        UtilComponent.HandlePacket(p, pi, ns);
                        break;
                    case 0x7802:
                        UserSessionComponent.HandlePacket(p, pi, ns);
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

        public static void Log(string s, object color = null)
        {
            if (box == null) return;
            try
            {
                box.Invoke(new Action(delegate
                {
                    string stamp = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " : ";
                    Color c;
                    if (color != null)
                        c = (Color)color;
                    else
                        c = Color.Black;
                    box.SelectionStart = box.TextLength;
                    box.SelectionLength = 0;
                    box.SelectionColor = c;
                    box.AppendText(stamp + s + "\n");
                    box.SelectionColor = box.ForeColor;
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

    public class Profile
    {
        public string name;
        public long sessionId;
        public static Profile Load(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            Profile p = new Profile();
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length != 2) continue;
                string what = parts[0].Trim().ToLower();
                switch (what)
                {
                    case "name":
                        p.name = parts[1].Trim();
                        break;
                    case "id":
                        p.sessionId = Convert.ToInt32(parts[1].Trim());
                        break;
                }
            }
            if (p.name == null || p.sessionId == 0)
                return null;
            return p;
        }
    }

    public static class Profiles
    {
        public static List<Profile> profiles = new List<Profile>();
        public static void Refresh()
        {
            profiles = new List<Profile>();
            if (!Directory.Exists("backend"))
                Directory.CreateDirectory("backend");
            if (!Directory.Exists("backend\\profiles"))
                Directory.CreateDirectory("backend\\profiles");
            string[] files = Directory.GetFiles("backend\\profiles\\", "*.txt");
            foreach (string file in files)
            {
                Profile p = Profile.Load(file);
                if (p != null)
                    profiles.Add(p);
            }
            BlazeServer.Log("[MAIN] Loaded " + profiles.Count + " player profiles");
        }

        public static Profile Create(string name)
        {
            Profile p = new Profile();
            p.name = name;
            long id = 1000;
            bool found = true;
            while (found)
            {
                found = false;
                foreach(Profile tmp in profiles)
                    if (tmp.sessionId == id)
                    {
                        found = true;
                        id++;
                        break;
                    }
            }
            p.sessionId = id;
            File.WriteAllText("backend\\profiles\\" + id.ToString("X8") + "_profile.txt", "name=" + name + "\nid=" + id, Encoding.Unicode);
            return p;
        }
    }
}
