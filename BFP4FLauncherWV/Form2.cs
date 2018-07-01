using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
                comboBox1.Items.Add(p.id + ": " + p.name);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled =
            button6.Enabled = false;
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
            args = args.Replace("#SESSION#", p.id.ToString());
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

        private void button7_Click(object sender, EventArgs e)
        {
            Button btnSender = (Button)sender;
            Point ptLowerLeft = new Point(0, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            contextMenuStrip1.Show(ptLowerLeft);
        }

        private void sethostsFileToIPToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string ip = textBox1.Text.Trim();
            if (MessageBox.Show("This will overwrite your current 'hosts' file in 'C:\\Windows\\System32\\drivers\\etc\\', are you sure?", "Security Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                string content = Resources.Resource1.template_hosts_file.Replace("#IP#", ip);
                File.WriteAllText("C:\\Windows\\System32\\drivers\\etc\\hosts", content);
                MessageBox.Show("Done.");
            }
        }

        private void syncPlayerProfilesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will overwrite your local player profiles, are you sure?", "Security Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string ip = textBox1.Text.Trim();
                        string xml = client.DownloadString("http://" + ip + "/wv/getProfiles");
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xml);
                        XmlNodeList list = xmlDoc.SelectNodes("//profile");
                        string[] oldFiles = Directory.GetFiles("backend\\profiles\\");
                        foreach (string oldFile in oldFiles)
                            File.Delete(oldFile);
                        foreach (XmlNode node in list)
                        {
                            XmlAttribute attr = node.Attributes[0];
                            byte[] tmp = Convert.FromBase64String(node.InnerText);
                            File.WriteAllText(attr.Value, Encoding.Unicode.GetString(tmp));
                        }
                        Profiles.Refresh();
                        RefreshProfiles();
                        if (Profiles.profiles.Count != 0)
                            comboBox1.SelectedIndex = 0;
                        MessageBox.Show("Done");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading profiles: \n" + ex.Message);
                }
            }
        }
    }
}
