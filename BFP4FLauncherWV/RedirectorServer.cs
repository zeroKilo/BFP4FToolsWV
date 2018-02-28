using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlazeLibWV;

namespace BFP4FLauncherWV
{
    public static class RedirectorServer
    {
        public static readonly object _sync = new object();
        public static bool _exit;
        public static RichTextBox box = null;
        public static TcpListener lRedirector = null;
        public static int targetPort = 30000;
        
        public static void Start()
        {
            SetExit(false);
            Log("--==Blaze Backend by Warranty Voider==--");
            Log("");
            Log("Starting Redirector...");
            new Thread(tRedirectorMain).Start();
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }            
        }

        public static void Stop()
        {
            Log("Backend stopping...");
            if (lRedirector != null) lRedirector.Stop();
            SetExit(true);
            Log("Done.");
        }

        public static void tRedirectorMain(object obj)
        {
            try
            {
                Log("[REDI] Redirector starting...");
                lRedirector = new TcpListener(IPAddress.Parse("127.0.0.1"), 42127);
                Log("[REDI] Redirector bound to port 42127");
                lRedirector.Start();
                Log("[REDI] Loading Cert...");
                X509Certificate2 cert = new X509Certificate2(BFP4FLauncherWV.Resource1.redi, "123456");
                Log("[REDI] Redirector listening...");
                TcpClient client;
                while (!GetExit())
                {
                    client = lRedirector.AcceptTcpClient();
                    Log("[REDI] Client connected");
                    SslStream sslStream = new SslStream(client.GetStream(), false);
                    Log("[REDI] Authenticating...");
                    sslStream.AuthenticateAsServer(cert, false, SslProtocols.Ssl3, false);
                    Log("[REDI] Reading data...");
                    byte[] data = Helper.ReadContentSSL(sslStream);
                    Log("[REDI] Received " + data.Length + " bytes of data");
                    MemoryStream m = new MemoryStream();
                    m.Write(data, 0, data.Length);
                    data = CreateRedirectorPacket();
                    m.Write(data, 0, data.Length);
                    Log("[REDI] Sending response");
                    sslStream.Write(data);
                    sslStream.Flush();
                    client.Close();
                    Log("[REDI] Client disconnected");
                    File.WriteAllBytes("redidump.bin", m.ToArray());
                }
            }
            catch (Exception ex)
            {
                LogError("REDI", ex);
            }
        }

        public static byte[] CreateRedirectorPacket()
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            List<Blaze.Tdf> VALU = new List<Blaze.Tdf>();
            VALU.Add(Blaze.TdfString.Create("HOST", "localhost"));
            VALU.Add(Blaze.TdfInteger.Create("IP\0\0", Blaze.GetIPfromString("127.0.0.1")));
            VALU.Add(Blaze.TdfInteger.Create("PORT", targetPort));
            Blaze.TdfUnion ADDR = Blaze.TdfUnion.Create("ADDR", 0, Blaze.TdfStruct.Create("VALU", VALU));
            Result.Add(ADDR);
            Result.Add(Blaze.TdfInteger.Create("SECU", 0));
            Result.Add(Blaze.TdfInteger.Create("XDNS", 0));
            return Blaze.CreatePacket(5, 1, 0, 0x1000, 0, Result);
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
