﻿
namespace SZD_ZN8VJ5.Groups
{
    public abstract class Group
    {
        public static int GroupCount = 0;
        public int groupIndex;
        public int[,] field;
        public int complexity;
        protected int originX;
        protected int originY;

        public Group(int complexity)
        {
            this.complexity = complexity;
            groupIndex = GroupCount++;
        }

        public abstract int[,] Slice(int width, int height);

        public bool AbstractEquals(Group other)
        {
            if (field.GetLength(0) != other.field.GetLength(0)
                || field.GetLength(1) != other.field.GetLength(1))
            {
                return false;
            }

            Dictionary<int, int> map = new Dictionary<int, int>();

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    /*if (field[i, j] >= 0 && other.field[i, j] == -1)
                    {
                        return false;
                    }
                    else if (other.field[i, j] >= 0 && field[i, j] == -1)
                    {
                        return false;
                    }

                    if (!map.ContainsKey(field[i, j]))
                    {
                        if (!map.ContainsValue(other.field[i, j]))
                        {
                            map.Add(field[i, j], other.field[i, j]);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (map[field[i, j]] != other.field[i, j])
                        {
                            return false;
                        }
                    }*/

                    if (field[i, j] != other.field[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            return "'" + groupIndex;
        }

        public int RegionCount(int region)
        {
            int count = 0;

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] == region)
                    {
                        ++count;
                    }
                }
            }

            return count;
        }

        public static int ComputeComplexity(int[,] slice)
        {
            List<int> uniqueColors = new List<int>();
            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    int color = slice[i, j];
                    if (!uniqueColors.Contains(color))
                    {
                        uniqueColors.Add(color);
                    }
                }
            }
            return uniqueColors.Count;
        }
    }
}