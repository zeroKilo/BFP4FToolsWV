using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FLauncherWV
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            RefreshProfiles();
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            this.Text += " - Build " + Resources.Resource1.BuildDate;
            button6.Top = splitContainer1.Panel1.Height - button6.Height - 10;
        }

        private void RefreshProfiles()
        {
            Profiles.Refresh();
            comboBox1.Items.Clear();
            foreach (Profile p in Profiles.profiles)
                comboBox1.Items.Add(p.sessionId + ": " + p.name);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            ProviderInfo.backendIP = textBox1.Text;
            RedirectorServer.useSSL = checkBox1.Checked;
            if (!checkBox2.Checked)
                RedirectorServer.targetPort = 30001;
            MagmaServer.basicMode = true;
            RedirectorServer.box =
            BlazeServer.box =
            Webserver.box =
            MagmaServer.box = rtb1;
            RedirectorServer.Start();
            BlazeServer.Start();
            MagmaServer.Start();
            Webserver.Start();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            RedirectorServer.Stop();
            BlazeServer.Stop();
            MagmaServer.Stop();
            Webserver.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string args = "+key \"eakey\" +useServerMonitorTool 0 +soldierName \"test-server\" +sessionId 1234 +magmaProtocol http +magmaHost \"#HOSTIP#\" +magma 1 +guid \"5678\" +secret \"secret\"";
            args = args.Replace("#HOSTIP#", textBox1.Text);
            Helper.RunShell("bfp4f_w32ded.exe", args);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Helper.KillRunningProcesses();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int n = comboBox1.SelectedIndex;
            if (n == -1)
                return;
            Profile p = Profiles.profiles[n];
            string args = Resources.Resource1.client_startup;
            args = args.Replace("#SESSION#", p.sessionId.ToString());
            args = args.Replace("#PLAYER#", p.name);
            args = args.Replace("#IP#", textBox1.Text);
            Helper.RunShell("bfp4f.exe", args);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form3 f = new Form3();
            f.ShowDialog();
            RefreshProfiles();
        }

        private void splitContainer1_Panel1_SizeChanged(object sender, EventArgs e)
        {
            button6.Top = splitContainer1.Panel1.Height - button6.Height - 10;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form f = new Form1();
            f.ShowDialog();
            this.Close();
        }
    }
}
