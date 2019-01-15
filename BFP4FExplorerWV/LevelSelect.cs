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
    public partial class LevelSelect : Form
    {
        public string basepath;
        public bool _exitOK = false;
        public string result;

        public LevelSelect()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectLevel();
        }

        private void LevelSelect_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string[] dirs = Directory.GetDirectories(basepath);
            foreach (string dir in dirs)
                listBox1.Items.Add(dir.Substring(basepath.Length));
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SelectLevel();
        }

        public void SelectLevel()
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            result = listBox1.Items[n].ToString();
            _exitOK = true;
            this.Close();
        }
    }
}
