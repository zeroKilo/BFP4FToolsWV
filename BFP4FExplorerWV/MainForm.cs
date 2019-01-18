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
        private bool meshMouseUp = true;
        private bool levelMouseUp = true;
        private Point meshLastMousePos = new Point(0, 0);
        private Point levelLastMousePos = new Point(0, 0);
        private Engine3D engineMeshExplorer;
        private Engine3D engineLevelExplorer;
        private bool isLoading = false;
        private bool allowEdit = true;

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
            engineMeshExplorer = new Engine3D(pic2);
            engineLevelExplorer = new Engine3D(pic3);
            BF2Level.engine = engineLevelExplorer;
            engineLevelExplorer.renderLevel = true;
            renderTimerMeshes.Enabled = true;
            renderTimerLevel.Enabled = true;
            init = true;
        }

        private void RefreshTrees()
        {
            tv1.Nodes.Clear();
            tv1.Nodes.Add(BF2FileSystem.MakeFSTree());
            tv2.Nodes.Clear();
            tv2.Nodes.Add(BF2FileSystem.MakeFSTreeFiltered(new string[] { ".staticmesh", ".bundledmesh", ".skinnedmesh", ".collisionmesh" }));
            listBox1.Items.Clear();
            foreach (string objname in BF2Level.MakeList())
                listBox1.Items.Add(objname);
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
            engineMeshExplorer.Render();
        }

        private void renderTimerLevel_Tick(object sender, EventArgs e)
        {
            engineLevelExplorer.Render();
        }

        private void pic2_SizeChanged(object sender, EventArgs e)
        {
            if (engineMeshExplorer != null)
                engineMeshExplorer.Resize(pic2);
        }

        private void mountLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LevelSelect ls = new LevelSelect();
            ls.basepath = BF2FileSystem.basepath + "Levels\\";
            ls.ShowDialog();
            if (ls._exitOK)
            {
                mountLevelToolStripMenuItem.Enabled = false;
                isLoading = true;
                consoleBox.Text = "";
                BF2FileSystem.Load();
                BF2FileSystem.LoadLevel(ls.result);
                BF2Level.engine = engineLevelExplorer;                
                BF2Level.Load();
                Log.WriteLine("Done. Loaded " + (BF2FileSystem.clientFS.Count() + BF2FileSystem.serverFS.Count()) + " files");
                RefreshTrees();
                isLoading = false;
                saveChangesToolStripMenuItem.Enabled =
                mountLevelToolStripMenuItem.Enabled = true;
            }
        }

        private void tv2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            engineMeshExplorer.ClearScene();
            byte[] data = BF2FileSystem.GetFileFromNode(tv2.SelectedNode);
            if (data == null)
                return;
            string ending = Path.GetExtension(BF2FileSystem.GetPathFromNode(tv2.SelectedNode)).ToLower();
            switch (ending)
            {
                case ".staticmesh":
                    BF2StaticMesh stm = new BF2StaticMesh(data);
                    engineMeshExplorer.objects.AddRange(stm.ConvertForEngine(engineMeshExplorer, toolStripButton3.Checked));
                    break;
                case ".bundledmesh":
                    BF2BundledMesh bm = new BF2BundledMesh(data);
                    engineMeshExplorer.objects.AddRange(bm.ConvertForEngine(engineMeshExplorer, toolStripButton3.Checked));
                    break;
                case ".skinnedmesh":
                    BF2SkinnedMesh skm = new BF2SkinnedMesh(data);
                    engineMeshExplorer.objects.AddRange(skm.ConvertForEngine(engineMeshExplorer, toolStripButton3.Checked));
                    break;
                case ".collisionmesh":
                    BF2CollisionMesh cm = new BF2CollisionMesh(data);
                    engineMeshExplorer.objects.AddRange(cm.ConvertForEngine(engineMeshExplorer));
                    break;
                default:
                    RenderObject o = new RenderObject(engineMeshExplorer.device, RenderObject.RenderType.TriListWired, engineMeshExplorer.defaultTexture, engineMeshExplorer);
                    o.InitGeometry();
                    engineMeshExplorer.objects.Add(o);
                    break;
            }
            engineMeshExplorer.ResetCameraDistance();
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

        private void pic2_MouseDown(object sender, MouseEventArgs e)
        {
            meshMouseUp = false;
            meshLastMousePos = e.Location;
        }

        private void pic2_MouseMove(object sender, MouseEventArgs e)
        {
            if (!meshMouseUp)
            {
                int dx = e.X - meshLastMousePos.X;
                int dy = e.Y - meshLastMousePos.Y;
                engineMeshExplorer.CamDis *= 1 + (dy * 0.01f);
                engineMeshExplorer.CamRot += dx * 0.01f;
                meshLastMousePos = e.Location;
            }
        }

        private void pic2_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
                meshMouseUp = true;
        }

        private int NormRot(float r)
        {
            int t = (int)((r * 10) % 3600);
            if (t < 0) t += 3600;
            return t;
        }

        private void pic3_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (!isLoading && e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                int idx = BF2Level.Process3DClick(e.X, e.Y);
                if (idx != -1)
                    listBox1.SelectedIndex = idx;
            }
        }

        private void pic3_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                levelMouseUp = false;
                levelLastMousePos = e.Location;
            }
        }

        private void pic3_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (!levelMouseUp)
            {
                int dx = e.X - levelLastMousePos.X;
                int dy = e.Y - levelLastMousePos.Y;
                engineLevelExplorer.CamHeight = ((e.Y / (float)pic3.Height) * 2f - 1f) * 5;
                engineLevelExplorer.CamRot += dx * 0.01f;
                levelLastMousePos = e.Location;
            }
        }

        private void pic3_MouseUp_1(object sender, MouseEventArgs e)
        {
            levelMouseUp = true;
        }

        private void pic3_SizeChanged_1(object sender, EventArgs e)
        {
            if (engineLevelExplorer != null)
                engineLevelExplorer.Resize(pic3);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage3 && pic3.Focused)
            {
                SharpDX.Vector3 camPosRel = new SharpDX.Vector3((float)Math.Sin(engineLevelExplorer.CamRot) * engineLevelExplorer.CamDis, engineLevelExplorer.CamHeight, (float)Math.Cos(engineLevelExplorer.CamRot) * engineLevelExplorer.CamDis);
                SharpDX.Vector3 camPosAbs = engineLevelExplorer.CamPos + camPosRel;
                SharpDX.Vector3 dir = -camPosRel;
                SharpDX.Vector3 side = SharpDX.Vector3.Cross(dir, SharpDX.Vector3.UnitY);
                dir.Normalize();
                side.Normalize();
                if (e.KeyCode == Keys.W)
                    engineLevelExplorer.CamPos += dir;
                if (e.KeyCode == Keys.S)
                    engineLevelExplorer.CamPos -= dir;
                if (e.KeyCode == Keys.A)
                    engineLevelExplorer.CamPos += side;
                if (e.KeyCode == Keys.D)
                    engineLevelExplorer.CamPos -= side;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || !allowEdit)
                return;
            BF2LevelObject lo = BF2Level.objects[n];
            lo.rotation.X = trackBar1.Value / 10f;
            lo.RefreshTransform();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || !allowEdit)
                return;
            BF2LevelObject lo = BF2Level.objects[n];
            lo.rotation.Y = trackBar2.Value / 10f;
            lo.RefreshTransform();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || !allowEdit)
                return;
            BF2LevelObject lo = BF2Level.objects[n];
            lo.rotation.Z = trackBar3.Value / 10f;
            lo.RefreshTransform();
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            BF2Level.SelectIndex(n);
            allowEdit = false;
            BF2LevelObject lo = BF2Level.objects[n];
            textBox1.Text = lo.position.X.ToString();
            textBox2.Text = lo.position.Y.ToString();
            textBox3.Text = lo.position.Z.ToString();
            trackBar1.Value = NormRot(lo.rotation.X);
            trackBar2.Value = NormRot(lo.rotation.Y);
            trackBar3.Value = NormRot(lo.rotation.Z);
            StringBuilder sb = new StringBuilder();
            foreach (string s in lo.properties)
                sb.AppendLine(s);
            rtbProps.Text = sb.ToString();
            allowEdit = true;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (e.KeyChar == 13 && allowEdit && n != -1)
            {
                try
                {
                    BF2LevelObject lo = BF2Level.objects[n];
                    lo.position.X = Convert.ToSingle(textBox1.Text);
                    lo.RefreshTransform();
                }
                catch { };
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (e.KeyChar == 13 && allowEdit && n != -1)
            {
                try
                {
                    BF2LevelObject lo = BF2Level.objects[n];
                    lo.position.Y = Convert.ToSingle(textBox2.Text);
                    lo.RefreshTransform();
                }
                catch { };
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (e.KeyChar == 13 && allowEdit && n != -1)
            {
                try
                {
                    BF2LevelObject lo = BF2Level.objects[n];
                    lo.position.Z = Convert.ToSingle(textBox3.Text);
                    lo.RefreshTransform();
                }
                catch { };
            }
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BF2Level.Save();
            MessageBox.Show("Done.");
        }
    }
}
