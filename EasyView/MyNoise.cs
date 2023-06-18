using System;

namespace EasyView
{
    // the idea with this class is to have a predictable random values in front and back directions
    internal class MyNoise
    {
        private interface IIndexable
        {
            float GetAtFloor(float cursor);
        }

        private interface IInterpolator
        {
            void SetIndexable(IIndexable array);
            float Interpolate(float _x);
        }

        private class LinearInterpolation : IInterpolator
        {
            private IIndexable indexable;
            public void SetIndexable(IIndexable indexable)
            {
                this.indexable = indexable;
            }
            public float Interpolate(float _x)
            {
                float x0 = (float)Math.Floor(_x);
                float x1 = x0 + 1.0f;

                float y0 = indexable.GetAtFloor(x0);
                float y1 = indexable.GetAtFloor(x1);

                return y0 + (y1 - y0) * (_x - x0);
            }
        }

        [Obsolete("I don't remember the origin of this algorithm.")]
        private class CubicInterpolation : IInterpolator
        {
            private IIndexable indexable;
            public void SetIndexable(IIndexable indexable)
            {
                this.indexable = indexable;
            }

            public float Interpolate(float _x)
            {
                const int n = 4;

                float[] x = new float[n];
                x[0] = (float)Math.Floor(_x);
                x[1] = x[0] - 1.0f;
                x[2] = x[0] + 1.0f;
                x[3] = x[0] + 2.0f;

                float[] y = new float[n];
                for (int i = 0; i < n; ++i)
                    y[i] = indexable.GetAtFloor(x[i]);

                // Allocate matrix
                float[][] mx = new float[][] { new float[n + 1], new float[n + 1], new float[n + 1], new float[n + 1] };

                // Initialize matrix
                for (int j = 0; j < n; j++)
                {
                    mx[j][0] = 1.0f;
                    for (int i = 1; i < n; ++i)
                        mx[j][i] = mx[j][i - 1] * x[j];
                    mx[j][n] = y[j];
                }

                // Triangularize matrix
                for (int j = 0; j < n - 1; ++j)
                {
                    for (int k = j + 1; k < n; ++k)
                    {
                        float factor = mx[k][j] / mx[j][j];
                        for (int i = 0; i < n + 1; ++i)
                        {
                            mx[k][i] -= factor * mx[j][i];
                        }
                    }
                }

                float a = mx[3][4] / mx[3][3];
                float b = (mx[2][4] - a * mx[2][3]) / mx[2][2];
                float c = (mx[1][4] - a * mx[1][3] - b * mx[1][2]) / mx[1][1];
                float d = (mx[0][4] - a * mx[0][3] - b * mx[0][2] - c * mx[0][1]) / mx[0][0];

                return ((a * _x + b) * _x + c) * _x + d;
            }
        }

        private class RandomValueCyclicBuffer<T> : IIndexable where T : IInterpolator, new()
        {
            // size of value buffer
            private const int Granularity = 65536;
            private readonly float[] array = new float[Granularity];
            private readonly T interpolator;
            public RandomValueCyclicBuffer(int seed)
            {
                Random random = new Random(seed);
                for (int i = 0; i < Granularity; ++i)
                    array[i] = (float)(random.NextDouble() - 0.5); // between -0.5(inclusive) and 0.5(exclusive)
                interpolator = new T();
                interpolator.SetIndexable(this);
            }

            public float GetAtFloor(float cursor)
            {
                int index = (int)(cursor - Math.Floor(cursor / Granularity) * Granularity);
                return array[index];
            }
            public float GetAt(float cursor) => interpolator.Interpolate(cursor);
        }

        private RandomValueCyclicBuffer<LinearInterpolation> buffer;
        private const float Resolution = 17.4569f;
        private const int LayerCount = 5;
        private const float epsilon = 1.0f / (1 << LayerCount);
        private int CurrentIndex = 0;

        public MyNoise(int seed)
        {
            buffer = new RandomValueCyclicBuffer<LinearInterpolation>(seed);
        }

        private float Fractal()
        {
            float t = CurrentIndex * Resolution;
            float f = 0.0f;
            float m = 1.0f;

            for (int i = 0; i < LayerCount; ++i)
            {
                f += buffer.GetAt(t * m) / m;
                m += m;
            }
            // f is between -(1-epsilon) and (1-epsilon)

            f /= (1.0f - epsilon);

            f += 0.5f;
            if (f > 1.0f)
                f = 2.0f - f;
            if (f < 0.0f)
                f = -f;
            // now f is between 0 and 1, triangle shape signal from [-1, 1] to [0, 1]

            return f;
        }

        public int Next(int MaxValue)
        {
            CurrentIndex++;
            return (int)(MaxValue * Fractal());
        }

        public int Previous(int MaxValue)
        {
            CurrentIndex--;
            return (int)(MaxValue * Fractal());
        }
    }
}
