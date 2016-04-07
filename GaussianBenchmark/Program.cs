using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace GaussianBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var iterations = 10;
            var gaussianSizeStart = 5;
            var gaussianSizeEnd = 35;
            var results = new List<Result>();
            var timespanCpu = default(TimeSpan);
            var timespanGpu = default(TimeSpan);
            var sourceImage = default(Image);

            // Get an image
            using (var bitmap = new Bitmap(Properties.Settings.Default.ImagePath))
            {
                sourceImage = new Image(bitmap);
            }
            
            for (var gaussianSize = gaussianSizeStart; gaussianSize <= gaussianSizeEnd; gaussianSize += 2)
            {
                Console.WriteLine(string.Format("Commencing calculations for gaussian size {0}", gaussianSize));

                {
                    // CPU gaussian blurs
                    // Get the bitmap into an image

                    var targetImage = new Image(sourceImage.Width, sourceImage.Height);

                    var cpu = new GaussianCpu(gaussianSize);
                    var startTime = DateTime.Now;

                    for (var i = 0; i < iterations; i++)
                    {
                        cpu.Apply(targetImage, sourceImage);
                        sourceImage = targetImage;

                        if (i == 0)
                        {
                            Console.Write(string.Format("CPU gaussian size {0} iteration 01", gaussianSize));
                        }
                        else
                        {
                            Console.Write("\b\b");
                            Console.Write((i + 1).ToString().PadLeft(2, '0'));
                        }
                    }

                    timespanCpu = DateTime.Now - startTime;

                    Console.WriteLine();

                    using (var resultBitmap = targetImage.ToBitmap())
                    {
                        resultBitmap.Save(string.Format(@"{0}output_cpu_{1}.jpeg", Properties.Settings.Default.OutputFolder, gaussianSize.ToString().PadLeft(2, '0')));
                    }
                }

                {
                    // GPU gaussian blurs
                    // Get the bitmap into an image
                    var targetImage = new Image(sourceImage.Width, sourceImage.Height);

                    using (var gpu = new GaussianGpu(gaussianSize))
                    {
                        var startTime = DateTime.Now;

                        for (var i = 0; i < iterations; i++)
                        {
                            gpu.Apply(targetImage, sourceImage);
                            sourceImage = targetImage;

                            if (i == 0)
                            {
                                Console.Write(string.Format("GPU gaussian size {0} iteration 01", gaussianSize));
                            }
                            else
                            {
                                Console.Write("\b\b");
                                Console.Write((i + 1).ToString().PadLeft(2, '0'));
                            }
                        }

                        timespanGpu = DateTime.Now - startTime;
                    }

                    Console.WriteLine();

                    using (var resultBitmap = targetImage.ToBitmap())
                    {
                        resultBitmap.Save(string.Format(@"{0}output_gpu_{1}.jpeg", Properties.Settings.Default.OutputFolder, gaussianSize.ToString().PadLeft(2, '0')));
                    }
                }

                results.Add(new Result(gaussianSize, (int)timespanCpu.TotalMilliseconds, (int)timespanGpu.TotalMilliseconds));

                Console.WriteLine(string.Format("Gaussian size {0} total milliseconds CPU: {1:n0}", gaussianSize, timespanCpu.TotalMilliseconds));
                Console.WriteLine(string.Format("Gaussian size {0} total milliseconds GPU: {1:n0}", gaussianSize, timespanGpu.TotalMilliseconds));
                Console.WriteLine(string.Format("Gaussian size {0}, GPU is {1:n1} times faster", gaussianSize, timespanCpu.TotalMilliseconds / timespanGpu.TotalMilliseconds));
                Console.WriteLine("------------------------------------------");
            }


            Console.WriteLine("------------------------------------------");
            foreach (var result in results)
            {
                Console.WriteLine(string.Format("Size {0}\t{1}\t{2}\t{3:n1}", result.GaussianSize, result.TotalMillisecondsCpu, result.TotalMillisecondsGpu, (decimal)result.TotalMillisecondsCpu / result.TotalMillisecondsGpu));
            }

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
        
        private class Result
        {
            public int GaussianSize { get; set; }
            public int TotalMillisecondsCpu { get; set; }
            public int TotalMillisecondsGpu { get; set; }

            public Result(int gaussianSize, int totalMillisecondsCpu, int totalMillisecondsGpu)
            {
                this.GaussianSize = gaussianSize;
                this.TotalMillisecondsCpu = totalMillisecondsCpu;
                this.TotalMillisecondsGpu = totalMillisecondsGpu;
            }
        }
    }
}
