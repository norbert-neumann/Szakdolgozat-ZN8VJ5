using Newtonsoft.Json;
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
    public class PostprocessingTests
    {
        private static Group g0;
        private static Group g1;
        private static Group g2;

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

        private static IEnumerable<TestCaseData> ClipModeTests_Source()
        {
            //c0f76784
            yield return new TestCaseData(Utils.GetFileByName("b2862040"), ClipMode.Multiply);
            yield return new TestCaseData(Utils.GetFileByName("39e1d7f9"), ClipMode.Multiply);
            yield return new TestCaseData(Utils.GetFileByName("178fcbfb"), ClipMode.Multiply);
            yield return new TestCaseData(Utils.GetFileByName("d90796e8"), ClipMode.Multiply);
            yield return new TestCaseData(Utils.GetFileByName("f5b8619d"), ClipMode.Multiply);

            //662c240a
            yield return new TestCaseData(Utils.GetFileByName("4c4377d9"), ClipMode.Constant);
            yield return new TestCaseData(Utils.GetFileByName("c0f76784"), ClipMode.Constant);
            yield return new TestCaseData(Utils.GetFileByName("a8c38be5"), ClipMode.Constant);
            yield return new TestCaseData(Utils.GetFileByName("44f52bb0"), ClipMode.Constant);
            yield return new TestCaseData(Utils.GetFileByName("662c240a"), ClipMode.Constant);

            //e6721834
            yield return new TestCaseData(Utils.GetFileByName("5daaa586"), ClipMode.Fit);
            yield return new TestCaseData(Utils.GetFileByName("72ca375d"), ClipMode.Fit);
            yield return new TestCaseData(Utils.GetFileByName("b94a9452"), ClipMode.Fit);
            yield return new TestCaseData(Utils.GetFileByName("8731374e"), ClipMode.Fit);
            yield return new TestCaseData(Utils.GetFileByName("e6721834"), ClipMode.Fit);
        }

        private static IEnumerable<TestCaseData> RenderTests_Source()
        {
            g0 = LoadGroup("group0.txt");
            g1 = LoadGroup("group1.txt");
            g2 = LoadGroup("group2.txt");

            ARCObject obj1 = new ARCObject(g0, 0, 0, 3, 4, new int[] { 5 });
            ARCObject obj2 = new ARCObject(g0, 6, 10, 3, 4, new int[] { 1 });
            ARCObject obj3 = new ARCObject(g1, 20, 20, 2, 4, new int[] { 2 });
            ARCObject obj4 = new ARCObject(g2, 5, 9, 8, 8, new int[] { 9 });
            ARCObject obj5 = new ARCObject(g0, 7, 11, 3, 4, new int[] { 1 });

            List<ARCObject> scene1 = new List<ARCObject>() { obj1 };
            List<ARCObject> scene2 = new List<ARCObject>() { obj1, obj2 };
            List<ARCObject> scene3 = new List<ARCObject>() { obj1, obj3 };
            List<ARCObject> scene4 = new List<ARCObject>() { obj4, obj5 };
            List<ARCObject> scene5 = new List<ARCObject>() { obj1, obj3, obj4, obj5 };

            yield return new TestCaseData(scene1, LoadImage("img5.txt"));
            yield return new TestCaseData(scene2, LoadImage("img6.txt"));
            yield return new TestCaseData(scene3, LoadImage("img7.txt"));
            yield return new TestCaseData(scene4, LoadImage("img8.txt"));
            yield return new TestCaseData(scene5, LoadImage("img9.txt"));
        }

        private static IEnumerable<TestCaseData> FitClipTests_Source()
        {
            yield return new TestCaseData(LoadImage("img5.txt"), LoadImage("fit_img5.txt"));
            yield return new TestCaseData(LoadImage("img6.txt"), LoadImage("fit_img6.txt"));
            yield return new TestCaseData(LoadImage("img7.txt"), LoadImage("fit_img7.txt"));
            yield return new TestCaseData(LoadImage("img8.txt"), LoadImage("fit_img8.txt"));
            yield return new TestCaseData(LoadImage("img9.txt"), LoadImage("fit_img9.txt"));
        }

        [Test]
        [TestCaseSource(nameof(ClipModeTests_Source))]
        public void ClipModeTests(ARC_File file, ClipMode expected)
        {
            Postprocessing.SetClipMode(file);

            Assert.That(Postprocessing.Mode, Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource(nameof(RenderTests_Source))]
        public void RenderTests(List<ARCObject> objects, int[][] expected)
        {
            int[][] result = Postprocessing.Render(objects);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource(nameof(FitClipTests_Source))]
        public void FitClipTests(int[][] image, int[][] expected)
        {
            Postprocessing.Mode = ClipMode.Fit;

            int[][] result = Postprocessing.Clip(image, null);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
