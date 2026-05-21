using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Views.Controls;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

// 抑制 CA2022 警告
#pragma warning disable CA2022

namespace ModernFormatConverter.Extensions.Others
{
    /// <summary>
    /// Provides some extension methods for WriteableBitmap
    /// </summary>
    public static class WriteableBitmapExtensions
    {
        public static async Task<WriteableBitmap> GetCroppedImageAsync(this WriteableBitmap writeableBitmap, Rect croppedRect)
        {
            if (writeableBitmap is null)
            {
                return null;
            }
            WriteableBitmap croppedBitmap = new((int)Math.Floor(croppedRect.Width), (int)Math.Floor(croppedRect.Height));
            using (InMemoryRandomAccessStream randomAccessStream = new())
            {
                BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, randomAccessStream);
                await CropImageAsync(writeableBitmap, croppedRect, bitmapEncoder);
                croppedBitmap.SetSource(randomAccessStream);
            }
            return croppedBitmap;
        }

        public static async Task CropImageAsync(WriteableBitmap writeableBitmap, Rect croppedRect, BitmapEncoder bitmapEncoder)
        {
            croppedRect.X = croppedRect.X > 0 ? croppedRect.X : 0;
            croppedRect.Y = croppedRect.Y > 0 ? croppedRect.Y : 0;
            uint x = (uint)Math.Floor(croppedRect.X);
            uint y = (uint)Math.Floor(croppedRect.Y);
            uint width = (uint)Math.Floor(croppedRect.Width);
            uint height = (uint)Math.Floor(croppedRect.Height);
            using Stream sourceStream = writeableBitmap.PixelBuffer.AsStream();
            byte[] buffer = new byte[sourceStream.Length];
            await sourceStream.ReadAsync(buffer, 0, buffer.Length);
            bitmapEncoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)writeableBitmap.PixelWidth, (uint)writeableBitmap.PixelHeight, 96.0, 96.0, buffer);
            bitmapEncoder.BitmapTransform.Bounds = new BitmapBounds
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
            await bitmapEncoder.FlushAsync();
        }

        public static Guid GetEncoderId(BitmapFileFormat bitmapFileFormat)
        {
            return bitmapFileFormat switch
            {
                BitmapFileFormat.Bmp => BitmapEncoder.BmpEncoderId,
                BitmapFileFormat.Png => BitmapEncoder.PngEncoderId,
                BitmapFileFormat.Jpeg => BitmapEncoder.JpegEncoderId,
                BitmapFileFormat.Tiff => BitmapEncoder.TiffEncoderId,
                BitmapFileFormat.Gif => BitmapEncoder.GifEncoderId,
                BitmapFileFormat.JpegXR => BitmapEncoder.JpegXREncoderId,
                _ => BitmapEncoder.PngEncoderId,
            };
        }
    }
}
