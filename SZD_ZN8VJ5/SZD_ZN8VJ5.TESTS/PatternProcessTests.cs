using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZD_ZN8VJ5.Groups;

namespace SZD_ZN8VJ5.TESTS
{
    public class PatternProcessTests
    {
        private static Group g0;
        private static Group g1;
        private static Group g2;

        [OneTimeSetUp]
        public void Init()
        {
            Groups.Groups.Instantiate();

            g0 = LoadGroup("group0.txt");
            g1 = LoadGroup("group1.txt");
            g2 = LoadGroup("group2.txt");
        }

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

        private static Group LoadGroup(string fname)
        {
            var lines = File.ReadAllLines(fname);
            int complexity = int.Parse(lines[0]);
            int rows = int.Parse(lines[1]);
            int columns = int.Parse(lines[2]);

            int[,] field = new int[rows, columns];

            for (int i = 3; i < lines.Length; i++)
            {
                int[] values = lines[i].Split('\t').Select(text => int.Parse(text)).ToArray();
                for (int j = 0; j < values.Length; j++)
                {
                    field[i - 3, j] = values[j];
                }
            }

            return new PseudoGroup(field, complexity);
        }

        [Test]
        public void UnicolorProcess_OnImage0_ReturnsTargetObjects()
        {
            int[][] image = LoadImage("img0.txt");
            List<ARCObject> expected = new List<ARCObject>();
            expected.Add(new ARCObject(
                Groups.Groups.All.First(),
                1, 1, 2, 3, new int[] { 1 }));

            var output = Preprocessing.Process_Image_UniformPattern(image);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.EquivalentTo(expected));
        }

        [Test]
        public void PatternProcess_OnImage1_ReturnsTargetObjects()
        {
            int[][] image = LoadImage("img1.txt");
            List<ARCObject> expected = new List<ARCObject>();

            ARCObject expectedNoise = new ARCObject(
                g1, 1, 3, 2, 4, new int[] { 2 }
                );

            ARCObject expectedParent = new ARCObject(
                    Groups.Groups.All.First(), 1, 1, 4, 4, new int[] { 1 });
            expectedParent.Noises.Add(expectedNoise);

            var output = Preprocessing.Process_Image_UniformPattern(image);

            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output.First(), Is.EqualTo(expectedParent));

            Assert.That(output.First().Noises.Count, Is.EqualTo(1));
            Assert.That(output.First(), Is.EqualTo(output.First().Noises.First().Parent));
        }

        [Test]
        public void PatternProcess_OnImage4_DoesNotReturnPatternObject()
        {
            int[][] image = LoadImage("img4.txt");

            var output = Preprocessing.Process_Image_UniformPattern(image);

            Assert.That(output, Is.Not.Null);
            Assert.That(output.All(obj => obj.Noises.Count == 0));
        }
    }
}
