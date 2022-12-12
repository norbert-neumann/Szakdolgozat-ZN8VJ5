using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZD_ZN8VJ5
{
    public class Groups
    {
        public static List<Group> All;
        public static void Instantiate()
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

            int[,] spaceMod4Vertical = new int[30, 30];
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 30; x++)
                {
                    spaceMod4Vertical[x, y] = x + (30 * (y % 4));
                }
            }
            All.Add(new TranslationGroup(spaceMod4Vertical, 120));
        }
    }
}
