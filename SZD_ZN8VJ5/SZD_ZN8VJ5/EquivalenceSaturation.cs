namespace SZD_ZN8VJ5
{
    public struct Pair
    {
        public int Region;
        public int ObjectIndex;

        public Pair(int region, int objectIndex)
        {
            Region = region;
            ObjectIndex = objectIndex;
        }
    }

    public static class EquivalenceSaturation
    {
        public static Dictionary<int, List<ARCObject>> NumberMapX = new Dictionary<int, List<ARCObject>>();
        public static Dictionary<int, List<ARCObject>> NumberMapY = new Dictionary<int, List<ARCObject>>();
        public static Dictionary<int, List<ARCObject>> NumberMapWidth = new Dictionary<int, List<ARCObject>>();
        public static Dictionary<int, List<ARCObject>> NumberMapHeight = new Dictionary<int, List<ARCObject>>();

        public static Dictionary<int, List<Pair>> ColorMap = new Dictionary<int, List<Pair>>();

        public static List<Expression> SaturateGroup(ARCObject obj, List<ARCObject> context)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            int objIndex = 0;
            foreach (ARCObject X in context)
            {
                if (X.Group == obj.Group)
                {
                    if (ProgramInduction.objToAnchor[obj] == X)
                    {
                        expressions.Add(string.Format("(group anchor)"));
                        exps.Add(new Expression(ExpressionType.Group, 0, new object[] { Predicate.Anchor}));
                    }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[X])
                    {
                        expressions.Add(string.Format("(group {0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.Group, 0, new object[] { predicate }));
                    }
                }
                ++objIndex;
            }

            expressions.Add(obj.Group.ToString());

            return exps;
        }

        public static List<Expression> SaturateNumber(int number, ARCObject y)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            // X references
            if (NumberMapX.ContainsKey(number))
            {
                foreach (var obj in NumberMapX[number])
                {
                    if (ProgramInduction.objToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(x anchor)"));
                        exps.Add(new Expression(ExpressionType.Number, 0, new object[] { Predicate.Anchor }));
                    }

                    if (ProgramInduction.objToAnchor[y].noises.Contains(obj))
                    {
                        int n = ProgramInduction.objToAnchor[y].noises.IndexOf(obj);
                        expressions.Add(string.Format("(x (nth-noise anchor {0}))", n));
                        exps.Add(new Expression(ExpressionType.Exp_Int, 0, new object[]
                        {
                            new Expression(ExpressionType.ArcObj_Int, 0, new object[] { Predicate.Anchor, n })
                        }));
                    }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(x {0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.Number, 0, new object[] { predicate}));
                    }
                }
            }

            // Y references
            if (NumberMapY.ContainsKey(number))
            {
                foreach (var obj in NumberMapY[number])
                {
                    if (ProgramInduction.objToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(y anchor)"));
                        exps.Add(new Expression(ExpressionType.Number, 1, new object[] { Predicate.Anchor }));

                    }

                    if (ProgramInduction.objToAnchor[y].noises.Contains(obj))
                    {
                        int n = ProgramInduction.objToAnchor[y].noises.IndexOf(obj);
                        expressions.Add(string.Format("(y (nth-noise anchor {0}))", n));
                        exps.Add(new Expression(ExpressionType.Exp_Int, 1, new object[]
                                                {
                            new Expression(ExpressionType.ArcObj_Int, 0, new object[] { Predicate.Anchor, n })
                                                }));
                    }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(y {0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.Number, 1, new object[] { predicate }));
                    }
                }
            }

            // Width references
            if (NumberMapWidth.ContainsKey(number))
            {
                foreach (var obj in NumberMapWidth[number])
                {
                    if (ProgramInduction.objToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(width anchor)"));
                        exps.Add(new Expression(ExpressionType.Number, 2, new object[] { Predicate.Anchor}));
                    }

                    if (ProgramInduction.objToAnchor[y].noises.Contains(obj))
                    {
                        int n = ProgramInduction.objToAnchor[y].noises.IndexOf(obj);
                        expressions.Add(string.Format("(width (nth-noise anchor {0}))", n));
                        exps.Add(new Expression(ExpressionType.Exp_Int, 2, new object[]
                                                {
                            new Expression(ExpressionType.ArcObj_Int, 0, new object[] { Predicate.Anchor, n })
                                                }));
                    }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(width {0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.Number, 2, new object[] { predicate }));
                    }
                }
            }

            // Height references
            if (NumberMapHeight.ContainsKey(number))
            {
                foreach (var obj in NumberMapHeight[number])
                {
                    if (ProgramInduction.objToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(height anchor)"));
                        exps.Add(new Expression(ExpressionType.Number, 3, new object[] { Predicate.Anchor }));
                    }

                    if (ProgramInduction.objToAnchor[y].noises.Contains(obj))
                    {
                        int n = ProgramInduction.objToAnchor[y].noises.IndexOf(obj);
                        expressions.Add(string.Format("(height (nth-noise anchor {0}))", n));
                        exps.Add(new Expression(ExpressionType.Exp_Int, 3, new object[]
                                                {
                            new Expression(ExpressionType.ArcObj_Int, 0, new object[] { Predicate.Anchor, n })
                                                }));
                    }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(height {0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.Number, 3, new object[] { predicate}));
                    }
                }
            }

            // Constant
            expressions.Add(number.ToString());
            exps.Add(new Expression(ExpressionType.ConstNumber, -1, new object[] { number }));

            return exps;
        }

        public static List<Expression> SaturateColor(int color, List<ARCObject> context, ARCObject y)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            if (ColorMap.ContainsKey(color))
            {
                foreach (var pair in ColorMap[color])
                {
                    if (ProgramInduction.objToAnchor[y] == context[pair.ObjectIndex])
                    {
                        expressions.Add(string.Format("(color anchor {0})", pair.Region));
                        exps.Add(new Expression(ExpressionType.IntNumber, 0, new object[] {
                            Predicate.Anchor,
                            pair.Region
                        }));
                    }

                    /*if (ProgramInduction.objToAnchor[y].noises.Contains(context[pair.ObjectIndex]))
                    {
                        int n = ProgramInduction.objToAnchor[y].noises.IndexOf(context[pair.ObjectIndex]);
                        expressions.Add(string.Format("(color (nth-noise anchor n) {0})", pair.Region));
                    }*/

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[context[pair.ObjectIndex]])
                    {
                        expressions.Add(string.Format("(color {0} {1})",
                                               predicate.ToString(), pair.Region));
                        exps.Add(new Expression(ExpressionType.IntNumber, 0, new object[] {
                            predicate,
                            pair.Region
                        }));
                    }
                }
            }

            expressions.Add(color.ToString()); //const
            exps.Add(new Expression(ExpressionType.ConstNumber, 0, new object[] { color }));

            return exps;
        }

        public static List<Expression> SaturateRegionToColor(int[] regionToColor, List<ARCObject> context, ARCObject y)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            int objIndex = 0;
            foreach (var obj in context)
            {
                if (regionToColor.SequenceEqual(obj.regionToColor))
                {
                    if (ProgramInduction.objToAnchor[y] == context[objIndex])
                    {
                        expressions.Add(string.Format("(color anchor)"));
                        exps.Add(new Expression(ExpressionType.IntArr, 0, new object[] {
                            Predicate.Anchor
                        }));
                    }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[context[objIndex]])
                    {
                        expressions.Add(String.Format("(color {0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.IntArr, 0, new object[] {
                            predicate
                        }));
                    }

                }
                ++objIndex;
            }

            return exps;
        }

        public static List<Expression> SaturateNoiseExpressions(ARCObject noise, List<ARCObject> context, ARCObject y)
        {
            List<string> result = new List<string>();
            List<Expression> exps = new List<Expression>();
            int objIndex = 0;

            foreach (var other in context)
            {
                if (noise.Equals(other))
                {
                     if (ProgramInduction.objToAnchor[y] == context[objIndex])
                     {
                         result.Add(string.Format("(anchor)"));
                        exps.Add(new Expression(ExpressionType.ArcObj, 0, new object[] { Predicate.Anchor }));
                     }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[context[objIndex]])
                    {
                        result.Add(string.Format("({0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.ArcObj, 0, new object[] { predicate }));
                    }
                }
                ++objIndex;
            }

            return exps;
        }

        public static List<Expression> SaturateNoises(List<ARCObject> noises, List<ARCObject> context, ARCObject y)
        {
            List<string> result = new List<string>();
            List<Expression> exps = new List<Expression>();
            int objIndex = 0;


            foreach (var other in context)
            {
                if (NoisesMatch(noises, other.noises))
                {
                    if (ProgramInduction.objToAnchor[y] == context[objIndex])
                    {
                        result.Add(string.Format("(noise-of anchor)"));
                        exps.Add(new Expression(ExpressionType.ARCObjArr, 0, new object[] { Predicate.Anchor }));
                    }

                    foreach (var predicate in PredicateEngine.objToUniquePredicates[context[objIndex]])
                    {
                        result.Add(string.Format("(noise-of {0})", predicate.ToString()));
                        exps.Add(new Expression(ExpressionType.ARCObjArr, 0, new object[] { predicate }));
                    }
                }
                ++objIndex;
            }

            return exps;
        }

        private static bool NoisesMatch(List<ARCObject> N1, List<ARCObject> N2)
        {
            return N1.Count == N2.Count && N1.Count > 0 && N1.All(noise => N2.Any(other => noise.VisualEquals(other)));
        }

        public static void Update(List<ARCObject> context)
        {
            // Update color
            for (int i = 0; i < 10; i++)
            {
                if (!ColorMap.ContainsKey(i))
                {
                    ColorMap.Add(i, new List<Pair>());
                }
                else
                {
                    ColorMap[i] = new List<Pair>();
                }

                int objIndex = 0;
                foreach (ARCObject X in context)
                {
                    for (int r = 0; r < X.regionToColor.Length; r++)
                    {
                        if (X.regionToColor[r] == i)
                        {
                            ColorMap[i].Add(new Pair(r, objIndex));
                        }
                    }
                    ++objIndex;
                }
            }

            // Update numbers
            NumberMapX.Clear();
            NumberMapY.Clear();
            NumberMapWidth.Clear();
            NumberMapHeight.Clear();

            int objectIndex = 0;
            foreach (ARCObject X in context)
            {
                Add(X.x, X, NumberMapX);

                Add(X.y, X, NumberMapY);

                Add(X.width, X, NumberMapWidth);

                Add(X.height, X, NumberMapHeight);

                ++objectIndex;
            }
        }

        private static void Add(int key, ARCObject value, Dictionary<int, List<ARCObject>> map)
        {
            if (!map.ContainsKey(key))
            {
                map.Add(key, new List<ARCObject>());
            }
            map[key].Add(value);
        }

        public static SuperimposedARCPRogram Saturate(ARCObject obj, List<ARCObject> context)
        {
            var groups = SaturateGroup(obj, context);
            var x = SaturateNumber(obj.x, obj);
            var y = SaturateNumber(obj.y, obj);
            var w = SaturateNumber(obj.width, obj);
            var h = SaturateNumber(obj.height, obj);
            var C = SaturateRegionToColor(obj.regionToColor, context, obj);
            var colors = SaturateEachColor(obj.regionToColor, context, obj);
            var noiseRefs = SaturateNoises(obj.noises, context, obj);
            var noises = obj.noises.Select(noise => Saturate(noise, context)).ToList();

            return new SuperimposedARCPRogram(
                groups, x, y, w, h, C, colors, noiseRefs, noises
                );
        }

        private static List<Expression>[] SaturateEachColor(int[] regionToColor, List<ARCObject> context, ARCObject y)
        {
            List<Expression>[] colors = new List<Expression>[regionToColor.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = SaturateColor(regionToColor[i], context, y);
            }

            return colors;
        }

        private static string[] Parse(string text)
        {
            text = text.Replace("(", " ( ");
            text = text.Replace(")", " ) ");
            text = text.Replace("  ", " ");
            text = text.Replace("  ", " ");
            return text.Trim().Split();
        }
    }
}
