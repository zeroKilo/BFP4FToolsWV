using System;
using System.IO;
using System.IO.Pipes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace UDPMonitorWV
{
    public partial class Form1 : Form
    {
        public NamedPipeServerStream pipeServerRecv;
        public NamedPipeServerStream pipeServerSend;
        public bool _exit = false;
        public static readonly object _syncRecv = new object();
        public static readonly object _syncSend = new object();
        public RichTextBox box;
        public List<string> msgBufRecv = new List<string>();
        public List<string> msgBufSend = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            box = rtb1;
            toolStripButton1.Enabled = false;
            new Thread(ServerThreadSend).Start();
            new Thread(MsgThreadSend).Start();
            new Thread(ServerThreadRecv).Start();
            new Thread(MsgThreadRecv).Start();
        }

        private void MsgThreadRecv(object data)
        {
            List<string> local = new List<string>();
            while (true)
            {
                lock (_syncRecv)
                {
                    if (_exit) break;
                    foreach (string s in msgBufRecv)
                        local.Add(s);
                    msgBufRecv.Clear();
                }
                if (local.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string s in local)
                        sb.AppendLine("Recv: " + s.Trim());
                    Log(sb.ToString());
                    local.Clear();
                }
                Thread.Sleep(100);
            }
        }

        private void MsgThreadSend(object data)
        {
            List<string> local = new List<string>();
            while (true)
            {
                lock (_syncSend)
                {
                    if (_exit) break;
                    foreach (string s in msgBufSend)
                        local.Add(s);
                    msgBufSend.Clear();
                }
                if (local.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string s in local)
                        sb.AppendLine("Send: " + s.Trim());
                    Log(sb.ToString());
                    local.Clear();
                }
                Thread.Sleep(100);
            }
        }

        private void ServerThreadRecv(object data)
        {
            try
            {
                while (true)
                {
                    lock (_syncRecv)
                    {
                        if (_exit) break;
                    }
                    pipeServerRecv = new NamedPipeServerStream("UDPmon_recv", PipeDirection.In, 1);                    
                    Log("Recv Named Pipe Server started\n");
                    pipeServerRecv.WaitForConnection();
                    Log("Recv New Client connected\n");
                    StreamReader reader = new StreamReader(pipeServerRecv);
                    string s;
                    try
                    {
                        while (pipeServerRecv.IsConnected)
                        {
                            lock (_syncRecv)
                            {
                                if (_exit) break;
                            }
                            s = reader.ReadLine();
                            if (s != null)
                            {
                                lock (_syncRecv)
                                {
                                    msgBufRecv.Add(s);
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Log("Error:" + ex.Message + "\n"); }
                    pipeServerRecv.Close();
                }
            }
            catch { }
        }
        
        private void ServerThreadSend(object data)
        {
            try
            {
                while (true)
                {
                    lock (_syncSend)
                    {
                        if (_exit) break;
                    }
                    pipeServerSend = new NamedPipeServerStream("UDPmon_send", PipeDirection.In, 1);
                    Log("Send Named Pipe Server started\n");
                    pipeServerSend.WaitForConnection();
                    Log("Send New Client connected\n");
                    StreamReader reader = new StreamReader(pipeServerSend);
                    string s;
                    try
                    {
                        while (pipeServerSend.IsConnected)
                        {
                            lock (_syncSend)
                            {
                                if (_exit) break;
                            }
                            s = reader.ReadLine();
                            if (s != null)
                            {
                                lock (_syncSend)
                                {
                                    msgBufSend.Add(s);
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Log("Error:" + ex.Message + "\n"); }
                    pipeServerSend.Close();
                }
            }
            catch { }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (_syncRecv)
            {
                _exit = true;
            }
            if (pipeServerRecv != null)
            {
                try
                {
                    NamedPipeClientStream clt = new NamedPipeClientStream(".", "UDPmon_send", PipeDirection.Out);
                    clt.Connect();
                    clt.Close();
                    clt = new NamedPipeClientStream(".", "UDPmon_recv", PipeDirection.Out);
                    clt.Connect();
                    clt.Close();
                }
                catch { }
                pipeServerRecv.Close();
            }
        }

        public void Log(string s, object color = null)
        {
            if (box == null) return;
            try
            {
                box.Invoke(new Action(delegate
                {
                    box.SelectionStart = box.TextLength;
                    box.SelectionLength = 0;
                    box.AppendText(s);
                    if (box.Text.Length > 10000)
                        box.Text = box.Text.Substring(box.Text.Length - 10000);
                    box.SelectionColor = box.ForeColor;
                    box.ScrollToCaret();
                }));
            }
            catch { }
        }
    }
}
