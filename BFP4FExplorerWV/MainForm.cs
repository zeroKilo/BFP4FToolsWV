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
using Be.Windows.Forms;

namespace BFP4FExplorerWV
{
    public partial class MainForm : Form
    {
        private bool init = false;
        private Engine3D engine;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Log.box = consoleBox;
            Log.pb = pb1;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (init)
                return;
            BF2FileSystem.Load();
            Log.WriteLine("Done. Loaded " + (BF2FileSystem.clientFS.Count() + BF2FileSystem.serverFS.Count()) + " files");
            RefreshTrees();
            engine = new Engine3D(pic2);
            renderTimer.Enabled = true;
            init = true;
        }

        private void RefreshTrees()
        {
            tv1.Nodes.Clear();
            tv1.Nodes.Add(BF2FileSystem.MakeFSTree());
            tv2.Nodes.Clear();
            tv2.Nodes.Add(BF2FileSystem.MakeFSTreeFiltered(new string[] { ".staticmesh", ".bundledmesh", ".skinnedmesh", ".collisionmesh" }));
        }

        private void tv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            byte[] data = BF2FileSystem.GetFileFromNode(tv1.SelectedNode);
            if (data == null)
                return;
            string ending = Path.GetExtension(BF2FileSystem.GetPathFromNode(tv1.SelectedNode)).ToLower();
            rtb1.Visible =
            hb1.Visible =
            pic1.Visible = false;
            switch (ending)
            {
                case ".inc":
                case ".xml":
                case ".txt":
                case ".con":
                case ".tweak":
                    rtb1.Visible = true;
                    rtb1.Text = Encoding.ASCII.GetString(data);
                    break;
                case ".png":
                    pic1.Visible = true;
                    pic1.Image = new Bitmap(new MemoryStream(data));
                    break;
                case ".dds":
                    File.WriteAllBytes("temp.dds", data);
                    Helper.DDS2PNG("temp.dds");
                    pic1.Visible = true;
                    if (File.Exists("temp.png"))
                    {
                        pic1.Image = new Bitmap(new MemoryStream(File.ReadAllBytes("temp.png")));
                        File.Delete("temp.png");
                    }
                    else
                    {
                        pic1.Image = null;
                        pic1.Height = pic1.Width = 1;
                    }
                    File.Delete("temp.dds");
                    break;
                default:
                    hb1.Visible = true;
                    hb1.ByteProvider = new DynamicByteProvider(data);
                    break;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (tv1.SelectedNode == null)
            {
                e.Cancel = true;
                return;
            }
            byte[] data = BF2FileSystem.GetFileFromNode(tv1.SelectedNode);
            if (data == null)
            {
                e.Cancel = true;
                return;
            }
            string ending = Path.GetExtension(BF2FileSystem.GetPathFromNode(tv1.SelectedNode)).ToLower();
            switch (ending)
            {
                default:
                    
                    break;
            }
        }

        private void renderTimer_Tick(object sender, EventArgs e)
        {
            engine.Render();
            engine.CamRot += 0.01f;
        }

        private void pic2_SizeChanged(object sender, EventArgs e)
        {
            if (engine != null)
                engine.Resize(pic2);
        }

        private void mountLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LevelSelect ls = new LevelSelect();
            ls.basepath = BF2FileSystem.basepath + "Levels\\";
            ls.ShowDialog();
            if (ls._exitOK)
            {
                consoleBox.Text = "";
                BF2FileSystem.Load();
                BF2FileSystem.LoadLevel(ls.result);
                Log.WriteLine("Done. Loaded " + (BF2FileSystem.clientFS.Count() + BF2FileSystem.serverFS.Count()) + " files");
                RefreshTrees();
            }
        }

        private void tv2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            engine.ClearScene();
            byte[] data = BF2FileSystem.GetFileFromNode(tv2.SelectedNode);
            if (data == null)
                return;
            string ending = Path.GetExtension(BF2FileSystem.GetPathFromNode(tv2.SelectedNode)).ToLower();
            switch (ending)
            {
                case ".staticmesh":
                    BF2StaticMesh stm = new BF2StaticMesh(data);
                    engine.objects.AddRange(stm.ConvertForEngine(engine));
                    break;
                case ".bundledmesh":
                    BF2BundledMesh bm = new BF2BundledMesh(data);
                    engine.objects.AddRange(bm.ConvertForEngine(engine));
                    break;
                case ".skinnedmesh":
                    BF2SkinnedMesh skm = new BF2SkinnedMesh(data);
                    engine.objects.AddRange(skm.ConvertForEngine(engine));
                    break;
                case ".collisionmesh":
                    BF2CollisionMesh cm = new BF2CollisionMesh(data);
                    engine.objects.AddRange(cm.ConvertForEngine(engine));
                    break;
                default:
                    RenderObject o = new RenderObject(engine.device, RenderObject.RenderType.TriListWired, engine.defaultTexture, engine);
                    o.InitGeometry();
                    engine.objects.Add(o);
                    break;
            }
            engine.ResetCameraDistance();
        }

        private void tv1_DoubleClick(object sender, EventArgs e)
        {
            if (tv1.SelectedNode == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromNode(tv1.SelectedNode);
            if (data == null)
                return;
            string ending = Path.GetExtension(BF2FileSystem.GetPathFromNode(tv1.SelectedNode)).ToLower();
            switch (ending)
            {
                case ".inc":
                case ".xml":
                case ".txt":
                case ".con":
                case ".tweak":
                    TextEditor te = new TextEditor();
                    te.rtb1.Text = Encoding.ASCII.GetString(data);
                    te.ShowDialog();
                    if(te._exitOk)
                        BF2FileSystem.SetFileFromNode(tv1.SelectedNode, Encoding.ASCII.GetBytes(te.rtb1.Text));
                    break;
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = Path.GetFileName(BF2FileSystem.GetPathFromNode(tv1.SelectedNode));
            string ext = Path.GetExtension(name);
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = name;
            dlg.Filter = "*" + ext + "|*" + ext;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BF2FileSystem.SetFileFromNode(tv1.SelectedNode, File.ReadAllBytes(dlg.FileName));
                Log.WriteLine(dlg.FileName + " imported.");
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] data = BF2FileSystem.GetFileFromNode(tv1.SelectedNode);
            string name = Path.GetFileName(BF2FileSystem.GetPathFromNode(tv1.SelectedNode));
            string ext = Path.GetExtension(name);
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = name;
            dlg.Filter = "*" + ext + "|*" + ext;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(dlg.FileName, data);
                Log.WriteLine(dlg.FileName + " exported.");
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Helper.SelectNext(toolStripTextBox1.Text, tv1);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Helper.SelectNext(toolStripTextBox2.Text, tv2);
        }

        private void contextMenuMeshes_Opening(object sender, CancelEventArgs e)
        {
            if (tv2.SelectedNode == null)
            {
                e.Cancel = true;
                return;
            }
            byte[] data = BF2FileSystem.GetFileFromNode(tv2.SelectedNode);
            if (data == null)
            {
                e.Cancel = true;
                return;
            }
            string ending = Path.GetExtension(BF2FileSystem.GetPathFromNode(tv2.SelectedNode)).ToLower();
            switch (ending)
            {
                case ".staticmesh":
                case ".skinnedmesh":
                case ".bundledmesh":
                case ".collisionmesh":
                    break;
                default:
                    e.Cancel = true;
                    return;
            }
        }

        private void exportAsObjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] data = BF2FileSystem.GetFileFromNode(tv2.SelectedNode);
            string path = BF2FileSystem.GetPathFromNode(tv2.SelectedNode);
            string name = Path.GetFileNameWithoutExtension(path) + ".obj";
            string ext = Path.GetExtension(path);
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = name;
            dlg.Filter = "*.obj|*.obj";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (ext)
                {
                    case ".staticmesh":
                        ExporterObj.Export(new BF2StaticMesh(data), dlg.FileName);
                        break;
                    case ".bundledmesh":
                        ExporterObj.Export(new BF2BundledMesh(data), dlg.FileName);
                        break;
                    case ".skinnedmesh":
                        ExporterObj.Export(new BF2SkinnedMesh(data), dlg.FileName);
                        break;
                    case ".collisionmesh":
                        ExporterObj.Export(new BF2CollisionMesh(data), dlg.FileName);
                        break;
                }
                Log.WriteLine(dlg.FileName + " exported.");
            }
        }
    }
}
