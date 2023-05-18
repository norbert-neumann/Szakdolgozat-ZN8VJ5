using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

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
        private static Dictionary<int, List<ARCObject>> NumberMapX = new Dictionary<int, List<ARCObject>>();
        private static Dictionary<int, List<ARCObject>> NumberMapY = new Dictionary<int, List<ARCObject>>();
        private static Dictionary<int, List<ARCObject>> NumberMapWidth = new Dictionary<int, List<ARCObject>>();
        private static Dictionary<int, List<ARCObject>> NumberMapHeight = new Dictionary<int, List<ARCObject>>();
        private static Dictionary<int, List<Pair>> ColorMap = new Dictionary<int, List<Pair>>();

        private static List<ARCObject> Context;

        private static List<Expression> SaturateGroup(ARCObject obj)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            int objIndex = 0;
            foreach (ARCObject X in Context)
            {
                if (X.Group == obj.Group)
                {
                    if (Classifier.ObjectToAnchor[obj] == X)
                    {
                        expressions.Add(string.Format("(group anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.GroupOf,
                            new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    if (Classifier.ObjectToAnchor[obj].Noises.Contains(X))
                    {
                        int n = Classifier.ObjectToAnchor[obj].Noises.IndexOf(X);
                        var nth = ExpressionEngine.Make(ExpressionType.NthNoise, new object[] { ExpressionEngine.Make(Predicate.Anchor), n });
                        exps.Add(ExpressionEngine.Make(ExpressionType.GroupOf, new object[] { nth }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[X])
                    {
                        expressions.Add(string.Format("(group {0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.GroupOf, new object[] { ExpressionEngine.Make(predicate) }));
                    }
                }
                ++objIndex;
            }

            expressions.Add(obj.Group.ToString());
            exps.Add(ExpressionEngine.Make(ExpressionType.ConstGroup, new object[] { obj.Group }));

            return exps;
        }

        private static List<Expression> SaturateNumber(int number, ARCObject y)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            // X references
            if (NumberMapX.ContainsKey(number))
            {
                foreach (var obj in NumberMapX[number])
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(x anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.Xof, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    if (Classifier.ObjectToAnchor[y].Noises.Contains(obj))
                    {
                        int n = Classifier.ObjectToAnchor[y].Noises.IndexOf(obj);
                        expressions.Add(string.Format("(x (nth-noise anchor {0}))", n));
                        var nth = ExpressionEngine.Make(ExpressionType.NthNoise, new object[] { ExpressionEngine.Make(Predicate.Anchor), n});
                        exps.Add(ExpressionEngine.Make(ExpressionType.Xof, new object[] { nth }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(x {0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.Xof, new object[] { ExpressionEngine.Make(predicate) }));
                    }
                }
            }

            // Y references
            if (NumberMapY.ContainsKey(number))
            {
                foreach (var obj in NumberMapY[number])
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(y anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.Yof, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));

                    }

                    if (Classifier.ObjectToAnchor[y].Noises.Contains(obj))
                    {
                        int n = Classifier.ObjectToAnchor[y].Noises.IndexOf(obj);
                        var nth = ExpressionEngine.Make(ExpressionType.NthNoise, new object[] { ExpressionEngine.Make(Predicate.Anchor), n });
                        exps.Add(ExpressionEngine.Make(ExpressionType.Yof, new object[] { nth }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(y {0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.Yof, new object[] { ExpressionEngine.Make(predicate) }));
                    }
                }
            }

            // Width references
            if (NumberMapWidth.ContainsKey(number))
            {
                foreach (var obj in NumberMapWidth[number])
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(width anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.WidthOf, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    if (Classifier.ObjectToAnchor[y].Noises.Contains(obj))
                    {
                        int n = Classifier.ObjectToAnchor[y].Noises.IndexOf(obj);
                        var nth = ExpressionEngine.Make(ExpressionType.NthNoise, new object[] { ExpressionEngine.Make(Predicate.Anchor), n });
                        exps.Add(ExpressionEngine.Make(ExpressionType.WidthOf, new object[] { nth }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(width {0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.WidthOf, new object[] { ExpressionEngine.Make(predicate) }));
                    }
                }
            }

            // Height references
            if (NumberMapHeight.ContainsKey(number))
            {
                foreach (var obj in NumberMapHeight[number])
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(string.Format("(height anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.HeightOf, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    if (Classifier.ObjectToAnchor[y].Noises.Contains(obj))
                    {
                        int n = Classifier.ObjectToAnchor[y].Noises.IndexOf(obj);
                        expressions.Add(string.Format("(height (nth-noise anchor {0}))", n));
                        var nth = ExpressionEngine.Make(ExpressionType.NthNoise, new object[] { ExpressionEngine.Make(Predicate.Anchor), n });
                        exps.Add(ExpressionEngine.Make(ExpressionType.HeightOf, new object[] { nth }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(string.Format("(height {0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.HeightOf, new object[] { ExpressionEngine.Make(predicate) }));
                    }
                }
            }

            // Constant
            expressions.Add(number.ToString());
            exps.Add(ExpressionEngine.Make(ExpressionType.ConstNumber, new object[] { number}));

            return exps;
        }

        private static List<Expression> SaturateColor(int color, ARCObject y)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            foreach (var x in Context)
            {
                if (DSL.MaxColor(x) == color)
                {
                    if (Classifier.ObjectToAnchor[y] == x)
                    {
                        exps.Add(ExpressionEngine.Make(ExpressionType.MaxColor, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[x])
                    {
                        exps.Add(ExpressionEngine.Make(ExpressionType.MaxColor, new object[] { ExpressionEngine.Make(predicate) }));
                    }
                }
            }

            if (ColorMap.ContainsKey(color))
            {
                foreach (var pair in ColorMap[color])
                {
                    if (Classifier.ObjectToAnchor[y] == Context[pair.ObjectIndex])
                    {
                        expressions.Add(string.Format("(color anchor {0})", pair.Region));
                        exps.Add(ExpressionEngine.Make(ExpressionType.ColorOf, new object[] { ExpressionEngine.Make(Predicate.Anchor), pair.Region }));
                    }

                    /*if (ProgramInduction.objToAnchor[y].noises.Contains(context[pair.ObjectIndex]))
                    {
                        int n = ProgramInduction.objToAnchor[y].noises.IndexOf(context[pair.ObjectIndex]);
                        expressions.Add(string.Format("(color (nth-noise anchor n) {0})", pair.Region));
                    }*/

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[Context[pair.ObjectIndex]])
                    {
                        expressions.Add(string.Format("(color {0} {1})",
                                               predicate.ToString(), pair.Region));
                        exps.Add(ExpressionEngine.Make(ExpressionType.ColorOf, new object[] { ExpressionEngine.Make(predicate), pair.Region }));

                    }
                }
            }

            expressions.Add(color.ToString()); //const
            exps.Add(ExpressionEngine.Make(ExpressionType.ConstNumber, new object[] { color}));

            return exps;
        }

        private static List<Expression> SaturateRegionToColor(int[] regionToColor, ARCObject y)
        {
            List<string> expressions = new List<string>();
            List<Expression> exps = new List<Expression>();

            int objIndex = 0;
            foreach (var obj in Context)
            {
                if (regionToColor.SequenceEqual(obj.RegionToColor))
                {
                    if (Classifier.ObjectToAnchor[y] == Context[objIndex])
                    {
                        expressions.Add(string.Format("(color anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.ColorMapOf, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[Context[objIndex]])
                    {
                        expressions.Add(String.Format("(color {0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.ColorMapOf, new object[] { ExpressionEngine.Make(predicate) }));
                    }

                }
                ++objIndex;
            }

            return exps;
        }

        private static List<Expression> SaturateNoiseExpressions(ARCObject noise, ARCObject y)
        {
            List<string> result = new List<string>();
            List<Expression> exps = new List<Expression>();
            int objIndex = 0;

            foreach (var other in Context)
            {
                if (noise.Equals(other))
                {
                     if (Classifier.ObjectToAnchor[y] == Context[objIndex])
                     {
                        result.Add(string.Format("(anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.Object, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                     }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[Context[objIndex]])
                    {
                        result.Add(string.Format("({0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.Object, new object[] { ExpressionEngine.Make(predicate) }));
                    }
                }
                ++objIndex;
            }

            return exps;
        }

        private static List<Expression> SaturateNoises(List<ARCObject> noises, ARCObject y)
        {
            List<string> result = new List<string>();
            List<Expression> exps = new List<Expression>();
            int objIndex = 0;


            foreach (var other in Context)
            {
                if (NoisesMatch(noises, other.Noises))
                {
                    if (Classifier.ObjectToAnchor[y] == Context[objIndex])
                    {
                        result.Add(string.Format("(noise-of anchor)"));
                        exps.Add(ExpressionEngine.Make(ExpressionType.NoiseOf, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[Context[objIndex]])
                    {
                        result.Add(string.Format("(noise-of {0})", predicate.ToString()));
                        exps.Add(ExpressionEngine.Make(ExpressionType.NoiseOf, new object[] { ExpressionEngine.Make(predicate) }));
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
            EquivalenceSaturation.Context = context;

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
                    for (int r = 0; r < X.RegionToColor.Length; r++)
                    {
                        if (X.RegionToColor[r] == i)
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
                Add(X.X, X, NumberMapX);

                Add(X.Y, X, NumberMapY);

                Add(X.Width, X, NumberMapWidth);

                Add(X.Height, X, NumberMapHeight);

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

        public static SuperimposedARCPRogram Saturate(ARCObject obj)
        {
            var groups = SaturateGroup(obj);
            var x = SaturateNumber(obj.X, obj);
            var y = SaturateNumber(obj.Y, obj);
            var w = SaturateNumber(obj.Width, obj);
            var h = SaturateNumber(obj.Height, obj);
            var C = SaturateRegionToColor(obj.RegionToColor, obj);
            var colors = SaturateEachColor(obj.RegionToColor, obj);
            var noiseRefs = SaturateNoises(obj.Noises, obj);
            var noises = obj.Noises.Select(noise => Saturate(noise)).ToList();

            var fg = GenerateFlattenGroupExpressions(obj);
            groups.AddRange(fg);
            groups.AddRange(GenerateRotateGroupExpressions(obj));

            return new SuperimposedARCPRogram(
                groups, x, y, w, h, C, colors, noiseRefs, noises
                );
        }

        private static List<Expression>[] SaturateEachColor(int[] regionToColor, ARCObject y)
        {
            List<Expression>[] colors = new List<Expression>[regionToColor.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = SaturateColor(regionToColor[i], y);
                var dc = GenerateDominantColorExpressions(regionToColor[i], y);
                colors[i].AddRange(dc);
            }

            return colors;
        }

        private static List<Expression> GenerateDominantColorExpressions(int color, ARCObject y)
        {
            List<Expression> expressions = new List<Expression>();

            foreach (var obj in Context)
            {
                if (obj.RegionToColor.Length > 1 && DSL.DominantColor(obj) == color)
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.DominantColor, new object[] { ExpressionEngine.Make(Predicate.Anchor) }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.DominantColor, new object[] { ExpressionEngine.Make(predicate) }));
                    }

                }
            }

            return expressions;
        }
    
        // Flatten (GroupOf (Predicate p))
        private static List<Expression> GenerateFlattenGroupExpressions(ARCObject y)
        {
            List<Expression> expressions = new List<Expression>();

            foreach (var obj in Context)
            {
                if (y.Group.AbstractEquals(DSL.FlattenGroup(obj.Group)))
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.FlattenGroup,
                            new object[]
                            { 
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(Predicate.Anchor)
                                })
                            }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.FlattenGroup, new object[]
                            {
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(predicate)
                                })
                            }));
                    }

                }
            }


            return expressions;
        }

        private static List<Expression> GenerateRotateGroupExpressions(ARCObject y)
        {
            List<Expression> expressions = new List<Expression>();

            foreach (var obj in Context)
            {
                if (y.Group.AbstractEquals(DSL.RotateRigth(obj.Group)))
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.RotateRigth,
                            new object[]
                            {
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(Predicate.Anchor)
                                })
                            }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.RotateRigth, new object[]
                            {
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(predicate)
                                })
                            }));
                    }
                }

                else if (y.Group.AbstractEquals(DSL.RotateLeft(obj.Group)))
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.RotateLeft,
                            new object[]
                            {
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(Predicate.Anchor)
                                })
                            }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.RotateLeft, new object[]
                            {
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(predicate)
                                })
                            }));
                    }
                }
                else if (y.Group.AbstractEquals(DSL.VerticalReflect(obj.Group)))
                {
                    if (Classifier.ObjectToAnchor[y] == obj)
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.VerticalReflect,
                            new object[]
                            {
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(Predicate.Anchor)
                                })
                            }));
                    }

                    foreach (var predicate in PredicateEngine.ObjectToUniquePredicates[obj])
                    {
                        expressions.Add(ExpressionEngine.Make(ExpressionType.VerticalReflect, new object[]
                            {
                                ExpressionEngine.Make(ExpressionType.GroupOf, new object[]
                                {
                                    ExpressionEngine.Make(predicate)
                                })
                            }));
                    }
                }

            }
            return expressions;

        }
    }
}
