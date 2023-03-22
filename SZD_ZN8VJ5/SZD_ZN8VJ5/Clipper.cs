
namespace SZD_ZN8VJ5
{
    public static class Clipper
    {
        private enum ClipMode
        {
            Constant,
            Multiply,
            Divide,
            Fit
        }

        private static ClipMode Mode;
        private static int Width;
        private static int Height;

        public static void Instantiate(ARC_File task)
        {
            Height = task.train[0].output.Length;
            Width = task.train[0].output[0].Length;

            for (int i = 1; i < task.train.Length; i++)
            {
                if (!(Height == task.train[i].output.Length && Width == task.train[0].output[0].Length))
                {
                    Height = -1; Width = -1;
                }
            }

            if (Height != -1)
            {
                Mode = ClipMode.Constant;
                return;
            }

            if (task.train[0].output.Length >= task.train[0].input.Length &&
                task.train[0].output[0].Length >= task.train[0].input[0].Length)
            {
                // 20 / 10 = 2
                Height = task.train[0].output.Length / task.train[0].input.Length;
                Width = task.train[0].output[0].Length / task.train[0].input[0].Length;

                for (int i = 0; i < task.train.Length; i++)
                {
                    if (!(task.train[0].output.Length == task.train[0].input.Length * Height
                        && task.train[0].output[0].Length == task.train[0].input[0].Length * Width))
                    {
                        Height = -1;
                        break;
                    }
                }

                if (Height != -1)
                {
                    Mode = ClipMode.Multiply;
                    return;
                }
            }

            if (task.train[0].output.Length < task.train[0].input.Length &&
                task.train[0].output[0].Length < task.train[0].input[0].Length)
            {
                //  20 / 2 = 10
                Height = task.train[0].input.Length / task.train[0].output.Length;
                Width = task.train[0].input[0].Length / task.train[0].output[0].Length;

                for (int i = 0; i < task.train.Length; i++)
                {
                    if (!(task.train[0].output.Length == task.train[0].input.Length / (float)Height
                        && task.train[0].output[0].Length == task.train[0].input[0].Length / (float)Width))
                    {
                        Height = -1;
                        break;
                    }
                }

                if (Height != -1)
                {
                    Mode = ClipMode.Divide;
                    return;
                }
            }        

            Mode = ClipMode.Fit;
        }

        public static int[][] Clip(int[][] G, int[][] input)
        {
            switch (Mode)
            {
                case ClipMode.Constant: return ClipConstant(G);
                case ClipMode.Multiply: return ClipMultiply(G, input);
                case ClipMode.Divide: return ClipDivide(G, input);
                case ClipMode.Fit: return ClipToFit(G);
                default:
                    return null;
            }
        }

        public static int[][] ClipConstant(int[][] G)
        {
            int[][] C = new int[Height][];
            for (int i = 0; i < C.Length; i++)
            {
                C[i] = new int[Width];

                for (int j = 0; j < Width; j++)
                {
                    C[i][j] = G[i][j];
                }
            }

            return C;
        }

        public static int[][] ClipMultiply(int[][] G, int[][] input)
        {
            Height *= input.Length;
            Width *= input[0].Length;
            return ClipConstant(G);
        }

        public static int[][] ClipDivide(int[][] G, int[][] input)
        {
            Height /= input.Length;
            Width /= input[0].Length;
            return ClipConstant(G);
        }

        public static int[][] ClipToFit(int[][] G)
        {
            int minV = 0;
            while (G[minV].Max() == 0)
            {
                ++minV;
            }

            int maxV = G.Length - 1;
            while (G[maxV].Max() == 0)
            {
                --maxV;
            }

            int minH = 0;
            while (MaxHorizontal(G, minH) == 0)
            {
                minH++;
            }

            int maxH = G[0].Length - 1;
            while (MaxHorizontal(G, maxH) == 0)
            {
                --maxH;
            }

            int[][] Gnew = new int[maxV - minV + 1][];
            for (int i = 0; i < Gnew.Length; i++)
            {
                Gnew[i] = new int[maxH - minH + 1];
            }

            for (int i = 0; i < Gnew.Length; i++)
            {
                for (int j = 0; j < Gnew[i].Length; j++)
                {
                    Gnew[i][j] = G[i + minV][j + minH];
                }
            }

            return Gnew;
        }

        private static int MaxHorizontal(int[][] G, int v)
        {
            int max = 0;

            for (int i = 0; i < G.Length; i++)
            {
                if (G[i][v] > max)
                {
                    max = G[i][v];
                }
            }

            return max;
        }
    }
}
