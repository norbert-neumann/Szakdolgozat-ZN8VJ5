
namespace SZD_ZN8VJ5
{
    public enum ClipMode
    {
        Constant,
        Multiply,
        Divide,
        Fit
    }

    public static class Postprocessing
    {
        public static ClipMode Mode;
        public static int Width;
        public static int Height;

        public static void SetClipMode(ARC_File task)
        {
            Height = task.train[0].output.Length;
            Width = task.train[0].output[0].Length;

            for (int i = 1; i < task.train.Length; i++)
            {
                if (!(Height == task.train[i].output.Length && Width == task.train[i].output[0].Length))
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

                for (int i = 1; i < task.train.Length; i++)
                {
                    if (!(task.train[i].output.Length == task.train[i].input.Length / (float)Height
                        && task.train[i].output[0].Length == task.train[i].input[0].Length / (float)Width))
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

        private static int[][] ClipConstant(int[][] G)
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

        private static int[][] ClipMultiply(int[][] G, int[][] input)
        {
            Height *= input.Length;
            Width *= input[0].Length;
            return ClipConstant(G);
        }

        private static int[][] ClipDivide(int[][] G, int[][] input)
        {
            Height /= input.Length;
            Width /= input[0].Length;
            return ClipConstant(G);
        }

        private static int[][] ClipToFit(int[][] G)
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

        public static int[][] Render(List<ARCObject> objects)
        {
            int[][] canvas = new int[30][];
            for (int i = 0; i < 30; i++)
            {
                canvas[i] = new int[30];
            }

            foreach (var obj in objects)
            {
                int[,] visual = obj.FullVisual();
                for (int i = 0; i < 30; i++)
                {
                    for (int j = 0; j < 30; j++)
                    {
                        if (visual[i, j] != -1)
                        {
                            canvas[i][j] = visual[i, j];
                        }
                    }
                }

                foreach (var noise in obj.Noises)
                {
                    int[,] noiseVisual = noise.FullVisual();
                    for (int i = 0; i < 30; i++)
                    {
                        for (int j = 0; j < 30; j++)
                        {
                            if (noiseVisual[i, j] != -1)
                            {
                                canvas[i][j] = noiseVisual[i, j];
                            }
                        }
                    }
                }
            }

            return canvas;
        }

        public static void Display(int[][] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < arr[0].Length; j++)
                {
                    Console.BackgroundColor = GetConsoleColor(arr[i][j]);
                    Console.Write("  ");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static void Display(int[,] arr)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    Console.BackgroundColor = GetConsoleColor(arr[i, j]);
                    Console.Write("  ");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
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

        private static ConsoleColor GetConsoleColor(int color)
        {
            switch (color)
            {
                case 0:
                    return ConsoleColor.Black;
                case 1:
                    return ConsoleColor.DarkBlue;
                case 2:
                    return ConsoleColor.Red;
                case 3:
                    return ConsoleColor.Green;
                case 4:
                    return ConsoleColor.Yellow;
                case 5:
                    return ConsoleColor.Gray;
                case 6:
                    return ConsoleColor.Magenta;
                case 7:
                    return ConsoleColor.DarkYellow;
                case 8:
                    return ConsoleColor.Blue;
                case 9:
                    return ConsoleColor.DarkRed;
                default:
                    return ConsoleColor.White;
            }
        }

        public static void ToArcFile(string taskName, List<ARCProgram> programs, List<Predicate>[] classifiers)
        {
            string result = String.Empty;

            foreach (var (program, classifier) in programs.Zip(classifiers))
            {
                result += "\n" + "condition: " + string.Join<Predicate>(", ", classifier) +
                    "\n" + program.ToFileString() + "\n";
            }

            File.WriteAllText(taskName + ".arc", result);
        }
    }
}
