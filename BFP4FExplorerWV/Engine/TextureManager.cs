using System.Collections.Generic;
using System.IO;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using Device = SharpDX.Direct3D11.Device;

namespace BFP4FExplorerWV
{
    public class TextureManager
    {
        public Engine3D engine = null;
        public Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        public Texture2D FindTextureByPath(string path)
        {
            if (loadedTextures.ContainsKey(path))
                return loadedTextures[path];
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath(path.Replace("/", "\\"));
            if (e == null)
                return null;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return null;
            string ext = Path.GetExtension(e.inFSPath).ToLower();
            string tmpfile = "tmp" + ext;
            string tmpfile2 = "tmp.png";
            if (File.Exists(tmpfile2))
                File.Delete(tmpfile2);
            File.WriteAllBytes(tmpfile, data);
            Texture2D result = null;
            switch (ext)
            {
                case ".dds":
                    Helper.ConvertToPNG("tmp.dds");
                    if (File.Exists(tmpfile2))
                    {
                        System.Drawing.Bitmap bmp = Helper.LoadBitmapUnlocked(tmpfile2);
                        result = CreateTexture2DFromBitmap(engine.device, CreateWICBitmapFromGDI(bmp));
                        bmp.Dispose();
                        File.Delete(tmpfile2);
                    }
                    break;
            }
            File.Delete(tmpfile);
            if (result != null)
            {
                Log.WriteLine("[BF2 TM] Loaded texture " + path);
                loadedTextures.Add(path, result);
            }
            return result;
        }

        public void ClearCache()
        {
            loadedTextures.Clear();
        }

        public BitmapSource LoadBitmap(ImagingFactory2 factory, string filename)
        {
            var bitmapDecoder = new BitmapDecoder(
                factory,
                filename,
                DecodeOptions.CacheOnDemand
                );

            var formatConverter = new FormatConverter(factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            return formatConverter;
        }
        public Texture2D CreateTexture2DFromBitmap(Device device, BitmapSource bitmapSource)
        {
            int stride = bitmapSource.Size.Width * 4;
            using (var buffer = new DataStream(bitmapSource.Size.Height * stride, true, true))
            {
                bitmapSource.CopyPixels(stride, buffer);
                return new Texture2D(device, new Texture2DDescription()
                {
                    Width = bitmapSource.Size.Width,
                    Height = bitmapSource.Size.Height,
                    ArraySize = 1,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(buffer.DataPointer, stride));
            }
        }

        public unsafe SharpDX.WIC.Bitmap CreateWICBitmapFromGDI(System.Drawing.Bitmap gdiBitmap)
        {
            var wicFactory = new ImagingFactory();
            var wicBitmap = new SharpDX.WIC.Bitmap(
                wicFactory, gdiBitmap.Width, gdiBitmap.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGRA,
                BitmapCreateCacheOption.CacheOnLoad);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                0, 0, gdiBitmap.Width, gdiBitmap.Height);
            var btmpData = gdiBitmap.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte* pGDIData = (byte*)btmpData.Scan0;
            using (BitmapLock bl = wicBitmap.Lock(BitmapLockFlags.Write))
            {
                byte* pWICData = (byte*)bl.Data.DataPointer;
                for (int y = 0; y < gdiBitmap.Height; y++)
                {
                    int offsetWIC = y * bl.Stride;
                    int offsetGDI = y * btmpData.Stride;
                    for (int x = 0; x < gdiBitmap.Width; x++)
                    {
                        pWICData[offsetWIC + 0] = pGDIData[offsetGDI + 0];
                        pWICData[offsetWIC + 1] = pGDIData[offsetGDI + 1];
                        pWICData[offsetWIC + 2] = pGDIData[offsetGDI + 2];
                        pWICData[offsetWIC + 3] = pGDIData[offsetGDI + 3];
                        offsetWIC += 4;
                        offsetGDI += 4;
                    }
                }
            }
            gdiBitmap.UnlockBits(btmpData);
            return wicBitmap;
        }
    }
}
