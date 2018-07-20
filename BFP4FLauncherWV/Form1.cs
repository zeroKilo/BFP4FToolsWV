using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.SelectTab("TabPage2");
            this.Text += " - Build " + Resources.Resource1.BuildDate;
            RefreshProfiles();
        }

        private void RefreshProfiles()
        {
            Profiles.Refresh();
            toolStripComboBox1.Items.Clear();
            foreach (Profile p in Profiles.profiles)
                toolStripComboBox1.Items.Add(p.id + ": " + p.name);
        }

        private void launchToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string args = rtb1.Text.Replace("\r", "").Replace("\n", " ");
            while (args.Contains("  "))
                args = args.Replace("  ", " ");
            Helper.RunShell("bfp4f.exe", args);
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
            BackendLog.Clear();
            ProviderInfo.backendIP = toolStripTextBox3.Text;
            RedirectorServer.box =
            BlazeServer.box = rtb2;
            Webserver.box = rtb5;
            MagmaServer.box = rtb4;
            RedirectorServer.useSSL = checkBox1.Checked;
            RedirectorServer.Start();
            BlazeServer.Start();
            MagmaServer.Start();
            Webserver.Start();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string args = rtb3.Text.Replace("\r", "").Replace("\n", " ");
            while (args.Contains("  "))
                args = args.Replace("  ", " ");
            Helper.RunShell("bfp4f_w32ded.exe", args);
        }

        private void killRunningProcessesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helper.KillRunningProcesses();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            if (n == -1)
                return;
            Profile p = Profiles.profiles[n];
            string args = Resources.Resource1.client_startup;
            args = args.Replace("#SESSION#", p.id.ToString());
            args = args.Replace("#PLAYER#", p.name);
            args = args.Replace("#IP#", toolStripTextBox4.Text);
            Helper.RunShell("bfp4f.exe", args);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Form3 f = new Form3();
            f.ShowDialog();
            RefreshProfiles();
        }
    }
}
