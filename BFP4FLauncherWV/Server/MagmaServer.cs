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

namespace BFP4FLauncherWV
{
    public static class MagmaServer
    {
        public static bool basicMode = false;
        public static readonly object _sync = new object();
        public static bool _exit;
        public static RichTextBox box = null;
        public static TcpListener lMagma = null;
        public static Dictionary<int, int> entitlements;

        public static void Start()
        {
            SetExit(false);
            Log("Starting Magma...");
            LoadEntitlements();
            new Thread(tHTTPMain).Start();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }

        private static void LoadEntitlements()
        {
            if (!Directory.Exists("backend"))
                Directory.CreateDirectory("backend");
            string name = "backend\\entitlements.txt";
            if (!File.Exists(name))
                File.WriteAllText(name, Resources.Resource1.default_entitlement_map);
            string[] lines = File.ReadAllLines(name);
            entitlements = new Dictionary<int, int>();
            foreach (string line in lines)
            {
                if (line.Trim() == "" || line.Trim().StartsWith("#"))
                    continue;
                string[] parts = line.Split(';');
                entitlements.Add(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]));
            }
        }

        public static void Stop()
        {
            Log("Magma stopping...");
            if (lMagma != null) lMagma.Stop();
            SetExit(true);
            Log("Done.");
        }

        public static void tHTTPMain(object obj)
        {
            try
            {
                Log("[MGMA] starting...");
                lMagma = new TcpListener(IPAddress.Parse(ProviderInfo.backendIP), 80);
                Log("[MGMA] bound to  " + ProviderInfo.backendIP + ":80");
                lMagma.Start();
                Log("[MGMA] listening...");
                TcpClient client;
                while (!GetExit())
                {
                    client = lMagma.AcceptTcpClient();
                    NetworkStream ns = client.GetStream();
                    byte[] data = Helper.ReadContentTCP(ns);
                    if(!basicMode)
                        Log("[MGMA] Recvdump:\n" + Encoding.ASCII.GetString(data));
                    try
                    {
                        ProcessMagma(Encoding.ASCII.GetString(data), ns);
                    }
                    catch { }
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                LogError("MGMA", ex);
            }
        }

        public static void ProcessMagma(string data, Stream s)
        {
            string[] lines = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Log("[MGMA] Request: " + lines[0]);
            string cmd = lines[0].Split(' ')[0];
            string url = lines[0].Split(' ')[1].Split(':')[0];
            if (cmd == "GET")
            {
                switch (url)
                {
                    case "/api/nucleus/authToken":
                        Log("[MGMA] Sending AuthToken");
                        if (lines.Length > 5 && lines[5].StartsWith("x-server-key"))
                            ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<success><token>" + lines[5].Split(':')[1].Trim() + "</token></success>");
                        else
                            ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<success><token code=\"NEW_TOKEN\">" + lines[4].Split('=')[1] + "</token></success>");
                        return;
                    case "/api/relationships/roster/nucleus":
                        Log("[MGMA] Sending Roster response");
                        ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<roster relationships=\"0\"/><success code=\"SUCCESS\"/>");
                        return;
                    case "/wv/getProfiles":
                        Log("[MGMA] Sending Player Profiles");
                        StringBuilder sb = new StringBuilder();
                        Profiles.Refresh();
                        sb.Append("<profiles>\r\n");
                        foreach (Profile p in Profiles.profiles)
                            sb.Append("<profile name='" + Profiles.getProfilePath(p.id) + "'>" + Convert.ToBase64String(Encoding.Unicode.GetBytes(p._raw)) + "</profile>\r\n");
                        sb.Append("</profiles>\r\n");
                        ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n" + sb.ToString());
                        break;
                }
                if (url.StartsWith("/api/nucleus/name/"))
                {
                    int id = Convert.ToInt32(url.Substring(18));
                    Log("[MGMA] Sending name response for PID " + id);
                    PlayerInfo p = null;
                    foreach(PlayerInfo pi in BlazeServer.allClients)
                        if (pi.userId == id)
                        {
                            p = pi;
                            break;
                        }
                    if (p == null)
                    {
                        Log("[MGMA] Cant find player id!");
                        return;
                    }
                    ReplyWithXML(s, "<name>" + p.profile.name + "</name>");
                }
                if (url.StartsWith("/api/nucleus/entitlements/"))
                {
                    int id = Convert.ToInt32(url.Substring(26));
                    Log("[MGMA] Sending entitlement response for PID " + id);
                    PlayerInfo p = null;
                    foreach (PlayerInfo pi in BlazeServer.allClients)
                        if (pi.userId == id)
                        {
                            p = pi;
                            break;
                        }
                    if (p == null)
                    {
                        Log("[MGMA] Cant find player id!");
                        return;
                    }
                    string response = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><entitlements count=\"" + entitlements.Count + "\">";
                    int i = 1;
                    foreach (KeyValuePair<int, int> pair in entitlements)
                    {
                        response += "<entitlement><entitlementId>"
                                 + Convert.ToString(i)
                                 + "</entitlementId><entitlementTag>"
                                 + pair.Key
                                 + "-UNLM-</entitlementTag><useCount>" + pair.Value + "</useCount><grantDate>"
                                 + DateTime.UtcNow.ToString("MMM-dd-yyyy HH:mm:ss UTC")
                                 + "</grantDate><terminationDate></terminationDate><status>ACTIVE</status></entitlement>";
                        i++;
                    }
                    response += "</entitlements>";
                    ReplyWithXML(s, response);
                }                
            }
            if (cmd == "POST" && !basicMode)
            {
                int pos = data.IndexOf("\r\n\r\n");
                if (pos != -1)
                    Log("[MGMA] Content: \n" + data.Substring(pos + 4));
            }
        }

        public static void ReplyWithXML(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Warranty Voiders");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: Keep-Alive");
            sb.AppendLine();
            sb.Append(c);
            if (!basicMode)
            {
                Log("[MGMA] Sending: \n" + sb.ToString());
            }
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
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
                    box.AppendText(stamp + s + "\n");
                    BackendLog.Write(stamp + s + "\n");
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
