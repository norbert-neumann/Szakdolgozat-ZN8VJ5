using System.Drawing;

namespace SZD_ZN8VJ5
{
    public static class ImageProcessing
    {
        //public static Dictionary<List<ARCObject>, List<ARCObject>> Explainings = new Dictionary<List<ARCObject>, List<ARCObject>>();

        public static bool MatchPredictions(int[][][] predictions, ARC_Task[] test)
        {
            /*for (int i = 0; i < test.Length; i++)
            {
                if (!predictions[i].SequenceEqual(test[i].output))
                {
                    return false;
                }
            }*/

            for (int i = 0; i < test.Length; i++)
            {
                if (!(predictions[i].Length == test[i].output.Length 
                    && predictions[i][0].Length == test[i].output[0].Length))
                {
                    return false;
                }

                for (int row = 0; row < test[i].output.Length; row++)
                {
                    for (int column = 0; column < test[i].output[row].Length; column++)
                    {
                        if (predictions[i][row][column] != test[i].output[row][column])
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool Visit(Point p, List<Point> visitedPoints, int[][] input)
        {
            int Height = input.Length;
            int Width = input[0].Length;
            if (visitedPoints.Contains(p))
            {
                return false;
            }
            if (p.X >= 0 && p.X < Height && p.Y >= 0 && p.Y < Width)
            {
                return input[p.X][p.Y] > 0; //todo
            }
            return false;
        }

        public static bool Visit(Point p, List<Point> visitedPoints, int[,] input)
        {
            int Height = input.GetLength(0);
            int Width = input.GetLength(1);
            if (visitedPoints.Contains(p))
            {
                return false;
            }
            if (p.X >= 0 && p.X < Height && p.Y >= 0 && p.Y < Width)
            {
                return input[p.X, p.Y] > 0; //todo
            }
            return false;
        }

        public static void FloodFillNonzero(Point p, List<Point> pointsToVisit, List<Point> visitedPoints, int[][] input)
        {
            if (Visit(p, visitedPoints, input))
            {
                visitedPoints.Add(p);
                pointsToVisit.Add(new Point(p.X - 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X - 1, p.Y));
                pointsToVisit.Add(new Point(p.X - 1, p.Y + 1));

                pointsToVisit.Add(new Point(p.X, p.Y - 1));
                pointsToVisit.Add(new Point(p.X, p.Y + 1));

                pointsToVisit.Add(new Point(p.X + 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X + 1, p.Y));
                pointsToVisit.Add(new Point(p.X + 1, p.Y + 1));

                pointsToVisit = pointsToVisit.Distinct().ToList();

            }
            pointsToVisit.Remove(p);
            if (pointsToVisit.Count > 0)
            {
                FloodFillNonzero(pointsToVisit.First(), pointsToVisit, visitedPoints, input);
            }
        }

        public static void FloodFillNonzero(Point p, List<Point> pointsToVisit, List<Point> visitedPoints, int[,] input)
        {
            if (Visit(p, visitedPoints, input))
            {
                visitedPoints.Add(p);
                pointsToVisit.Add(new Point(p.X - 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X - 1, p.Y));
                pointsToVisit.Add(new Point(p.X - 1, p.Y + 1));

                pointsToVisit.Add(new Point(p.X, p.Y - 1));
                pointsToVisit.Add(new Point(p.X, p.Y + 1));

                pointsToVisit.Add(new Point(p.X + 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X + 1, p.Y));
                pointsToVisit.Add(new Point(p.X + 1, p.Y + 1));

                pointsToVisit = pointsToVisit.Distinct().ToList();
            }

            pointsToVisit.Remove(p);
            if (pointsToVisit.Count > 0)
            {
                FloodFillNonzero(pointsToVisit.First(), pointsToVisit, visitedPoints, input);
            }
        }

        public static void FloodFillColor(Point p, List<Point> pointsToVisit, List<Point> visitedPoints, int[][] input,
          int color)
        {
            if (VisitNoise(p, visitedPoints, input, color))
            {
                visitedPoints.Add(p);
                pointsToVisit.Add(new Point(p.X - 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X - 1, p.Y));
                pointsToVisit.Add(new Point(p.X - 1, p.Y + 1));

                pointsToVisit.Add(new Point(p.X, p.Y - 1));
                pointsToVisit.Add(new Point(p.X, p.Y + 1));

                pointsToVisit.Add(new Point(p.X + 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X + 1, p.Y));
                pointsToVisit.Add(new Point(p.X + 1, p.Y + 1));

                pointsToVisit = pointsToVisit.Distinct().ToList();
            }
            pointsToVisit.Remove(p);
            if (pointsToVisit.Count > 0)
            {
                FloodFillColor(pointsToVisit.First(), pointsToVisit, visitedPoints, input, color);
            }
        }

        public static void FloodFillColor(Point p, List<Point> pointsToVisit, List<Point> visitedPoints, int[,] input,
          int color)
        {
            if (VisitNoise(p, visitedPoints, input, color))
            {
                visitedPoints.Add(p);
                pointsToVisit.Add(new Point(p.X - 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X - 1, p.Y));
                pointsToVisit.Add(new Point(p.X - 1, p.Y + 1));

                pointsToVisit.Add(new Point(p.X, p.Y - 1));
                pointsToVisit.Add(new Point(p.X, p.Y + 1));

                pointsToVisit.Add(new Point(p.X + 1, p.Y - 1));
                pointsToVisit.Add(new Point(p.X + 1, p.Y));
                pointsToVisit.Add(new Point(p.X + 1, p.Y + 1));

                pointsToVisit = pointsToVisit.Distinct().ToList();
            }
            pointsToVisit.Remove(p);
            if (pointsToVisit.Count > 0)
            {
                FloodFillColor(pointsToVisit.First(), pointsToVisit, visitedPoints, input, color);
            }
        }

        public static bool VisitNoise(Point p, List<Point> visitedPoints, int[][] input, int color)
        {
            int Height = input.Length;
            int Width = input[0].Length;
            if (visitedPoints.Contains(p))
            {
                return false;
            }
            if (p.X >= 0 && p.X < Height && p.Y >= 0 && p.Y < Width)
            {
                return input[p.X][p.Y] == color; //todo
            }
            return false;
        }

        public static bool VisitNoise(Point p, List<Point> visitedPoints, int[,] input, int color)
        {
            int Height = input.GetLength(0);
            int Width = input.GetLength(1);
            if (visitedPoints.Contains(p))
            {
                return false;
            }
            if (p.X >= 0 && p.X < Height && p.Y >= 0 && p.Y < Width)
            {
                return input[p.X,p.Y] == color; //todo
            }
            return false;
        }

        public static Point FirstNonZero(int[][] G)
        {
            for (int i = 0; i < G.Length; i++)
            {
                for (int j = 0; j < G[0].Length; j++)
                {
                    if (G[i][j] > 0)
                    {
                        return new Point(i, j);
                    }
                }
            }

            return new Point(-1, -1);
        }

        public static Point FirstNonZero(int[,] G)
        {
            for (int i = 0; i < G.GetLength(0); i++)
            {
                for (int j = 0; j < G.GetLength(1); j++)
                {
                    if (G[i, j] > 0)
                    {
                        return new Point(i, j);
                    }
                }
            }

            return new Point(-1, -1);
        }

        public static Point FirstPositive(int[,] G)
        {
            for (int i = 0; i < G.GetLength(0); i++)
            {
                for (int j = 0; j < G.GetLength(1); j++)
                {
                    if (G[i, j] >= 0)
                    {
                        return new Point(i, j);
                    }
                }
            }

            return new Point(-1, -1);
        }

        public static int FirstVerticalPositive(int[,] G)
        {
            for (int x = 0; x < G.GetLength(0); x++)
            {
                if (G[x, 0] > 0)
                {
                    return x;
                }
            }
            return -1;
        }

        public static int FirstHorizontalPositive(int[,] G)
        {
            for (int y = 0; y < G.GetLength(1); y++)
            {
                if (G[0, y] > 0)
                {
                    return y;
                }
            }
            return -1;
        }

        public static int[][] CopyArray(int[][] G)
        {
            int[][] copy = new int[G.Length][];

            for (int i = 0; i < G.Length; i++)
            {
                copy[i] = new int[G[0].Length];
                for (int j = 0; j < G[0].Length; j++)
                {
                    copy[i][j] = G[i][j];
                }
            }

            return copy;
        }

        public static int[,] CopyArray(int[,] G)
        {
            int[,] copy = new int[G.GetLength(0), G.GetLength(1)];

            for (int i = 0; i < G.GetLength(0); i++)
            {
                for (int j = 0; j < G.GetLength(1); j++)
                {
                    copy[i, j] = G[i, j];
                }
            }

            return copy;
        }

        public static int[,] ToMatrix(int[][] G)
        {
            int[,] result = new int[G.Length, G[0].Length];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = G[i][j];
                }
            }

            return result;
        }

        public static int[,] ListToField(List<Point> points, int[,] G, out Point topLeft)
        {
            int minX = points.Min(p => p.X);
            int maxX = points.Max(p => p.X);
            int minY = points.Min(p => p.Y);
            int maxY = points.Max(p => p.Y);

            List<Point> minXs = points.Where(p => p.X == minX).ToList();
            List<Point> maxXs = points.Where(p => p.X == maxX).ToList();

            topLeft = new Point(minX, minY);
            Point topRight = new Point(minX, maxY);
            Point bottomLeft = new Point(maxX, minY);
            Point bottomRight = new Point(maxX, maxY);
            int width = topRight.Y - topLeft.Y;
            int height = bottomLeft.X - topLeft.X;

            int[,] field = new int[height + 1, width + 1];
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    field[i, j] = G[i + topLeft.X, j + topLeft.Y];
                }
            }
            /*foreach (var point in points)
            {
                field[point.X - topLeft.X, point.Y - topLeft.Y] = G[point.X, point.Y];
            }*/

            return field;
        }

        public static int[,] ListToFieldPointsOnly(List<Point> points, int[,] G, out Point topLeft)
        {
            int minX = points.Min(p => p.X);
            int maxX = points.Max(p => p.X);
            int minY = points.Min(p => p.Y);
            int maxY = points.Max(p => p.Y);

            List<Point> minXs = points.Where(p => p.X == minX).ToList();
            List<Point> maxXs = points.Where(p => p.X == maxX).ToList();

            topLeft = new Point(minX, minY);
            Point topRight = new Point(minX, maxY);
            Point bottomLeft = new Point(maxX, minY);
            Point bottomRight = new Point(maxX, maxY);
            int width = topRight.Y - topLeft.Y;
            int height = bottomLeft.X - topLeft.X;

            int[,] field = new int[height + 1, width + 1];

            foreach (var point in points)
            {
                field[point.X - topLeft.X, point.Y - topLeft.Y] = G[point.X, point.Y];
            }

            return field;
        }

        public static List<List<Point>> EnumeratePatches(int[,] patch)
        {
            List<List<Point>> patches = new List<List<Point>>();

            int[,] Gc = ImageProcessing.CopyArray(patch);
            Point nonzero = new Point(-1, -1);
            while ((nonzero = ImageProcessing.FirstNonZero(Gc)).X != -1)
            {
                List<Point> visited = new List<Point>();
                ImageProcessing.FloodFillColor(nonzero, new List<Point>(), visited, Gc,
                    patch[nonzero.X, nonzero.Y]);
                patches.Add(new List<Point>(visited));
                visited.ForEach(p => Gc[p.X, p.Y] = 0);
            }

            return patches;
        }

        public static List<List<Point>> EnumerateMultiColorPatches(int[,] patch)
        {
            List<List<Point>> patches = new List<List<Point>>();

            int[,] Gc = ImageProcessing.CopyArray(patch);
            Point nonzero = new Point(-1, -1);
            while ((nonzero = ImageProcessing.FirstNonZero(Gc)).X != -1)
            {
                List<Point> visited = new List<Point>();
                ImageProcessing.FloodFillNonzero(nonzero, new List<Point>(), visited, Gc);
                patches.Add(new List<Point>(visited));
                visited.ForEach(p => Gc[p.X, p.Y] = 0);
            }

            return patches;
        }

        public static List<List<Point>> EnumeratePatchesAllowBackground(int[,] patch)
        {
            List<List<Point>> patches = new List<List<Point>>();

            int[,] Gc = ImageProcessing.CopyArray(patch);
            Point nonzero = new Point(-1, -1);
            while ((nonzero = ImageProcessing.FirstPositive(Gc)).X != -1)
            {
                List<Point> visited = new List<Point>();
                ImageProcessing.FloodFillColor(nonzero, new List<Point>(), visited, Gc,
                    patch[nonzero.X, nonzero.Y]);
                patches.Add(new List<Point>(visited));
                visited.ForEach(p => Gc[p.X, p.Y] = -1);
            }

            return patches;
        }
    }
}
