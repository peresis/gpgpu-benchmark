using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace GaussianBenchmark
{
    public class Image
    {
        public uint[,] Pixels { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public Image(int width, int height)
        {
            this.Pixels = new uint[width, height];
            this.Width = width;
            this.Height = height;
        }

        public Image(Bitmap source)
        {
            this.Width = source.Width;
            this.Height = source.Height;
            this.Pixels = new uint[this.Width, this.Height];

            unsafe
            {
                var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                var p = (byte*)sourceData.Scan0.ToPointer();
                var buffer = new byte[4];

                for (int i = 0; i < this.Height; i++)
                {
                    for (int j = 0; j < this.Width; j++)
                    {
                        buffer[0] = p[0];
                        buffer[1] = p[1];
                        buffer[2] = p[2];
                        buffer[3] = p[3];

                        this.Pixels[j, i] = BitConverter.ToUInt32(buffer, 0);

                        p += 4;
                    }
                }

                source.UnlockBits(sourceData);
            }
        }

        public Bitmap ToBitmap()
        {
            var bitmap = new Bitmap(this.Width, this.Height);

            unsafe
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, this.Width, this.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                byte* p = (byte*)bitmapData.Scan0.ToPointer();

                for (int i = 0; i < this.Height; i++)
                {
                    for (int j = 0; j < this.Width; j++)
                    {
                        var pixel = this.Pixels[j, i];

                        p[0] = pixel.GetNthByte(0);
                        p[1] = pixel.GetNthByte(1);
                        p[2] = pixel.GetNthByte(2);
                        p[3] = pixel.GetNthByte(3);

                        p += 4;
                    }
                }

                bitmap.UnlockBits(bitmapData);
            }

            return bitmap;
        }
    }
}
