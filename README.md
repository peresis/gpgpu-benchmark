# gpgpu-benchmark
Gaussian blur benchmark on CPU vs GPU for NGC presentation.

Some notes about getting this to run:

* You'll need to have the NVIDIA CUDA Toolkit 7.0 installed - CUDAfy doesn't currently work with v7.5 (https://developer.nvidia.com/cuda-toolkit-70). Or, presumably the OpenCL toolkit from Intel or AMD depending on the chipset in your graphics card, but I've only tested it with CUDA. If you want to use OpenCL you'll need to edit the eGPUType input into CudafyHost.GetDevice in the constructor of the GaussianGpu class.

* Before running the project, check the project settings. There are 2, one for a working folder (the output images will be written here), and a link to the source image that it will be using to blur. The larger the image you blur the more processing work is required, and the more the GPU will tend to outperform the CPU.
