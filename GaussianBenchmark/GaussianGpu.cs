using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace GaussianBenchmark
{
    public class GaussianGpu : IDisposable
    {
        private const int threadsPerBlock = 16;

        private GPGPU gpu;
        private float[] kernel;
        
        public GaussianGpu(int size)
        {
            this.gpu = CudafyHost.GetDevice(eGPUType.Cuda);

            var cudafyModule = default(CudafyModule);

            // If the CudafyModule has been embedded in the assembly, load it from there
            // Otherwise, try deserialising the .cdfy file based on the assembly name

            if (CudafyModule.HasCudafyModuleInAssembly())
            {
                cudafyModule = typeof(Program).Assembly.GetCudafyModule();
            }
            else 
            {
                cudafyModule = CudafyModule.TryDeserialize(typeof(Program).Assembly.GetName().Name);
            }

            // At this point, the CudafyModule should be loaded if the cdfy file has been embedded
            // in the assembly, or if it was deployed together with the .NET assembly.
            // If it isn't, or if the checksums aren't the same, re-generate it (this would
            // require the CUDA Toolkit to be installed on the PC).

            if (cudafyModule == null || !cudafyModule.TryVerifyChecksums())
            {
                cudafyModule = CudafyTranslator.Cudafy(ePlatform.Auto, this.gpu.GetArchitecture());
            }

            this.gpu.LoadModule(cudafyModule);

            // Create the Gaussian kernel for the given size
            this.kernel = GaussianKernel.Create(size);
        }

        public void Apply(Image target, Image source)
        {
            // Allocate memory on the GPU
            uint[,] devIntermediate = this.gpu.Allocate<uint>(target.Pixels);
            uint[,] devTarget = this.gpu.Allocate<uint>(target.Pixels);

            // Copy the source image and kernel to the GPU
            uint[,] devSource = this.gpu.CopyToDevice(source.Pixels);
            float[] devKernel = this.gpu.CopyToDevice(this.kernel);

            // Calculate the blocks per grid from the image width
            var blocksPerGrid = (source.Width + threadsPerBlock - 1) / threadsPerBlock;

            // Do the convolution in the X dimension
            this.gpu
                .Launch(threadsPerBlock, blocksPerGrid)
                .ConvolveX(devSource, devKernel, devIntermediate);

            // Calculate the blocks per grid from the image height
            blocksPerGrid = (source.Height + threadsPerBlock - 1) / threadsPerBlock;

            // Do the convolution in the Y dimension
            this.gpu
                .Launch(threadsPerBlock, blocksPerGrid)
                .ConvolveY(devIntermediate, devKernel, devTarget);

            // Copy the target matrix back from the GPU to the CPU
            this.gpu.CopyFromDevice(devTarget, target.Pixels);

            // Free the memory allocated on the GPU
            this.gpu.FreeAll();
        }

        [Cudafy]
        public static void ConvolveX(GThread thread, uint[,] source, float[] kernel, uint[,] target)
        {
            // Get the unique index of the thread: size of the block * block index + thread index
            int x = thread.blockDim.x * thread.blockIdx.x + thread.threadIdx.x;
            int kernelSize = kernel.Length;
            int radius = kernelSize >> 1;
            var width = source.GetLength(0);
            var height = source.GetLength(1);
            int maxX = width - 1;
            int maxY = height - 1;

            if (x < width)
            {
                for (var y = 0; y < height; y++)
                {
                    float red = 0;
                    float green = 0;
                    float blue = 0;
                    float alpha = 0;

                    // Apply each matrix multiplier to the color components for each pixel
                    for (int k = 0; k < kernelSize; k++)
                    {
                        int offsetX = x + k - radius;

                        if (offsetX < 0)
                            offsetX = 0;
                        if (offsetX > maxX)
                            offsetX = maxX;

                        var currentPixel = source[offsetX, y];

                        red += (kernel[k] * ((currentPixel >> 16) & 0xff));
                        green += (kernel[k] * ((currentPixel >> 8) & 0xff));
                        blue += (kernel[k] * (currentPixel & 0xff));
                        alpha += (kernel[k] * ((currentPixel >> 24) & 0xff));
                    }

                    target[x, y] = (((uint)blue) << 24) | (((uint)green) << 16) | (((uint)red) << 8) | ((uint)alpha);
                }
            }
        }

        [Cudafy]
        public static void ConvolveY(GThread thread, uint[,] source, float[] kernel, uint[,] target)
        {
            // Get the unique index of the thread: size of the block * block index + thread index
            int y = thread.blockDim.x * thread.blockIdx.x + thread.threadIdx.x;
            int kernelSize = kernel.Length;
            int radius = kernelSize >> 1;
            var width = source.GetLength(0);
            var height = source.GetLength(1);
            int maxX = width - 1;
            int maxY = height - 1;

            if (y < height)
            {
                for (var x = 0; x < width; x++)
                {
                    float red = 0;
                    float green = 0;
                    float blue = 0;
                    float alpha = 0;

                    // Apply each matrix multiplier to the color components for each pixel
                    for (int k = 0; k < kernelSize; k++)
                    {
                        int offsetY = y + k - radius;

                        if (offsetY < 0)
                            offsetY = 0;
                        if (offsetY > maxY)
                            offsetY = maxY;
                        
                        var currentPixel = source[x, offsetY];

                        red += (kernel[k] * ((currentPixel >> 16) & 0xff));
                        green += (kernel[k] * ((currentPixel >> 8) & 0xff));
                        blue += (kernel[k] * (currentPixel & 0xff));
                        alpha += (kernel[k] * ((currentPixel >> 24) & 0xff));
                    }

                    target[x, y] = (((uint)blue) << 24) | (((uint)green) << 16) | (((uint)red) << 8) | ((uint)alpha);
                }
            }
        }

        public void Dispose()
        {
            // It is managed, and will be disposed when it falls out of scope, but this is neater.
            if (this.gpu != null)
            {
                this.gpu.FreeAll();
                this.gpu.Dispose();
            }
        }
    }
}
