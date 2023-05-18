using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZD_ZN8VJ5.Groups;

namespace SZD_ZN8VJ5.TESTS
{
    [TestFixture]
    public class MulticolorProcessTests
    {
        private static Group g0;
        private static Group g1;
        private static Group g2;
        private static Group g3;
        private static Group g4;

        [OneTimeSetUp]
        public void Init()
        {
            Groups.Groups.Instantiate();

            g0 = LoadGroup("group0.txt");
            g1 = LoadGroup("group1.txt");
            g2 = LoadGroup("group2.txt");
            g3 = LoadGroup("group3.txt");
            g4 = LoadGroup("group4.txt");
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
        public void MulticolorProcess_OnImage0_ReturnsTargetObjects()
        {
            int[][] image = LoadImage("img0.txt");
            List<ARCObject> expected = new List<ARCObject>();
            expected.Add(new ARCObject(
                Groups.Groups.All.First(),
                1, 1, 2, 3, new int[] { 1 }));

            var output = Preprocessing.Process_Image_Multicolor(image);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.EquivalentTo(expected));
        }

        [Test]
        public void MulticolorProcess_OnImage1_ReturnsTargetObjects()
        {
            int[][] image = LoadImage("img1.txt");
            List<ARCObject> expected = new List<ARCObject>();

            expected.Add(new ARCObject(
                    g4, 1, 1, 4, 4, new int[] { 1, 2 }
                    ));



            var output = Preprocessing.Process_Image_Multicolor(image);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.EquivalentTo(expected));
        }

        [Test]
        public void MulticolorProcess_OnImage3_ReturnsTargetObjects()
        {
            int[][] image = LoadImage("img3.txt");
            List<ARCObject> expected = new List<ARCObject>();

            expected.Add(new ARCObject(
                    g3, 1, 1, 8, 8, new int[] { 1, 2 }
                    ));



            var output = Preprocessing.Process_Image_Multicolor(image);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.EquivalentTo(expected));
        }

        [Test]
        public void MulticolorProcess_EstablishesContainsRelation()
        {
            int[][] image = LoadImage("img2.txt");

            var output = Preprocessing.Process_Image_Multicolor(image);

            Assert.That(output.Count, Is.EqualTo(2));
            Assert.That(output.Exists(obj => obj.ContainedObjects.Count > 0));
            var container = output.Single(obj => obj.ContainedObjects.Count > 0);

            Assert.That(output.Exists(obj => obj.ContainedBy != null));
            var contained = output.Single(obj => obj.ContainedBy != null);

            Assert.That(container, Is.Not.EqualTo(contained));
            Assert.That(container.ContainedObjects.Contains(contained));
            Assert.That(contained.ContainedBy, Is.EqualTo(container));
        }

        [Test]
        [TestCaseSource(nameof(MulticolorProcess_NeverReturnsNoisedObject_Scource))]
        public void MulticolorProcess_NeverReturnsNoisedObject(int[][] image)
        {
            var output = Preprocessing.Process_Image_Multicolor(image);

            Assert.That(output.All(obj => obj.Noises.Count == 0));
        }

        private static IEnumerable<int[][]> MulticolorProcess_NeverReturnsNoisedObject_Scource()
        {
            yield return LoadImage("img0.txt");
            yield return LoadImage("img1.txt");
            yield return LoadImage("img2.txt");
        }
    }
}
