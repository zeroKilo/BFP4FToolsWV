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
            this.Text += " - Build " + Resources.Resource1.BuildDate;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (init)
                return;
            BF2FileSystem.Load();
            BF2HUDLoader.Init();
            Log.WriteLine("Done. Loaded " + (BF2FileSystem.clientFS.Count() + BF2FileSystem.serverFS.Count()) + " files");
            RefreshTrees();
            engineMeshExplorer = new Engine3D(pic2);
            engineLevelExplorer = new Engine3D(pic3);
            BF2Level.engine = engineLevelExplorer;
            engineLevelExplorer.renderLevel = true;
            renderTimerMeshes.Enabled = true;
            renderTimerLevel.Enabled = true;
            toolStripComboBox1.SelectedIndex = 0;
            init = true;
        }

        private void RefreshTrees()
        {
            tv1.Nodes.Clear();
            tv1.Nodes.Add(BF2FileSystem.MakeFSTree());
            tv2.Nodes.Clear();
            tv2.Nodes.Add(BF2FileSystem.MakeFSTreeFiltered(new string[] { ".staticmesh", ".bundledmesh", ".skinnedmesh", ".collisionmesh" }));
            tv3.Nodes.Clear();
            tv3.Nodes.Add(BF2HUDLoader.MakeTree());
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
                case ".tga":
                    File.WriteAllBytes("temp.tga", data);
                    Helper.ConvertToPNG("temp.tga");
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
                    File.Delete("temp.tga");
                    break;
                case ".dds":
                    File.WriteAllBytes("temp.dds", data);
                    Helper.ConvertToPNG("temp.dds");
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
                BF2Level.name = ls.result;
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
            PickMesh();
        }

        private void PickMesh()
        {
            if (tv2.SelectedNode == null)
                return;
            engineMeshExplorer.ClearScene();
            byte[] data = BF2FileSystem.GetFileFromNode(tv2.SelectedNode);
            if (data == null)
                return;
            string ending = Path.GetExtension(BF2FileSystem.GetPathFromNode(tv2.SelectedNode)).ToLower();
            switch (ending)
            {
                case ".staticmesh":
                    BF2StaticMesh stm = new BF2StaticMesh(data);
                    engineMeshExplorer.objects.AddRange(stm.ConvertForEngine(engineMeshExplorer, toolStripButton3.Checked, toolStripComboBox1.SelectedIndex));
                    break;
                case ".bundledmesh":
                    BF2BundledMesh bm = new BF2BundledMesh(data);
                    engineMeshExplorer.objects.AddRange(bm.ConvertForEngine(engineMeshExplorer, toolStripButton3.Checked, toolStripComboBox1.SelectedIndex));
                    break;
                case ".skinnedmesh":
                    BF2SkinnedMesh skm = new BF2SkinnedMesh(data);
                    engineMeshExplorer.objects.AddRange(skm.ConvertForEngine(engineMeshExplorer, toolStripButton3.Checked, toolStripComboBox1.SelectedIndex));
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
            exportAsObjToolStripMenuItem.Enabled = true;
            if (tv2.SelectedNode == null)
            {
                exportAsObjToolStripMenuItem.Enabled = false;
                return;
            }
            byte[] data = BF2FileSystem.GetFileFromNode(tv2.SelectedNode);
            if (data == null)
            {
                exportAsObjToolStripMenuItem.Enabled = false;
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
                    exportAsObjToolStripMenuItem.Enabled = false;
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
                        ExporterObj.Export(new BF2StaticMesh(data), dlg.FileName, toolStripComboBox1.SelectedIndex);
                        break;
                    case ".bundledmesh":
                        ExporterObj.Export(new BF2BundledMesh(data), dlg.FileName, toolStripComboBox1.SelectedIndex);
                        break;
                    case ".skinnedmesh":
                        ExporterObj.Export(new BF2SkinnedMesh(data), dlg.FileName, toolStripComboBox1.SelectedIndex);
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
            if (lo.type != BF2LevelObject.BF2LOTYPE.StaticObject)
                return;
            lo.rotation.X = trackBar1.Value / 10f;
            lo.RefreshTransform();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || !allowEdit)
                return;
            BF2LevelObject lo = BF2Level.objects[n];
            if (lo.type != BF2LevelObject.BF2LOTYPE.StaticObject)
                return;
            lo.rotation.Y = trackBar2.Value / 10f;
            lo.RefreshTransform();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || !allowEdit)
                return;
            BF2LevelObject lo = BF2Level.objects[n];
            if (lo.type != BF2LevelObject.BF2LOTYPE.StaticObject)
                return;
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
            bool enabled = true;
            if (lo.type == BF2LevelObject.BF2LOTYPE.Road)
                enabled = false;
            trackBar1.Enabled = trackBar2.Enabled = trackBar3.Enabled = enabled;
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

        private void rtbProps_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            StringBuilder sb = new StringBuilder();
            BF2LevelObject lo = BF2Level.objects[n];
            foreach (string s in lo.properties)
                sb.AppendLine(s);
            TextEditor te = new TextEditor();
            te.rtb1.Text = sb.ToString();
            te.ShowDialog();
            if (te._exitOk)
            {
                StringReader sr = new StringReader(te.rtb1.Text);
                List<string> props = new List<string>();
                string line;
                while ((line = sr.ReadLine()) != null)
                    if (line.Trim() != "")
                        props.Add(line);
                lo.properties = props;
                rtbProps.Text = te.rtb1.Text;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            BF2Level.objects.RemoveAt(n);
            RefreshTrees();
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            BF2Level.CloneEntry(n);
            listBox1.Items.Clear();
            RefreshTrees();
        }

        private void convertDFMSWFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.dfm;*swf|*.dfm;*.swf";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Helper.ConvertDFMSWF(d.FileName);
                MessageBox.Show("Done.");
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripComboBox1.Enabled = false;
            PickMesh();
            toolStripComboBox1.Enabled = true;
        }

        private void CheckAndMakeDir(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private void exportALLAsObjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportALLAsObjToolStripMenuItem.Enabled = false;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string basePath = fbd.SelectedPath + "\\";
                pb1.Maximum = BF2FileSystem.clientFS.Count;
                int count = 0;
                foreach (BF2FileSystem.BF2FSEntry entry in BF2FileSystem.clientFS)
                {
                    pb1.Value = count++;
                    try
                    {
                        string ending = Path.GetExtension(entry.inFSPath).ToLower();
                        switch (ending)
                        {
                            case ".staticmesh":
                            case ".bundledmesh":
                            case ".skinnedmesh":
                            case ".collisionmesh":
                                break;
                            default:
                                continue;
                        }
                        string path = basePath + Path.GetDirectoryName(entry.inFSPath);
                        byte[] data = BF2FileSystem.GetFileFromEntry(entry);
                        if (data == null)
                            continue;
                        switch (ending)
                        {
                            case ".staticmesh":
                                CheckAndMakeDir(path);
                                path += "\\" + Path.GetFileNameWithoutExtension(entry.inFSPath);
                                Log.WriteLine("Exporting \"" + path + ".staticmesh\"...");
                                BF2StaticMesh stm = new BF2StaticMesh(data);
                                for (int i = 0; i < stm.geomat.Count; i++)
                                    ExporterObj.Export(stm, path + ".lod" + i + ".obj", i);
                                break;
                            case ".bundledmesh":
                                CheckAndMakeDir(path);
                                path += "\\" + Path.GetFileNameWithoutExtension(entry.inFSPath);
                                Log.WriteLine("Exporting \"" + path + ".bundledmesh\"...");
                                BF2BundledMesh bm = new BF2BundledMesh(data);
                                for (int i = 0; i < bm.geomat.Count; i++)
                                    ExporterObj.Export(bm, path + ".lod" + i + ".obj", i);
                                break;
                            case ".skinnedmesh":
                                CheckAndMakeDir(path);
                                path += "\\" + Path.GetFileNameWithoutExtension(entry.inFSPath);
                                Log.WriteLine("Exporting \"" + path + ".skinnedmesh\"...");
                                BF2SkinnedMesh skm = new BF2SkinnedMesh(data);
                                for (int i = 0; i < skm.geomat.Count; i++)
                                    ExporterObj.Export(skm, path + ".lod" + i + ".obj", i);
                                break;
                            case ".collisionmesh":
                                CheckAndMakeDir(path);
                                path += "\\" + Path.GetFileNameWithoutExtension(entry.inFSPath);
                                Log.WriteLine("Exporting \"" + path + ".bundledmesh\"...");
                                ExporterObj.Export(new BF2CollisionMesh(data), path + ".obj");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine("ERROR: " + ex.Message);
                    }
                    Application.DoEvents();
                }
                pb1.Value = 0;
                exportALLAsObjToolStripMenuItem.Enabled = true;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in BF2HUDManager.parameter)
                sb.AppendLine(line);
            rtb2.Text = sb.ToString();
        }

        private void tv3_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = tv3.SelectedNode;
            if (t == null)
                return;
            BF2HUDElement result = null;
            foreach (BF2HUDElement el in BF2HUDLoader.elements)
                if (el.name == t.Name)
                {
                    result = el;
                    break;
                }
            if (result == null)
                return;
            StringBuilder sb = new StringBuilder();
            foreach (string p in result.parameter)
                sb.AppendLine(p);
            rtb2.Text = sb.ToString();
        }

        private void exportLevelIntoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LevelSelect ls = new LevelSelect();
            ls.basepath = BF2FileSystem.basepath + "Levels\\";
            ls.ShowDialog();
            if (ls._exitOK)
            {
                string source = BF2FileSystem.basepath + "Levels\\" + ls.result + "\\";
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "bf2editor.exe|bf2editor.exe";
                if (d.ShowDialog() == DialogResult.OK)
                {
                    string target = Path.GetDirectoryName(d.FileName) + "\\mods\\bfp4f\\Levels\\" + ls.result + "\\";
                    Log.WriteLine("Exporting Level from \"" + source + "\" to \"" + target + "\"...");
                    Helper.ClearFolder(new DirectoryInfo(target));
                    Directory.CreateDirectory(target + "Editor");
                    Directory.CreateDirectory(target + "Info");
                    if (Directory.Exists(source + "Info"))
                        Helper.CopyFolder(new DirectoryInfo(source + "Info"), new DirectoryInfo(target + "Info"));
                    if (File.Exists(source + "client.zip"))
                    {
                        Log.WriteLine("Unpacking client.zip...");
                        Helper.UnpackZip(source + "client.zip", target);
                    }
                    if (File.Exists(source + "server.zip"))
                    {
                        Log.WriteLine("Unpacking server.zip...");
                        Helper.UnpackZip(source + "server.zip", target);
                    }
                    if (File.Exists(target + "StaticObjects.con"))
                        File.Copy(target + "StaticObjects.con", target + "Editor\\StaticObjects.con");
                    MessageBox.Show("Done.");
                }
            }
        }

        private void importLevelFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BF2Level.name == "")
            {
                MessageBox.Show("Please mount a level first");
                return;
            }
            string target = BF2FileSystem.basepath + "Levels\\" + BF2Level.name + "\\";
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "bf2editor.exe|bf2editor.exe";
            if (d.ShowDialog() == DialogResult.OK)
            {
                string source = Path.GetDirectoryName(d.FileName) + "\\mods\\bfp4f\\Levels\\" + BF2Level.name + "\\";
                if (!Directory.Exists(source))
                {
                    Log.WriteLine("Cant find source folder \"" + source + "\"");
                    return;
                }
                Log.WriteLine("Importing Level from \"" + source + "\" to \"" + target + "\"...");
                string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
                pb1.Maximum = files.Length;
                int count = 0;
                foreach (string file in files)
                {
                    pb1.Value = count++;
                    string shortname = file.Substring(source.Length);
                    BF2FileSystem.BF2FSEntry entry = BF2FileSystem.FindEntryFromIngamePath(shortname);
                    if (entry == null)
                    {
                        Log.WriteLine("Cant find \"" + shortname + "\"");
                        continue;
                    }
                    if (!entry.zipFile.ToLower().Contains("\\levels\\") || file.ToLower().EndsWith("ambientobjects.con"))
                    {
                        Log.WriteLine("Skipping \"" + file + "\"");
                        continue;
                    }
                    byte[] data = File.ReadAllBytes(file);
                    if (file.ToLower().EndsWith("staticobjects.con"))
                    {
                        Log.WriteLine("Processing \"" + shortname + "\"...");
                        data = ProcessStaticObjects(File.ReadAllLines(file));
                    }
                    BF2FileSystem.SetFileFromEntry(entry, data);
                    Log.WriteLine("Importing \"" + shortname + "\" into \"" + Path.GetFileName(entry.zipFile) + "\"");
                }
                Log.WriteLine("Done.");
                pb1.Value = 0;
            }
        }

        private byte[] ProcessStaticObjects(string[] data)
        {
            StringBuilder sb = new StringBuilder();
            bool skipIntro = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("if v_arg1 == BF2Editor"))
                {
                    skipIntro = true;
                    continue;
                }
                if (skipIntro && data[i].StartsWith("endIf"))
                {
                    i++;
                    skipIntro = false;
                    continue;
                }
                if (skipIntro || data[i].StartsWith("Object.layer"))
                    continue;
                sb.AppendLine(data[i]);
            }
            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}
