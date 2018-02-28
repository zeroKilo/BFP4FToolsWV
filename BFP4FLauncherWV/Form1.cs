using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FLauncherWV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            tabControl1.SelectTab("TabPage2");
        }

        public static void RunShell(string file, string command)
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = file;
            startInfo.Arguments = command;
            process.StartInfo = startInfo;
            process.Start();
        }

        private void launchToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string args = rtb1.Text.Replace("\r", "").Replace("\n", " ");
            while (args.Contains("  "))
                args = args.Replace("  ", " ");
            RunShell("bfp4f.exe", args);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Enabled =
            launchStandaloneToolStripMenuItem.Enabled = false;
            RedirectorServer.box =
            BlazeServer.box =
            HttpServer.box = rtb2;
            RedirectorServer.Start();
            BlazeServer.Start();
            HttpServer.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RedirectorServer.Stop();
            BlazeServer.Stop();
            HttpServer.Stop();
        }

        private void launchStandaloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Enabled =
            launchStandaloneToolStripMenuItem.Enabled = false;
            RedirectorServer.targetPort = 30001;
            RedirectorServer.box =
            BlazeServer.box =
            HttpServer.box = rtb2;
            RedirectorServer.Start();
            BlazeServer.Start();
            HttpServer.Start();
        }
    }
}
