using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FExplorerWV
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //OpenFileDialog d = new OpenFileDialog();
            //d.Filter = "*.collisionmesh|*.collisionmesh";
            //if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    byte[] data = File.ReadAllBytes(d.FileName);
            //    BF2CollisionMesh skm = new BF2CollisionMesh(data);
            //}

            //return;

            if (!File.Exists("config.txt"))
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ModLoader ml = new ModLoader();
                    ml.basePath = fbd.SelectedPath + "\\";
                    File.WriteAllText("config.txt", ml.basePath);
                    Application.Run(ml);
                }
            }
            else
            {
                ModLoader ml = new ModLoader();
                ml.basePath = File.ReadAllText("config.txt").Trim();
                Application.Run(ml);
            }
        }
    }
}
