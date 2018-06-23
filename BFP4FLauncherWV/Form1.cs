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
            tabControl1.TabPages.Remove(tabPage6);
            tabControl1.SelectTab("TabPage2");
            this.Text += " - Build " + Resources.Resource1.BuildDate;
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
            Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RedirectorServer.Stop();
            BlazeServer.Stop();
            MagmaServer.Stop();
            Webserver.Stop();
        }

        private void launchStandaloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Enabled =
            launchStandaloneToolStripMenuItem.Enabled = false;
            RedirectorServer.targetPort = 30001;
            Start();
        }

        private void Start()
        {
            RedirectorServer.box =
            BlazeServer.box = rtb2;
            Webserver.box = rtb5;
            MagmaServer.box = rtb4;
            RedirectorServer.useSSL = checkBox1.Checked;
            RedirectorServer.Start();
            BlazeServer.Start();
            MagmaServer.Start();
            Webserver.Start();
            tabControl1.TabPages.Add(tabPage6);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string args = rtb3.Text.Replace("\r", "").Replace("\n", " ");
            while (args.Contains("  "))
                args = args.Replace("  ", " ");
            RunShell("bfp4f_w32ded.exe", args);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string name = toolStripTextBox1.Text;
            Profiles.Create(name);
            Profiles.Refresh();
            RefreshProfileList();
        }

        private void RefreshProfileList()
        {
            listBox1.Items.Clear();
            foreach (Profile p in Profiles.profiles)
                listBox1.Items.Add(p.sessionId + ": " + p.name);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Profiles.Refresh();
            RefreshProfileList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Profile p = Profiles.profiles[n];
            string path = "backend\\profiles\\" + p.sessionId.ToString("X8") + "_profile.txt";
            if (File.Exists(path))
                File.Delete(path);
            Profiles.Refresh();
            RefreshProfileList();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Profile p = Profiles.profiles[n];
            string args = Resources.Resource1.client_startup;
            args = args.Replace("#SESSION#", p.sessionId.ToString());
            args = args.Replace("#PLAYER#", p.name);
            RunShell("bfp4f.exe", args);
        }
    }
}
