using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SZD_ZN8VJ5
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
            this.groupIndex = GroupCount++;
        }

        public abstract int[,] Slice(int width, int height);

        public bool AbstractEquals(Group other)
        {
            if (this.field.GetLength(0) != other.field.GetLength(0)
                || this.field.GetLength(1) != other.field.GetLength(1))
            {
                return false;
            }

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (this.field[i, j] != other.field[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            return "'" + this.groupIndex;
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