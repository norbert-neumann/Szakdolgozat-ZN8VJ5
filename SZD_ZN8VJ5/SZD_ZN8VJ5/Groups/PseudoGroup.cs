
namespace SZD_ZN8VJ5.Groups
{
    public class PseudoGroup : Group
    {
        public PseudoGroup(int[,] field, int complexity) : base(complexity)
        {
            this.field = field;
        }

        public PseudoGroup(int[,] field, int complexity, List<int> uniqueColors,
            bool isNoise)
            : base(complexity)
        {
            originX = 0;
            originY = 0;

            this.field = new int[field.GetLength(0), field.GetLength(1)];

            if (!isNoise)
            {
                for (int i = 0; i < field.GetLength(0); i++)
                {
                    for (int j = 0; j < field.GetLength(1); j++)
                    {
                        if (field[i, j] > 0)
                        {
                            this.field[i, j] = uniqueColors.IndexOf(field[i, j]);
                        }
                        else
                        {
                            this.field[i, j] = -1;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < field.GetLength(0); i++)
                {
                    for (int j = 0; j < field.GetLength(1); j++)
                    {
                        this.field[i, j] = uniqueColors.IndexOf(field[i, j]);
                    }
                }
            }
        }

        public override int[,] Slice(int width, int height)
        {
            int[,] result = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = field[i, j];
                }
            }
            return result;
        }
    }
}
