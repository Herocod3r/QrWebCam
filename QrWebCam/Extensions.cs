using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace QrWebCam
{
    public static class Extensions
    {
        public static BitmapImage Convert(this Bitmap src)
        {
            var image = new BitmapImage();
            using (var ms = new MemoryStream())
            {
                src.Save(ms, ImageFormat.Bmp);

                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }

            return image;
        }
    }
}