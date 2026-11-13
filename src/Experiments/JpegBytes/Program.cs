// Encodes test images as JPEG as the SVG export does, through the Skia bitmaps' WriteToStream, and prints for each the
// SHA-256 of the decoded pixels and of each encoding, so that the bytes can be compared between machines. For contrast it
// also encodes with ImageSharp's encoder, as BitmapIO did before. A throwaway experiment.
//   dotnet run -- <image>...
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;

internal static class Program
{
    private static readonly int[] Qualities = { 50, 75, 90, 91, 100 };

    private static int Main(string[] arguments)
    {
        Console.WriteLine("# " + RuntimeInformation.OSDescription + " | " + RuntimeInformation.ProcessArchitecture + " | .NET " + Environment.Version +
                          " | DOTNET_EnableHWIntrinsic=" + (Environment.GetEnvironmentVariable("DOTNET_EnableHWIntrinsic") ?? "") +
                          " | JSIMD_FORCENONE=" + (Environment.GetEnvironmentVariable("JSIMD_FORCENONE") ?? "") +
                          " | SkiaSharp " + typeof(SKBitmap).Assembly.GetName().Version);
        foreach (string path in arguments) {
            string name = Path.GetFileName(path);
            IGraphicsBitmap bitmap;
            using (FileStream stream = File.OpenRead(path))
                bitmap = new SkiaBitmapGraphicsLoader().ReadBitmapFromStream(stream);
            Report(name, "decoded " + bitmap.PixelWidth + "x" + bitmap.PixelHeight, Pixels(bitmap));
            foreach (int quality in Qualities)
                Report(name, "skia-q" + quality, Encode(bitmap, quality));
            using (IGraphicsBitmap part = bitmap.Crop(3, 5, Math.Min(101, bitmap.PixelWidth - 3), Math.Min(67, bitmap.PixelHeight - 5)))
                Report(name, "skia-part-q90", Encode(part, 90));
            Report(name, "imagesharp-q90", EncodeWithImageSharp(bitmap, 90));
            bitmap.Dispose();
        }
        return 0;
    }

    // Print a line: the image, the step, the length and the SHA-256 of the bytes.
    private static void Report(string name, string step, byte[] bytes)
    {
        Console.WriteLine(name + "\t" + step + "\t" + bytes.Length + "\t" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    // The pixels of a bitmap as BGRA with straight alpha, which every platform's Skia gives alike.
    private static byte[] Pixels(IGraphicsBitmap bitmap)
    {
        SKBitmap pixels = ((Skia_Bitmap)bitmap).Bitmap;
        SKImageInfo info = new SKImageInfo(pixels.Width, pixels.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        byte[] buffer = new byte[info.BytesSize];
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try {
            if (!pixels.PeekPixels().ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes))
                throw new InvalidOperationException("cannot read the pixels");
        }
        finally {
            handle.Free();
        }
        return buffer;
    }

    // A bitmap encoded as JPEG through its WriteToStream, as the SVG target encodes it.
    private static byte[] Encode(IGraphicsBitmap bitmap, int quality)
    {
        using (MemoryStream stream = new MemoryStream()) {
            if (!bitmap.WriteToStream(GraphicsBitmapFormat.JPEG, stream, quality))
                throw new InvalidOperationException("cannot encode");
            return stream.ToArray();
        }
    }

    // A bitmap encoded as JPEG by ImageSharp's encoder, as BitmapIO encoded it before.
    private static byte[] EncodeWithImageSharp(IGraphicsBitmap bitmap, int quality)
    {
        using (Image<Bgra32> image = Image.LoadPixelData<Bgra32>(Pixels(bitmap), bitmap.PixelWidth, bitmap.PixelHeight))
        using (MemoryStream stream = new MemoryStream()) {
            image.Save(stream, new JpegEncoder { Quality = quality });
            return stream.ToArray();
        }
    }
}
