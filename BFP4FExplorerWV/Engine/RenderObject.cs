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
        public struct Vertex
        {
            public Vertex(Vector4 position, Color4 color, Vector2 textureUV)
            {
                Position = position;
                Color = color;
                TextureUV = textureUV;
            }

            public Vector4 Position;
            public Color4 Color;
            public Vector2 TextureUV;
        }

        public Device device;
        public Engine3D engine;
        public RenderType type;
        public RawVector3[] vertices = new RawVector3[] { new RawVector3(-0.5f, 0.5f, 0.0f),
                                                          new RawVector3(0.5f, 0.5f, 0.0f),
                                                          new RawVector3(0.0f, -0.5f, 0.0f) };
        public Vertex[] verticesTextured = new Vertex[] { new Vertex(new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color4.White, new Vector2(0, 0)),
                                                          new Vertex(new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color4.White, new Vector2(0.5f, 1f)),
                                                          new Vertex(new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color4.White, new Vector2(1, 0))};
        public Buffer triangleVertexBufferWired;
        public Buffer triangleVertexBufferTextured;
        public Texture2D texture;
        public ShaderResourceView textureView;
        public Matrix transform = Matrix.Identity;

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
            if (vertices != null)
                triangleVertexBufferWired = Buffer.Create<RawVector3>(device, BindFlags.VertexBuffer, vertices);
            if (verticesTextured != null)
                triangleVertexBufferTextured = Buffer.Create<Vertex>(device, BindFlags.VertexBuffer, verticesTextured);
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
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferWired, Utilities.SizeOf<RawVector3>(), 0));
                    context.Draw(vertices.Count(), 0);
                    break;
                case RenderType.TriListWired:
                    context.Rasterizer.State = engine.rasterStateWired;
                    context.VertexShader.Set(engine.vsWired);
                    context.PixelShader.Set(engine.psWired);
                    context.InputAssembler.InputLayout = engine.inputLayoutWired;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferWired, Utilities.SizeOf<RawVector3>(), 0));
                    context.Draw(vertices.Count(), 0);
                    break;
                case RenderType.TriListTextured:
                    context.Rasterizer.State = engine.rasterStateTextured;
                    context.VertexShader.Set(engine.vsTextured);
                    context.InputAssembler.InputLayout = engine.inputLayoutTextured;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferTextured, Utilities.SizeOf<Vertex>(), 0));
                    context.PixelShader.Set(engine.psTextured);
                    context.PixelShader.SetShaderResource(0, textureView);
                    context.PixelShader.SetSampler(0, engine.sampler);
                    context.Draw(verticesTextured.Count(), 0);
                    break;
            }
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
