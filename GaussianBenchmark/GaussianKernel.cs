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
    public static class GaussianKernel
    {
        public static float[] Create(int size)
        {
            float[] kernel = new float[size];
            float sum = 0.0f;

            float midpoint = (size - 1) / 2f;
            for (int i = 0; i < size; i++)
            {
                float x = i - midpoint;
                float gx = Gaussian(x);
                sum += gx;
                kernel[i] = gx;
            }

            // Normalise kernel so that the sum of all weights equals 1
            for (int i = 0; i < size; i++)
            {
                kernel[i] = kernel[i] / sum;
            }

            return kernel;
        }

        /// <summary>
        /// Implementation of 1D Gaussian G(x) function
        /// </summary>
        /// <param name="x">The x provided to G(x)</param>
        /// <returns>The Gaussian G(x)</returns>
        private static float Gaussian(float x)
        {
            const float Numerator = 1.0f;
            float deviation = 3; // this.sigma;
            float denominator = (float)(Math.Sqrt(2 * Math.PI) * deviation);

            float exponentNumerator = -x * x;
            float exponentDenominator = (float)(2 * Math.Pow(deviation, 2));

            float left = Numerator / denominator;
            float right = (float)Math.Exp(exponentNumerator / exponentDenominator);

            return left * right;
        }
    }
}
