using SZD_ZN8VJ5.Groups;

namespace SZD_ZN8VJ5
{
    public static class DSL
    {
        public static int MaxColor(ARCObject obj)
        {
            return obj.RegionToColor.Max();
        }

        public static int DominantColor(ARCObject obj)
        {
            Dictionary<int, int> colorFrequencies = new Dictionary<int, int>();
            var visual = obj.Visual();

            for (int i = 0; i < visual.GetLength(0); i++)
            {
                for (int j = 0; j < visual.GetLength(1); j++)
                {
                    int color = visual[i, j];
                    if (!colorFrequencies.ContainsKey(color))
                    {
                        colorFrequencies.Add(color, 0);
                    }
                    colorFrequencies[color]++;
                }
            }
            return colorFrequencies.MaxBy(kvp => kvp.Value).Key;
        }

        public static Group FlattenGroup(Group group)
        {
            int[,] field = new int[group.field.GetLength(0), group.field.GetLength(1)];
            for (int x = 0; x < group.field.GetLength(0); x++)
            {
                for (int y = 0; y < group.field.GetLength(1); y++)
                {
                    field[x, y] = group.field[x, y] >= 0 ? 0 : -1;
                }
            }

            return new PseudoGroup(field, 1);
        }

        public static Group RotateRigth(Group group)
        {
            int[,] field = new int[group.field.GetLength(1), group.field.GetLength(0)];

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    field[i, j] = group.field[j, i];
                }
            }

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1) / 2; j++)
                {
                    int tmp = field[i, j];
                    field[i, j] = field[i, field.GetLength(1) - 1 - j];
                    field[i, field.GetLength(1) - 1 - j] = tmp;
                }
            }

            return new PseudoGroup(field, group.complexity);
        }

        public static Group RotateLeft(Group group)
        {
            int[,] field = new int[group.field.GetLength(1), group.field.GetLength(0)];

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    field[i, j] = group.field[j, i];
                }
            }

            for (int i = 0; i < field.GetLength(0) / 2; i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    int tmp = field[i, j];
                    field[i, j] = field[field.GetLength(0) - 1 - i, j];
                    field[field.GetLength(0) - 1 - i, j] = tmp;
                }
            }

            return new PseudoGroup(field, group.complexity);
        }

        public static Group VerticalReflect(Group group)
        {
            int[,] field = new int[group.field.GetLength(0), group.field.GetLength(1)];

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    field[i, j] = group.field[i, group.field.GetLength(1) - 1 - j];
                }
            }

            return new PseudoGroup(field, group.complexity);
        }

        public static ARCObject ClipOverlappedPart(ARCObject obj, ARCObject noise)
        {
            int[,] objVisual = obj.Visual();
            int[,] resultVisual = new int[noise.Height, noise.Width];

            for (int v = 0; v < noise.Height; v++)
            {
                for (int h = 0; h < noise.Width; h++)
                {
                    resultVisual[v, h] = objVisual[v + noise.X, h + noise.Y];
                }
            }

            return Preprocessing.SynthetizeObject(new System.Drawing.Point(0, 0), resultVisual, true);
        }
    }
}
