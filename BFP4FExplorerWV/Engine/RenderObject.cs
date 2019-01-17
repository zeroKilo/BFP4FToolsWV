using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

using Device = SharpDX.Direct3D11.Device;

namespace BFP4FExplorerWV
{

    public class RenderObject
    {
        public enum RenderType
        {
            LineList,
            TriListWired,
            TriListTextured,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexTextured
        {
            public VertexTextured(Vector4 position, Color4 color, Vector2 textureUV)
            {
                Position = position;
                Color = color;
                TextureUV = textureUV;
            }

            public Vector4 Position;
            public Color4 Color;
            public Vector2 TextureUV;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexWired
        {
            public VertexWired(Vector4 position, Color4 color)
            {
                Position = position;
                Color = color;
            }

            public VertexWired(Vector3 position, Color4 color)
            {
                Position = new Vector4(position.X, position.Y, position.Z, 1f);
                Color = color;
            }

            public Vector4 Position;
            public Color4 Color;
        }

        public Device device;
        public Engine3D engine;
        public RenderType type;
        public VertexWired[] verticesWired = new VertexWired[] { new VertexWired(new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color4.Black),
                                                            new VertexWired(new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color4.Black),
                                                            new VertexWired(new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color4.Black) };
                                                          
        public VertexTextured[] verticesTextured = new VertexTextured[] { new VertexTextured(new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color4.White, new Vector2(0, 0)),
                                                          new VertexTextured(new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color4.White, new Vector2(0.5f, 1f)),
                                                          new VertexTextured(new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color4.White, new Vector2(1, 0))};
        public Buffer triangleVertexBufferWired;
        public Buffer triangleVertexBufferTextured;
        public Texture2D texture;
        public ShaderResourceView textureView;
        public Matrix transform = Matrix.Identity;
        public bool Selected = false;
        public BoundingSphere bsphere = new BoundingSphere();

        public RenderObject(Device d, RenderType t, Texture2D tex, Engine3D e)
        {
            device = d;
            type = t;
            engine = e;
            texture = tex;
            if (tex != null)
                textureView = new ShaderResourceView(device, tex);
        }

        public void InitGeometry()
        {
            if (verticesWired != null)
                triangleVertexBufferWired = Buffer.Create<VertexWired>(device, BindFlags.VertexBuffer, verticesWired);
            if (verticesTextured != null)
                triangleVertexBufferTextured = Buffer.Create<VertexTextured>(device, BindFlags.VertexBuffer, verticesTextured);
            bsphere = CalcBoundingSphere();
        }

        public void Render(DeviceContext context, Matrix view, Matrix proj)
        {
            Matrix world = transform * view * proj;
            world = Matrix.Transpose(world);
            context.VertexShader.SetConstantBuffer(0, engine.worldViewProjectionBuffer);
            context.UpdateSubresource(ref world, engine.worldViewProjectionBuffer);
            switch (type)
            {
                case RenderType.LineList:
                    context.Rasterizer.State = engine.rasterStateWired;
                    context.VertexShader.Set(engine.vsWired);
                    context.PixelShader.Set(engine.psWired);
                    context.InputAssembler.InputLayout = engine.inputLayoutWired;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferWired, Utilities.SizeOf<VertexWired>(), 0));
                    context.Draw(verticesWired.Count(), 0);
                    break;
                case RenderType.TriListWired:
                    context.Rasterizer.State = engine.rasterStateWired;
                    context.VertexShader.Set(engine.vsWired);
                    context.PixelShader.Set(engine.psWired);
                    context.InputAssembler.InputLayout = engine.inputLayoutWired;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferWired, Utilities.SizeOf<VertexWired>(), 0));
                    context.Draw(verticesWired.Count(), 0);
                    break;
                case RenderType.TriListTextured:
                    context.Rasterizer.State = engine.rasterStateTextured;
                    context.VertexShader.Set(engine.vsTextured);
                    context.InputAssembler.InputLayout = engine.inputLayoutTextured;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferTextured, Utilities.SizeOf<VertexTextured>(), 0));
                    context.PixelShader.Set(engine.psTextured);
                    context.PixelShader.SetShaderResource(0, textureView);
                    context.PixelShader.SetSampler(0, engine.sampler);
                    context.Draw(verticesTextured.Count(), 0);
                    break;
            }
        }

        public BoundingSphere CalcBoundingSphere()
        {
            BoundingSphere s = new BoundingSphere();
            float f = 0;
            foreach (VertexTextured v in verticesTextured)
            {
                float t = (v.Position.X * v.Position.X) +
                          (v.Position.Y * v.Position.Y) +
                          (v.Position.Z * v.Position.Z);
                if (t > f)
                    f = t;
            }
            foreach (VertexWired v in verticesWired)
            {
                float t = (v.Position.X * v.Position.X) +
                          (v.Position.Y * v.Position.Y) +
                          (v.Position.Z * v.Position.Z);
                if (t > f)
                    f = t;
            }
            s.Center = Vector3.Zero;
            s.Radius = (float)System.Math.Sqrt(f);
            return s;
        }

        public bool CheckRayHit(Ray ray, out float dist)
        {
            bool result = false;
            dist = 100000;
            float d = 0;
            if(verticesWired != null)
                for (int i = 0; i < verticesWired.Length / 3; i++)
                {
                    Vector3 v1 = DropW(Vector3.Transform(DropW(verticesWired[i * 3].Position), transform));
                    Vector3 v2 = DropW(Vector3.Transform(DropW(verticesWired[i * 3 + 1].Position), transform));
                    Vector3 v3 = DropW(Vector3.Transform(DropW(verticesWired[i * 3 + 2].Position), transform));
                    if (Collision.RayIntersectsTriangle(ref ray, ref v1, ref v2, ref v3, out d))
                        if (d < dist)
                        {
                            dist = d;
                            result = true;
                        }
                }
            if(verticesTextured != null)
                for (int i = 0; i < verticesTextured.Length / 3; i++)
                {
                    Vector3 v1 = DropW(Vector3.Transform(DropW(verticesTextured[i * 3].Position), transform));
                    Vector3 v2 = DropW(Vector3.Transform(DropW(verticesTextured[i * 3 + 1].Position), transform));
                    Vector3 v3 = DropW(Vector3.Transform(DropW(verticesTextured[i * 3 + 2].Position), transform));
                    if (Collision.RayIntersectsTriangle(ref ray, ref v1, ref v2, ref v3, out d))
                        if (d < dist)
                        {
                            dist = d;
                            result = true;
                        }
                }
            return result;
        }

        private Vector3 DropW(Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public void Dispose()
        {
            if (triangleVertexBufferWired != null)
                triangleVertexBufferWired.Dispose();
            if (triangleVertexBufferTextured != null)
                triangleVertexBufferTextured.Dispose();
        }
    }
}
