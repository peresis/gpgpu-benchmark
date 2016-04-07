using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GaussianBenchmark
{
    public class GaussianCpu
    {
        private float[] kernel;

        public GaussianCpu(int size)
        {
            // Create the Gaussian kernel for the given size
            this.kernel = GaussianKernel.Create(size);
        }

        public void Apply(Image target, Image source)
        {
            var intermediateImage = new Image(source.Width, source.Height);

            this.ConvolveX(intermediateImage, source);

            this.ConvolveY(target, intermediateImage);
        }

        private void ConvolveX(Image target, Image source)
        {
            int kernelSize = this.kernel.Length;
            int radius = kernelSize >> 1;
            int maxX = source.Width - 1;
            int maxY = source.Height - 1;

            Parallel.For(
                0,
                source.Width,
                x =>
                {
                    for (int y = 0; y <= maxY; y++)
                    {
                        float red = 0;
                        float green = 0;
                        float blue = 0;
                        float alpha = 0;

                        // Apply each matrix multiplier to the color components for each pixel
                        for (int k = 0; k < kernelSize; k++)
                        {
                            int offsetX = x + k - radius;

                            offsetX = offsetX.Clamp(0, maxX);

                            var currentPixel = source.Pixels[offsetX, y];

                            red += (this.kernel[k] * currentPixel.GetNthByte(2));
                            green += (this.kernel[k] * currentPixel.GetNthByte(1));
                            blue += (this.kernel[k] * currentPixel.GetNthByte(0));
                            alpha += (this.kernel[k] * currentPixel.GetNthByte(3));
                        }

                        target.Pixels[x, y] = BitConverter.ToUInt32(new byte[] { (byte)blue, (byte)green, (byte)red, (byte)alpha }, 0);
                    }
                });
        }

        private void ConvolveY(Image target, Image source)
        {
            int kernelSize = this.kernel.Length;
            int radius = kernelSize >> 1;
            int maxX = source.Width - 1;
            int maxY = source.Height - 1;

            Parallel.For(
                0,
                source.Height,
                y =>
                {
                    for (int x = 0; x <= maxX; x++)
                    {
                        float red = 0;
                        float green = 0;
                        float blue = 0;
                        float alpha = 0;

                        // Apply each matrix multiplier to the color components for each pixel
                        for (int k = 0; k < kernelSize; k++)
                        {
                            int offsetY = y + k - radius;

                            offsetY = offsetY.Clamp(0, maxY);

                            var currentPixel = source.Pixels[x, offsetY];

                            red += (this.kernel[k] * currentPixel.GetNthByte(2));
                            green += (this.kernel[k] * currentPixel.GetNthByte(1));
                            blue += (this.kernel[k] * currentPixel.GetNthByte(0));
                            alpha += (this.kernel[k] * currentPixel.GetNthByte(3));
                        }

                        target.Pixels[x, y] = BitConverter.ToUInt32(new byte[] { (byte)blue, (byte)green, (byte)red, (byte)alpha }, 0);
                    }
                });
        }
    }
}
