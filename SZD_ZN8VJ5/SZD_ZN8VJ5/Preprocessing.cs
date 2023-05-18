using System.Drawing;
using SZD_ZN8VJ5.Groups;

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

        public static ARCObject SynthetizeObject(Point topLeft, int[,] patch, bool isNoise)
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

        private static List<ARCObject> PatchToObjects(Point topLeft, int[,] patch, bool allowZero, float theta1 = 0.6f, float theta2 = 0.5f)
        {
            List<ARCObject> objects = new List<ARCObject>();
            foreach (var G in Groups.Groups.All)
            {
                ARCObject result = TryComplexGroup(G, patch, theta1, theta2);

                if (result != null)
                {
                    
                    if (!allowZero && result.RegionToColor.Contains(0))
                    {
                        continue;
                    }

                    result.X = topLeft.X;
                    result.Y = topLeft.Y;
                    result.Width = patch.GetLength(1);
                    result.Height = patch.GetLength(0);
                    result._Visual = new Visual(result);
                    objects.Add(result);
                    break;
                }
            }
            return objects;
        }

        private static ARCObject PatchToUniformPatternObject(Point topLeft, int[,] patch, bool allowZero, float theta1 = 0.6f, float theta2 = 0.5f)
        {
            ARCObject result = TryGroup(Groups.Groups.All.First(), patch, theta1, theta2);

            if (result != null)
            {
                if (!allowZero && result.RegionToColor.Contains(0))
                {
                    return null;
                }

                result.X = topLeft.X;
                result.Y = topLeft.Y;
                result.Width = patch.GetLength(1);
                result.Height = patch.GetLength(0);
                result._Visual = new Visual(result);
            }

            return result;
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
                if (patchHeight == groupHeight && patchWidth == groupWidth && PossibleGroupMatch(G, patch))
                {
                    Dictionary<int, int> regionToColorDict = PseudoGroupMatch(G, patch);
                    if (regionToColorDict != null)
                    {
                        int[] regionToColor = new int[regionToColorDict.Count];
                        foreach (var pair in regionToColorDict)
                        {
                            regionToColor[pair.Key] = pair.Value;
                        }

                        /*if (regionToColor.Contains(0))
                        {
                            continue;
                        }*/

                        objects.Add(new ARCObject(
                           G,
                           topLeft.X,
                           topLeft.Y,
                           patch.GetLength(1),
                           patch.GetLength(0),
                           regionToColor
                           ));
                    }
                }
            }
            return objects;
        }

        private static List<ARCObject> CollectObjects_1(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ARCObject> objects = new List<ARCObject>();

            // Add proper groups
            var properObject = PatchToUniformPatternObject(topLeft, patch, isNoise, 1f, 1f);
            if (properObject != null)
            {
                objects.Add(properObject);
            }
            bool properGroupFound = objects.Count > 0 && objects.Any(obj => !ExpHasOverlaps(patch, obj.Visual()));

            // Add reused pseudo-groups
            List<ARCObject> reused = ValidPseudoObjects(topLeft, patch);
            if (!properGroupFound)
            {
                foreach (var reusedObj in reused)
                {
                    if (reusedObj.Sum() == Sum(patch, isNoise))
                    {
                        objects.Add(reusedObj);
                    }
                }
            }

            // Create synthetic full object
            ARCObject full = SynthetizeObject(topLeft, patch, isNoise);
            if (!reused.Contains(full) && !properGroupFound)
            {
                objects.Add(full);
                ExistingPseudoGroups.Add((PseudoGroup)full.Group);
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

        private static List<ARCObject> CollectObjects_2(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ARCObject> objects = new List<ARCObject>();

            // Add proper groups
            var properObject = PatchToUniformPatternObject(topLeft, patch, isNoise, 0.5f, 0.5f);
            if (properObject != null)
            {
                objects.Add(properObject);
            }

            // Enumerate first uni-color continous patch
            var firstPatch = ImageProcessing.EnumeratePatches(patch).FirstOrDefault();
            if (firstPatch != null && objects.Count == 0)
            {
                Point TL_internal;
                int[,] colorPatch = ImageProcessing.ListToField(firstPatch, patch, out TL_internal);
                TL_internal.Offset(topLeft);
                objects.AddRange(PatchToObjects(TL_internal, colorPatch, isNoise));
            }

            if (objects.Count == 0)
            {
                List<ARCObject> reused = ValidPseudoObjects(topLeft, patch);
                objects.AddRange(reused);
                if (reused.Count == 0)
                {
                    ARCObject full = SynthetizeObject(topLeft, patch, isNoise);
                    objects.Add(full);
                    ExistingPseudoGroups.Add((PseudoGroup)full.Group);
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

            return objects;
        }

        private static List<ARCObject> CollectObjects_ComplexPattern(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ARCObject> objects = new List<ARCObject>();

            // Add proper groups
            objects.AddRange(PatchToObjects(topLeft, patch, allowZero: true, 0.7f, 0.7f));

            if (objects.Count == 0)
            {
                List<ARCObject> reused = ValidPseudoObjects(topLeft, patch);
                objects.AddRange(reused);
                if (reused.Count == 0)
                {
                    ARCObject full = SynthetizeObject(topLeft, patch, isNoise);
                    objects.Add(full);
                    ExistingPseudoGroups.Add((PseudoGroup)full.Group);
                }
            }

            return objects;
        }

        // Unicolor 1f theta
        public static List<ARCObject> Process_Image_Unicolor(int[][] G)
        {
            List<ARCObject> objects = new List<ARCObject>();
            int[,] input = ImageProcessing.ToMatrix(G);

            foreach (var patchPoints in ImageProcessing.EnumeratePatches(input))
            {
                Point TL;
                int[,] patch = ImageProcessing.ListToFieldPointsOnly(patchPoints, input, out TL);
                var explanations = Process_Patch_Unicolor(TL, patch, false);
                objects.AddRange(explanations);

                foreach (var other in objects)
                {
                    var contained = objects.Where(exp => other != exp && ContainsWOCoexsits(other, exp)).ToList();
                    if (contained.Count > 0)
                    {
                        other.ContainedObjects = contained;
                        contained.ForEach(exp => exp.ContainedBy = other);
                    }
                }

            }


            return objects;
        }

        private static List<ARCObject> Process_Patch_Unicolor(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ARCObject> output = new List<ARCObject>();

            foreach (ARCObject obj in CollectObjects_1(topLeft, patch, isNoise))
            {
                output.Add(obj);
                // There won't be any overlapped objects
                // No residuals either
            }

            
            return output;
        }

        // Pattern based 0.5f theta (multi-color)
        public static List<ARCObject> Process_Image_UniformPattern(int[][] G)
        {
            List<ARCObject> objects = new List<ARCObject>();
            int[,] input = ImageProcessing.ToMatrix(G);

            foreach (var patchPoints in ImageProcessing.EnumerateMultiColorPatches(input))
            {
                Point TL;
                int[,] patch = ImageProcessing.ListToField(patchPoints, input, out TL);
                var explanations = Process_Patch_UniformPattern(TL, patch, false);
                objects.AddRange(explanations);

                foreach (var other in objects)
                {
                    var contained = objects.Where(exp => other != exp && ContainsWOCoexsits(other, exp)).ToList();
                    if (contained.Count > 0)
                    {
                        other.ContainedObjects = contained;
                        contained.ForEach(exp => exp.ContainedBy = other);
                    }
                }

            }

            
            List<ARCObject> result = new List<ARCObject>();
            foreach (var obj in objects)
            {
                if (objects.All(x => !x.Explains(obj)))
                {
                    result.Add(obj);
                }
            }

            return result;
        }

        private static List<ARCObject> Process_Patch_UniformPattern(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ARCObject> output = new List<ARCObject>();

            foreach (ARCObject obj in CollectObjects_2(topLeft, patch, isNoise))
            {
                output.Add(obj);

                int[,] objVisual = obj.Visual(
                    patch.GetLength(0), patch.GetLength(1),
                    obj.X - topLeft.X, obj.Y - topLeft.Y);

                int[,] overlaps = OverlappingParts2(patch, objVisual);
                int[,] nonOverlaps = NonOverlappingParts2(patch, objVisual);

                // Explain overlaps
                foreach (List<Point> overlapPoints in ImageProcessing.EnumeratePatchesAllowBackground(overlaps))
                {
                    Point overlapTL;
                    int[,] overlapPatch = ImageProcessing.ListToField(overlapPoints, overlaps, out overlapTL);
                    overlapTL.Offset(topLeft);
                    var overlapExp = Process_Patch_Unicolor(overlapTL, overlapPatch, true);
                    overlapExp.ForEach(noise => obj.Noises.Add(noise));
                    overlapExp.ForEach(noise => noise.Parent = obj);
                }

                // Explain residuals
                foreach (List<Point> residualPoints in ImageProcessing.EnumerateMultiColorPatches(nonOverlaps))
                {
                    Point TL;
                    int[,] residualPatch = ImageProcessing.ListToField(residualPoints, nonOverlaps, out TL);
                    TL.Offset(topLeft);
                    var residualExp = Process_Patch_UniformPattern(TL, residualPatch, false);
                    residualExp.ForEach(residual => output.Add(residual));
                }
            }


            return output;
        }

        // Multi-color 1f theta
        public static List<ARCObject> Process_Image_Multicolor(int[][] G)
        {
            List<ARCObject> objects = new List<ARCObject>();
            int[,] input = ImageProcessing.ToMatrix(G);

            foreach (var patchPoints in ImageProcessing.EnumerateMultiColorPatches(input))
            {
                Point TL;
                int[,] patch = ImageProcessing.ListToFieldPointsOnly(patchPoints, input, out TL);
                var explanations = Process_Patch_Unicolor(TL, patch, false);
                objects.AddRange(explanations);

                foreach (var other in objects)
                {
                    var contained = objects.Where(exp => other != exp && ContainsWOCoexsits(other, exp)).ToList();
                    if (contained.Count > 0)
                    {
                        other.ContainedObjects = contained;
                        contained.ForEach(exp => exp.ContainedBy = other);
                    }
                }

            }

            return objects;
        }

        public static List<ARCObject> Process_Image_Multicolor(int[,] G)
        {
            List<ARCObject> objects = new List<ARCObject>();

            foreach (var patchPoints in ImageProcessing.EnumerateMultiColorPatches(G))
            {
                Point TL;
                int[,] patch = ImageProcessing.ListToFieldPointsOnly(patchPoints, G, out TL);
                var explanations = Process_Patch_Unicolor(TL, patch, false);
                objects.AddRange(explanations);

                foreach (var other in objects)
                {
                    var contained = objects.Where(exp => other != exp && ContainsWOCoexsits(other, exp)).ToList();
                    if (contained.Count > 0)
                    {
                        other.ContainedObjects = contained;
                        contained.ForEach(exp => exp.ContainedBy = other);
                    }
                }

            }

            return objects;
        }

        public static List<ARCObject> Process_Image_ComplexPattern(int[][] G)
        {
            List<ARCObject> objects = new List<ARCObject>();
            int[,] input = ImageProcessing.ToMatrix(G);

            Point TL = new Point(0, 0);
            var explanations = Process_Patch_ComplexPattern(TL, input, false);
            objects.AddRange(explanations);

            foreach (var other in objects)
            {
                var contained = objects.Where(exp => other != exp && ContainsWOCoexsits(other, exp)).ToList();
                if (contained.Count > 0)
                {
                    other.ContainedObjects = contained;
                    contained.ForEach(exp => exp.ContainedBy = other);
                }
            }

            List<ARCObject> result = new List<ARCObject>();
            foreach (var obj in objects)
            {
                if (objects.All(x => !x.Explains(obj)))
                {
                    result.Add(obj);
                }
            }

            return result;
        }

        private static List<ARCObject> Process_Patch_ComplexPattern(Point topLeft, int[,] patch, bool isNoise)
        {
            List<ARCObject> output = new List<ARCObject>();

            foreach (ARCObject obj in CollectObjects_ComplexPattern(topLeft, patch, isNoise))
            {
                output.Add(obj);

                int[,] objVisual = obj.Visual(
                    patch.GetLength(0), patch.GetLength(1),
                    obj.X - topLeft.X, obj.Y - topLeft.Y);

                int[,] overlaps = OverlappingParts2(patch, objVisual);
                int[,] nonOverlaps = NonOverlappingParts2(patch, objVisual);

                // Explain overlaps
                foreach (List<Point> overlapPoints in ImageProcessing.EnumeratePatchesAllowBackground(overlaps))
                {
                    Point overlapTL;
                    int[,] overlapPatch = ImageProcessing.ListToField(overlapPoints, overlaps, out overlapTL);
                    overlapTL.Offset(topLeft);
                    var overlapExp = Process_Patch_Unicolor(overlapTL, overlapPatch, true);
                    overlapExp.ForEach(noise => obj.Noises.Add(noise));
                    overlapExp.ForEach(noise => noise.Parent = obj);
                }

                // Explain residuals
                foreach (List<Point> residualPoints in ImageProcessing.EnumerateMultiColorPatches(nonOverlaps))
                {
                    Point TL;
                    int[,] residualPatch = ImageProcessing.ListToField(residualPoints, nonOverlaps, out TL);
                    TL.Offset(topLeft);
                    var residualExp = Process_Patch_Unicolor(TL, residualPatch, false);
                    residualExp.ForEach(residual => output.Add(residual));
                }
            }


            return output;
        }

        private static bool PerfectOverlap(ARCObject obj1, ARCObject obj2)
        {
            return obj1.X == obj2.X && obj1.Y == obj2.Y &&
                   obj1.Width == obj2.Width && obj1.Height == obj2.Height;
        }

        private static bool ContainsWOCoexsits(ARCObject obj1, ARCObject obj2)
        {
            return !PerfectOverlap(obj1, obj2) &&
                   obj1.X <= obj2.X && obj1.Y <= obj2.Y &&
                   obj1.Width >= obj2.Width && obj1.Height >= obj2.Height &&
                   obj2.X <= obj1.X + obj1.Width &&
                   obj2.X + obj2.Width <= obj1.X + obj1.Width &&
                   obj2.Y <= obj1.Y + obj1.Height &&
                   obj2.Y + obj2.Height <= obj1.Y + obj1.Height;
        }

        private static ARCObject TryGroup(Group G, int[,] patch, float theta1, float theta2)
        {
            int[,] slice = G.Slice(patch.GetLength(1), patch.GetLength(0));
            int complexity = Group.ComputeComplexity(slice);
            int[,] regionToColorCount = new int[complexity, 10];
            int[] regionFrequency = new int[complexity];

            int regionCount = 0;
            Dictionary<int, int> globalToLocal = new Dictionary<int, int>();

            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    if (!globalToLocal.ContainsKey(slice[i, j]))
                    {
                        globalToLocal.Add(slice[i, j], regionCount++);
                    }
                    slice[i, j] = globalToLocal[slice[i, j]];
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
                    if (regionToColorCount[i, j] >= regionToColorCount[i, maxIdx])
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
            //predictionScore /= G.complexity;

            if (accuracy >= theta1 && predictionScore >= theta2)
            {
                return new ARCObject(G, regionToColor);
            }

            return null;
        }

        private static ARCObject TryComplexGroup(Group G, int[,] patch, float theta1, float theta2)
        {
            int[,] slice = G.Slice(patch.GetLength(1), patch.GetLength(0));
            int complexity = Group.ComputeComplexity(slice);
            int[,] regionToColorCount = new int[complexity, 10];
            int[] regionFrequency = new int[complexity];

            int regionCount = 0;
            Dictionary<int, int> globalToLocal = new Dictionary<int, int>();

            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    if (!globalToLocal.ContainsKey(slice[i, j]))
                    {
                        globalToLocal.Add(slice[i, j], regionCount++);
                    }
                    slice[i, j] = globalToLocal[slice[i, j]];
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

            int[] noiseProbabilities = new int[10];
            for (int i = 0; i < complexity; i++)
            {
                int maxIdx = 0;
                for (int j = 1; j < 10; j++)
                {
                    if (regionToColorCount[i, j] >= regionToColorCount[i, maxIdx])
                    {
                        maxIdx = j;
                    }
                }

                bool multipleMaxima = false;
                for (int j = 0; j < 10; j++)
                {
                    if (j != maxIdx && regionToColorCount[i, j] == regionToColorCount[i, maxIdx])
                    {
                        multipleMaxima = true;
                    }
                }

                if (multipleMaxima)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        noiseProbabilities[j] += regionToColorCount[i, j];
                    }
                }
                else
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (j != maxIdx)
                        {
                            noiseProbabilities[j] += regionToColorCount[i, j];
                        }
                    }
                }


            }

            int maxNoiseIdx = 0;
            for (int i = 1; i < noiseProbabilities.Length; i++)
            {
                if (noiseProbabilities[i] > noiseProbabilities[maxNoiseIdx])
                {
                    maxNoiseIdx = i;
                }
            }

            for (int i = 0; i < complexity; i++)
            {
                regionToColorCount[i, maxNoiseIdx] = 0;
            }

            int[] regionToColor = new int[complexity];
            for (int i = 0; i < regionToColor.Length; i++)
            {
                int maxIdx = 0;
                for (int j = 1; j < 10; j++)
                {
                    if (regionToColorCount[i, j] >= regionToColorCount[i, maxIdx])
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
            //predictionScore /= G.complexity;

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
                            if (T.ContainsValue(patch[i, j]))
                            {
                                return null;
                            }
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

        private static bool ExpHasOverlaps(int[,] patch, int[,] exp)
        {
            int[,] result = new int[patch.GetLength(0), patch.GetLength(1)];

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    if (patch[i, j] != exp[i, j] && exp[i, j] >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
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

        private static bool PossibleGroupMatch(Group group, int[,] patch)
        {
            for (int i = 0; i < group.field.GetLength(0); i++)
            {
                for (int j = 0; j < group.field.GetLength(1); j++)
                {
                    if (group.field[i, j] == -1 && patch[i, j] > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static int Sum(int[,] patch, bool isNoise)
        {
            int sum = 0;
            for (int i = 0; i < patch.GetLength(0); i++)
            {
                for (int j = 0; j < patch.GetLength(1); j++)
                {
                    if (isNoise && patch[i, j] >= 0)
                    {
                        ++sum;
                    }
                    else if (!isNoise && patch[i, j] > 0)
                    {
                        ++sum;
                    }
                }
            }
            return sum;
        }
    }
}
