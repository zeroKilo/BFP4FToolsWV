using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;

namespace BFP4FExplorerWV
{

    public class BF2LevelObject
    {
        public enum BF2LOTYPE
        {
            StaticObject = 0,
            Road = 1,
        }
        public bool _valid = false;
        public string _template;
        public string _name;
        public byte[] _data;
        public Vector3 position;
        public Vector3 rotation;
        public Matrix transform;
        public BF2LOTYPE type;
        public List<RenderObject> staticMeshes = null;
        public List<RenderObject> meshes = null;
        public List<string> properties;
        public BF2LevelObject(Vector3 pos, Vector3 rot, BF2LOTYPE t)
        {
            position = pos;
            rotation = rot;
            type = t;
            RefreshTransform();
        }

        public void RefreshTransform()
        {
            float f = 3.1415f / 180f;
            transform = Matrix.RotationYawPitchRoll(rotation.X * f, rotation.Y * f, rotation.Z * f) * Matrix.Translation(position);
            if (staticMeshes != null)
                foreach (RenderObject ro in staticMeshes)
                    ro.transform = transform;
            if (meshes != null)
                foreach (RenderObject ro in meshes)
                    ro.transform = transform;
        }

        public void Render(DeviceContext context, Matrix view, Matrix proj)
        {
            if (staticMeshes != null)
                foreach (RenderObject ro in staticMeshes)
                    ro.Render(context, view, proj);
            if (meshes != null)
                foreach (RenderObject ro in meshes)
                    ro.Render(context, view, proj);
        }

        public TreeNode MakeNode()
        {
            TreeNode t = new TreeNode(_template);
            t.Nodes.Add(new TreeNode("Position = " + position.ToString()));
            t.Nodes.Add(new TreeNode("Rotation= " + position.ToString()));
            return t;
        }

        public void SetSelected(bool sel)
        {
            if (staticMeshes != null)
                foreach (RenderObject ro in staticMeshes)
                {
                    if (ro.Selected != sel)
                    {
                        ro.Selected = sel;
                        if (ro.verticesWired != null)
                        {
                            for (int i = 0; i < ro.verticesWired.Length; i++)
                                if (sel)
                                    ro.verticesWired[i].Color = new Color4(1, 0.6f, 0, 1);
                                else
                                    ro.verticesWired[i].Color = Color4.Black;
                            ro.InitGeometry();
                        }
                    }
                }
            if (meshes != null)
                foreach (RenderObject ro in meshes)
                {
                    if (ro.Selected != sel)
                    {
                        ro.Selected = sel;
                        if (ro.verticesWired != null)
                        {
                            for (int i = 0; i < ro.verticesWired.Length; i++)
                                if (sel)
                                    ro.verticesWired[i].Color = new Color4(1, 0.6f, 0, 1);
                                else
                                    ro.verticesWired[i].Color = Color4.Black;
                            ro.InitGeometry();
                        }
                    }
                }
        }

        public BoundingSphere CalcBoundingSphere()
        {
            BoundingSphere s = new BoundingSphere();
            float f = 0;
            List<BoundingSphere> spheres = new List<BoundingSphere>();
            if (staticMeshes != null)
                foreach (RenderObject ro in staticMeshes)
                    spheres.Add(ro.bsphere);
            if (meshes != null)
                foreach (RenderObject ro in meshes)
                    spheres.Add(ro.bsphere);
            foreach (BoundingSphere s2 in spheres)
                if (s2.Radius > f)
                    f = s2.Radius;
            s.Radius = f;
            return s;
        }

        public bool CheckRayHit(Ray ray, out float dist)
        {
            bool result = false;
            dist = 100000;
            float d = 0;
            if (staticMeshes != null)
                foreach (RenderObject ro in staticMeshes)
                    if (ro.CheckRayHit(ray, out d))
                        if (d < dist)
                        {
                            dist = d;
                            result = true;
                        }
            if (meshes != null)
                foreach (RenderObject ro in meshes)
                    if (ro.CheckRayHit(ray, out d))
                        if (d < dist)
                        {
                            dist = d;
                            result = true;
                        }
            return result;
        }

        public void Free()
        {
            staticMeshes.Clear();
            staticMeshes = null;
            meshes.Clear();
            meshes = null;
        }
    }
}
