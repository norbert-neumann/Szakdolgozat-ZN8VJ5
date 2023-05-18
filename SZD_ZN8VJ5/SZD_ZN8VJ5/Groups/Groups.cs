
namespace SZD_ZN8VJ5.Groups
{
    public class Groups
    {
        public static List<Group> All;

        public static void Instantiate()
        {
            if (All == null)
            {
                All = new List<Group>();

                int[,] uniform = new int[30, 30];
                for (int y = 0; y < 30; y++)
                {
                    for (int x = 0; x < 30; x++)
                    {
                        uniform[x, y] = 0;
                    }
                }
                All.Add(new TranslationGroup(uniform, 1));
                //All.Add(Generate_ReflectionGroup(30));
            }

        }

        public static ReflectionGroup Generate_ReflectionGroup(int N)
        {
            int[,] field;
            int c;
            if (N % 2 == 0)
            {
                field = Generate_EvenReflectionGroup(N);
                c = N / 2;
            }
            else
            {
                field = Generate_OddReflectionGroup(N);
                c = (N / 2) + 1;
            }

            return new ReflectionGroup(field, c * c - 1);
        }

        private static int[,] Generate_OddReflectionGroup(int N)
        {
            int[,] field = new int[N, N];
            int start = 0;
            int c = (N / 2) + 1;

            for (int row = 0; row < c; row++)
            {
                start = row * c;

                for (int col = 0; col < c; col++)
                {
                    field[row, col] = start++;
                }

                --start;

                for (int col = c; col < N; col++)
                {
                    field[row, col] = --start;
                }
            }

            int back = -2;
            for (int row = c; row < N; row++)
            {
                for (int col = 0; col < N; col++)
                {
                    field[row, col] = field[row + back, col];
                }
                back -= 2;
            }

            return field;
        }

        private static int[,] Generate_EvenReflectionGroup(int N)
        {
            int[,] field = new int[N, N];
            int start = 0;
            int c = N / 2;

            for (int row = 0; row < c; row++)
            {
                start = row * c;

                for (int col = 0; col < c; col++)
                {
                    field[row, col] = start++;
                }

                for (int col = c; col < N; col++)
                {
                    field[row, col] = --start;
                }
            }

            int back = -1;
            for (int row = c; row < N; row++)
            {
                for (int col = 0; col < N; col++)
                {
                    field[row, col] = field[row + back, col];
                }
                back -= 2;
            }

            return field;
        }
    }
}
