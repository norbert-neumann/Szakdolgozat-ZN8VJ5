using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZD_ZN8VJ5.Groups
{
    public class ReflectionGroup : Group
    {
        public ReflectionGroup(int[,] field, int complexity)
            : base(complexity)
        {
            originX = 0;
            originY = 0;

            this.field = new int[field.GetLength(0), field.GetLength(1)];
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    this.field[i, j] = field[i, j];
                }
            }
        }

        public override int[,] Slice(int width, int height)
        {
            int[,] result = new int[height, width];
            int row = (this.field.GetLength(0) / 2) - height / 2;
            int column = (this.field.GetLength(1) / 2) - width / 2;

            for (int i = row; i < row + height; i++)
            {
                for (int j = column; j < column + width; j++)
                {
                    result[i - row, j - column] = field[i, j];
                }
            }
            return result;
        }
    }
}
