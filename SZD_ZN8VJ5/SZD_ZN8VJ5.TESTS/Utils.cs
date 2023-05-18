using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZD_ZN8VJ5.TESTS
{
    public static class Utils
    {
        public static string DataDirectory = @"data\training\";

        public static ARC_File GetFileByName(string name)
        {
            return JsonConvert.DeserializeObject<ARC_File>(File.ReadAllText(Utils.DataDirectory + name + ".json"));
        }

        public static int[][] Image0
        {
            get { return LoadImage("img0.txt"); }
        }
        public static int[][] Image1;

        private static int[][] LoadImage(string fname)
        {
            var lines = File.ReadAllLines(fname);
            int[][] image = new int[lines.Length][];

            for (int i = 0; i < lines.Length; i++)
            {
                image[i] = lines[i].Split('\t').Select(text => int.Parse(text)).ToArray();
            }

            return image;
        }
    }
}
