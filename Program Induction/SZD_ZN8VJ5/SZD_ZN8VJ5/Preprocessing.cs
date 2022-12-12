using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SZD_ZN8VJ5
{
    class ObjectGroup
    {
        public List<ARCObject> Objects = new List<ARCObject>();

        public void Add(ARCObject obj) { Objects.Add(obj); }

        public bool SetEquals(ObjectGroup other)
        {
            return this.Objects.Union(other.Objects).Count() == this.Objects.Count;
        }
    }

    public static class Preprocessing
    {
        public static List<PseudoGroup> ExistingPseudoGroups = new List<PseudoGroup>();
        public static List<ARCObject> Objects = new List<ARCObject>();

        private static List<ARCObject> Patterns(int[,] patch)
        {
            return null;
        }

        private static ARCObject SynthetizeObject(Point topLeft, int[,] patch, bool isNoise)
        {
            List<int> uniqueColors = new List<int>();
            for (int i = 0; i < patch.GetLength(0); i++)
            {
                for (int j = 0; j < patch.GetLength(1); j++)
                {
                    int color = patch[i, j];
                    if (!uniqueColors.Contains(color)) //color > 0 && 
                    {
                        uniqueColors.Add(color);
                    }
                }
            }
            if (!isNoise)
            {
                uniqueColors.Remove(0);
            }
            uniqueColors.Remove(-1);

            return new ARCObject(
                new PseudoGroup(patch, uniqueColors.Count, uniqueColors, isNoise),
                topLeft.X,
                topLeft.Y,
                patch.GetLength(1),
                patch.GetLength(0),
                uniqueColors.ToArray()
                );
        }

        private static List<ARCObject> PatchToObjects(Point topLeft, int[,] patch)
        {
            List<ARCObject> objects = new List<ARCObject>();
            foreach (var G in Groups.All)
            {
                ARCObject result = TryGroup(G, patch, 0.6f, 0.5f);
                if (result != null)
                {
                    result.x = topLeft.X;
                    result.y = topLeft.Y;
                    result.width = patch.GetLength(1);
                    result.height = patch.GetLength(0);
                    objects.Add(result);
                }
            }
            return objects;
        }

        private static List<ARCObject> ValidPseudoObjects(Point topLeft, int[,] patch)
        {
            int patchHeight = patch.GetLength(0);
            int patchWidth = patch.GetLength(1);
            List<ARCObject> objects = new List<ARCObject>();
            foreach (var G in ExistingPseudoGroups)
            {
                int groupHeight = G.field.GetLength(0);
                int groupWidth = G.field.GetLength(1);
                if (patchHeight == groupHeight && patchWidth == groupWidth)
                {
                    Dictionary<int, int> regionToColorDict = PseudoGroupMatch(G, patch);
                    if (regionToColorDict != null)
                    {
                        int[] regionToColor = new int[regionToColorDict.Count];
                        foreach (var pair in regionToColorDict)
                        {
                            regionToColor[pair.Key] = pair.Value;
                        }

                        objects.Add(new ARCObject(
                           G,
                           topLeft.X,
                           topLeft.Y,
                           patch.GetLength(1),
                           patch.GetLength(0),
                           regionToColor
                           ));
                    }

                    /*int[,] cropped = Crop(patch, 0, 0, groupWidth, groupHeight);
                    ARCObject result = TryGroup(G, cropped, 1f, 0f);
                    if (result != null)
                    {
                        result.x = topLeft.X;
                        result.y = topLeft.Y;
                        result.width = groupWidth;
                        result.height = groupHeight;
                        objects.Add(result);
                    }*/
                }
            }
            return objects;
        }

        private static List<ARCObject> ValidExistingObjects(Point topLeft, int[,] patch)
        {
            List<ARCObject> objects = new List<ARCObject>();

            objects.AddRange(PatchToObjects(topLeft, patch));
            objects.AddRange(ValidPseudoObjects(topLeft, patch));

            return objects;
        }

        private static List<ARCObject> ValidObjects(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ARCObject> objects = new List<ARCObject>();

            // Add proper groups
            objects.AddRange(PatchToObjects(topLeft, patch));

            // Add reused pseudo-groups
            List<ARCObject> reused = ValidPseudoObjects(topLeft, patch);
            objects.AddRange(reused);

            // Create synthetic full object
            ARCObject full = SynthetizeObject(topLeft, patch, isNoise);
            if (!reused.Contains(full))
            {
                objects.Add(full);
                ExistingPseudoGroups.Add((PseudoGroup)full.Group);
            }

            // Enumerate first uni-color continous patch
            var firstPatch = Helpers.EnumeratePatches(patch).FirstOrDefault();
            if (firstPatch != null)
            {
                Point TL_internal;
                int[,] colorPatch = Helpers.ListToField(firstPatch, patch, out TL_internal);
                TL_internal.Offset(topLeft);
                var objs = ValidExistingObjects(TL_internal, colorPatch);
                // do not synth here, search for valid proper or existing pseujdo-groups

                foreach (var obj in objs.Distinct())
                {
                    if (objects.All(existing => !existing.Equals(obj)))
                    {
                        objects.Add(obj);
                    }
                }
            }

            List<ARCObject> output = new List<ARCObject>();
            foreach (var obj in objects)
            {
                if (!DoubleExplain(patch, obj.Visual()))
                {
                    output.Add(obj);
                }
            }

            return output;
        }

        private static List<ARCObject> IDK(Point topLeft, int[,] patch)
        {
            int patchHeight = patch.GetLength(0);
            int patchWidth = patch.GetLength(1);
            List<ARCObject> objects = new List<ARCObject>();
            foreach (var G in ExistingPseudoGroups)
            {
                int groupHeight = G.field.GetLength(0);
                int groupWidth = G.field.GetLength(1);
                if (patchHeight >= groupHeight && patchWidth >= groupWidth)
                {
                    int xEnd = patchHeight - groupHeight + 1; // possible source of error
                    int yEnd = patchWidth - groupWidth + 1;

                    for (int dx = 0; dx < xEnd; dx++)
                    {
                        for (int dy = 0; dy < yEnd; dy++)
                        {
                            int[,] cropped = Crop(patch, dx, dy, groupWidth, groupHeight);
                            ARCObject result = TryGroup(G, cropped, 1f, 0f);
                            if (result != null)
                            {
                                result.x = topLeft.X + dx;
                                result.y = topLeft.Y + dy;
                                result.width = groupWidth;
                                result.height = groupHeight;
                                objects.Add(result);
                            }
                        }
                    }

                }
            }
            return objects;
        }

        public static List<ARCObject> Outer(int[,] input)
        {
            List<ARCObject> objects = new List<ARCObject>();
            foreach (var patchPoints in Helpers.EnumerateMultiColorPatches(input))
            {
                Point TL;
                int[,] patch = Helpers.ListToField(patchPoints, input, out TL);
                var explanations = Process(TL, patch, false);

                var container = objects.Where(obj => Helpers.Contains(obj, explanations.First())).LastOrDefault();
                explanations.ForEach(exp => exp.containedBy = container);

                objects.AddRange(explanations);
            }

            return objects;
        }

        // Standard, no edge cases
        private static List<ARCObject> Process(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ObjectGroup> groups = new List<ObjectGroup>();

            foreach (ARCObject obj in ValidObjects(topLeft, patch, isNoise))
            {
                ObjectGroup group = new ObjectGroup();
                group.Add(obj);

                int[,] objVisual = obj.Visual(
                    patch.GetLength(0), patch.GetLength(1),
                    obj.x - topLeft.X, obj.y - topLeft.Y);

                int[,] overlaps = OverlappingParts2(patch, objVisual);
                int[,] nonOverlaps = NonOverlappingParts2(patch, objVisual);

                // Explain overlaps
                foreach (List<Point> overlapPoints in Helpers.EnumeratePatchesAllowBackground(overlaps))
                {
                    Point overlapTL;
                    int[,] overlapPatch = Helpers.ListToField(overlapPoints, overlaps, out overlapTL);
                    var overlapExp = Process(overlapTL, overlapPatch, true);
                    overlapExp.ForEach(noise => obj.noises.Add(noise));
                    overlapExp.ForEach(noise => noise.noiseTo = obj);
                }

                // Explain residuals
                foreach (List<Point> residualPoints in Helpers.EnumerateMultiColorPatches(nonOverlaps))
                {
                    Point TL;
                    int[,] residualPatch = Helpers.ListToField(residualPoints, nonOverlaps, out TL);
                    var residualExp = Process(TL, residualPatch, false);
                    residualExp.ForEach(residual => group.Objects.Add(residual));
                }

                groups.Add(group);
            }

            // Simplify: if elements in two groups can be paired then one of them is redundant
            List<ObjectGroup> noRedundants = new List<ObjectGroup>();

            foreach (var group in groups)
            {
                if (groups.Count(G => group.SetEquals(G)) == 1)
                {
                    noRedundants.Add(group);
                }
            }
            groups = noRedundants;


            // Each group explains all others
            foreach (var group in groups)
            {
                Helpers.Explainings.Add(group.Objects, new List<ARCObject>());
            }
            foreach (var group in groups)
            {
                foreach (var other in groups)
                {
                    Helpers.Explainings[group.Objects].AddRange(other.Objects);
                }
            }

            List<ARCObject> output = new List<ARCObject>();
            groups.ForEach(g => g.Objects.ForEach(obj => output.Add(obj)));
            return output;
        }

        public static int[,] Crop(int[,] G, int x, int y, int width, int height)
        {
            int[,] Gout = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Gout[i, j] = G[x + i, y + j];
                }
            }
            return Gout;
        }

        private static ARCObject TryGroup(Group G, int[,] patch, float theta1, float theta2)
        {
            int[,] slice = G.Slice(patch.GetLength(1), patch.GetLength(0));
            int complexity = Group.ComputeComplexity(slice);
            int[,] regionToColorCount = new int[complexity, 10];
            int[] regionFrequency = new int[complexity];

            int regionCount = 0;
            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    slice[i, j] = regionCount;
                    if (regionCount + 1 < complexity)
                    {
                        regionCount++;
                    }
                }
            }

            for (int i = 0; i < patch.GetLength(0); i++)
            {
                for (int j = 0; j < patch.GetLength(1); j++)
                {
                    if (patch[i, j] >= 0)
                    {
                        int v = patch[i, j];
                        regionToColorCount[slice[i, j], patch[i, j]]++;
                        regionFrequency[slice[i, j]]++;
                    }
                }
            }

            int[] regionToColor = new int[complexity];
            for (int i = 0; i < regionToColor.Length; i++)
            {
                int maxIdx = 0;
                for (int j = 1; j < 10; j++)
                {
                    if (regionToColorCount[i, j] > regionToColorCount[i, maxIdx])
                    {
                        maxIdx = j;
                    }
                }
                regionToColor[i] = maxIdx;
            }

            int hits = 0;
            int[,] reconstruction = new int[patch.GetLength(0), patch.GetLength(1)];
            for (int i = 0; i < reconstruction.GetLength(0); i++)
            {
                for (int j = 0; j < reconstruction.GetLength(1); j++)
                {
                    reconstruction[i, j] = regionToColor[slice[i, j]];
                    if (reconstruction[i, j] == patch[i, j])
                    {
                        hits++;
                    }
                }
            }

            float accuracy = hits / (float)patch.Length;
            float predictionScore = 0;
            int sum = 0;
            for (int i = 0; i < complexity; i++)
            {
                predictionScore += regionToColorCount[i, regionToColor[i]];
                sum += regionFrequency[i];
            }
            predictionScore /= sum;
            predictionScore /= G.complexity;

            if (accuracy >= theta1 && predictionScore >= theta2)
            {
                return new ARCObject(G, regionToColor);
            }

            return null;
        }

        private static Dictionary<int, int> PseudoGroupMatch(PseudoGroup G, int[,] patch)
        {
            Dictionary<int, int> T = new Dictionary<int, int>();
            for (int i = 0; i < patch.GetLength(0); i++)
            {
                for (int j = 0; j < patch.GetLength(1); j++)
                {
                    if (G.field[i, j] >= 0)
                    {
                        if (patch[i, j] == -1)
                        {
                            return null;
                        }
                        else if (!T.ContainsKey(G.field[i, j]))
                        {
                            T.Add(G.field[i, j], patch[i, j]);
                        }
                        else if (T[G.field[i, j]] != patch[i, j])
                        {
                            return null;
                        }
                    }
                }
            }
            return T;
        }

        private static int[,] NonOverlappingParts2(int[,] patch, int[,] exp)
        {
            int[,] result = new int[patch.GetLength(0), patch.GetLength(1)];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    if (patch[i, j] > 0 && exp[i, j] == -1)
                    {
                        result[i, j] = patch[i, j];
                    }
                    else
                    {
                        result[i, j] = -1;
                    }
                }
            }

            return result;
        }

        private static int[,] OverlappingParts2(int[,] patch, int[,] exp)
        {
            int[,] result = new int[patch.GetLength(0), patch.GetLength(1)];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    if (patch[i, j] != exp[i, j] && exp[i, j] >= 0)
                    {
                        result[i, j] = patch[i, j];
                    }
                    else
                    {
                        result[i, j] = -1;
                    }
                }
            }

            return result;
        }

        private static int[,] NonOverlappingParts(int[,] patch, int[,] exp)
        {
            int[,] result = new int[patch.GetLength(0), patch.GetLength(1)];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    if (i >= exp.GetLength(0) || j >= exp.GetLength(1))
                    {
                        result[i, j] = patch[i, j];
                    }
                    else
                    {
                        if (exp[i, j] == -1)
                        {
                            result[i, j] = patch[i, j];
                        }
                        else
                        {
                            result[i, j] = -1;
                        }
                    }
                }
            }
            return result;
        }

        private static int[,] ExplainPatch(int[,] patch, int[,] exp)
        {
            int[,] result = new int[exp.GetLength(0), exp.GetLength(1)];
            for (int i = 0; i < exp.GetLength(0); i++)
            {
                for (int j = 0; j < exp.GetLength(1); j++)
                {
                    if (patch[i, j] != exp[i, j] && exp[i, j] >= 0) // exp is -1, patch is 0, but they should match
                    {
                        result[i, j] = patch[i, j];
                    }
                    else
                    {
                        result[i, j] = -1;
                    }
                }
            }
            return result;
        }

        private static bool DoubleExplain(int[,] patch, int[,] exp)
        {
            for (int i = 0; i < exp.GetLength(0); i++)
            {
                for (int j = 0; j < exp.GetLength(1); j++)
                {
                    if (patch[i, j] == -1 && exp[i, j] >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static List<ARCObject> Simplify(List<ARCObject> A, List<ARCObject> B)
        {
            foreach (var b in B)
            {
                if (!A.Contains(b))
                {
                    A.Add(b);
                }
            }
            return A;
        }

        private static List<ARCObject> Placeholder(Point topLeft, int[,] patch)
        {
            List<ARCObject> unlucyObjects = new List<ARCObject>();
            if (patch[0, 0] == 0)
            {
                int verticalStart = Helpers.FirstVerticalPositive(patch);
                int horizontalStart = Helpers.FirstHorizontalPositive(patch);

                int[,] bottomPatch = Crop(patch, verticalStart, 0,
                    patch.GetLength(1), patch.GetLength(0) - verticalStart);

                int[,] rightPatch = Crop(patch, 0,
                    horizontalStart, patch.GetLength(1) - horizontalStart,
                    patch.GetLength(0));

                var bottomExp = Process(new Point(verticalStart, topLeft.Y), bottomPatch, false);
                var rightExp = Process(new Point(topLeft.X, horizontalStart), rightPatch, false);
                unlucyObjects.AddRange(Simplify(bottomExp, rightExp));
            }
            return unlucyObjects;
        }
    }
}
