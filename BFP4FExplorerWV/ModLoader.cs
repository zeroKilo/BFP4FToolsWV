using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FExplorerWV
{
    public partial class ModLoader : Form
    {
        public string basePath;

        public ModLoader()
        {
            InitializeComponent();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            string[] mods = Directory.GetDirectories(basePath + "mods\\");
            listBox1.Items.Clear();
            foreach (string mod in mods)
                if (File.Exists(mod + "\\Mod.desc") &&
                    File.Exists(mod + "\\ServerArchives.con") &&
                    File.Exists(mod + "\\ClientArchives.con"))
                    listBox1.Items.Add(mod.Substring(basePath.Length));
            if (listBox1.Items.Count != 0)
                listBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadIt();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LoadIt();
        }

        private void LoadIt()
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            MainForm main = new MainForm();
            BF2FileSystem.basepath = basePath + listBox1.Items[n] + "\\";
            this.Hide();
            main.ShowDialog();
            this.Close();
        }
    }
}
