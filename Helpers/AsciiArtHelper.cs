using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace WD2ModBundler.Helpers
{
    public static class AsciiArtHelper
    {
        private static readonly char[] _asciiChars = { '@', '#', 'S', '%', '?', '*', '+', ';', ':', ',', '.' };

        public static string ConvertToAsciiFromResource(string resourcePath, int width = 80)
        {
            // Load image as BitmapImage from embedded resource
            Uri uri = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
            StreamResourceInfo sri = Application.GetResourceStream(uri);

            if (sri == null)
                throw new FileNotFoundException("Embedded resource not found: " + resourcePath);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = sri.Stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            int newWidth = width;
            double ratio = (double)bitmap.PixelHeight / bitmap.PixelWidth;
            int newHeight = (int)(newWidth * ratio * 0.5); // adjust for font aspect

            // Create WriteableBitmap to read pixels
            WriteableBitmap wb = new WriteableBitmap(bitmap);
            wb.Freeze();

            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int px = (int)((x / (double)newWidth) * wb.PixelWidth);
                    int py = (int)((y / (double)newHeight) * wb.PixelHeight);

                    var cb = new byte[4];
                    wb.CopyPixels(new Int32Rect(px, py, 1, 1), cb, 4, 0);

                    // Convert pixel to grayscale
                    int gray = (cb[0] + cb[1] + cb[2]) / 3;
                    int index = (gray * (_asciiChars.Length - 1)) / 255;
                    sb.Append(_asciiChars[index]);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}