
namespace SZD_ZN8VJ5
{
    public class DescriptionFrequency
    {
        public int[][] SUM;
        public int[][] COLORS;
        public DescriptionFrequency[] NOISES;

        public List<ARCObject> members = new List<ARCObject>();

        public DescriptionFrequency(SuperimposedARCPRogram program)
        {
            SUM = new int[7][];
            SUM[0] = new int[program.groups.Count];
            SUM[1] = new int[program.xs.Count];
            SUM[2] = new int[program.ys.Count];
            SUM[3] = new int[program.widths.Count];
            SUM[4] = new int[program.heights.Count];
            SUM[5] = new int[program.colorMaps.Count];
            SUM[6] = new int[program.noiseMaps.Count];

            COLORS = new int[program.colors.Length][];
            for (int i = 0; i < COLORS.Length; i++)
            {
                COLORS[i] = new int[program.colors[i].Count];
            }

            NOISES = new DescriptionFrequency[program.noises.Count];
            for (int i = 0; i < NOISES.Length; i++)
            {
                NOISES[i] = new DescriptionFrequency(program.noises[i]);
            }
        }
    }

    public static class ProgramInduction
    {
        public static Dictionary<ARCObject, ARCObject> objToAnchor = new Dictionary<ARCObject, ARCObject>();

        public static void SetAnhors(List<ARCObject> X, List<ARCObject> Y)
        {
            foreach (var obj in Y)
            {
                objToAnchor.Add(obj, FindAnchorObject(X, obj));
            }
        }

        private static List<Expression[]> IndiciesOf(int[] frequencySum, List<Expression[]> expressions)
        {
            int max = frequencySum.Max();
            var indicies = Enumerable.Range(0, frequencySum.Length).Where(idx => frequencySum[idx] == max);
            return indicies.Select(idx => expressions[idx]).ToList();
        }

        private static List<Expression> IndiciesOf(int[] frequencySum, List<Expression> expressions)
        {
            int max = frequencySum.Max();
            var indicies = Enumerable.Range(0, frequencySum.Length).Where(idx => frequencySum[idx] == max);
            return indicies.Select(idx => expressions[idx]).ToList();
        }

        private static List<ARCObject> FullExplanation(List<ARCObject> objects, List<ARCObject> explained)
        {
            List<ARCObject> full = new List<ARCObject>(objects);

            foreach (var group in Helpers.Explainings.Keys)
            {
                if (objects.Intersect(group).Count() == group.Count())
                {
                    full.AddRange(Helpers.Explainings[group]);
                }
            }

            full = full.Distinct().ToList();
            var tmp = full.Except(explained).ToList();

            if (tmp.Count > 0)
            {
                return tmp.Union(FullExplanation(tmp, full)).ToList();
            }
            else
            {
                return tmp;
            }
        }

        private static void Match(SuperimposedARCPRogram P1, SuperimposedARCPRogram P2, DescriptionFrequency frequency)
        {
            var intersection = P1.Intersect(P2);

            if (intersection.Empty())
            {
                return;
            }

            foreach (var expr in intersection.groups)
            {
                int index = P1.groups.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[0][index]++;
                }
            }

            foreach (var expr in intersection.xs)
            {
                int index = P1.xs.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[1][index]++;
                }
            }

            foreach (var expr in intersection.ys)
            {
                int index = P1.ys.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[2][index]++;
                }
            }

            foreach (var expr in intersection.widths)
            {
                int index = P1.widths.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[3][index]++;
                }
            }

            foreach (var expr in intersection.heights)
            {
                int index = P1.heights.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[4][index]++;
                }
            }

            foreach (var expr in intersection.colorMaps)
            {
                int index = P1.colorMaps.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[5][index]++;
                }
            }

            foreach (var expr in intersection.noiseMaps)
            {
                int index = P1.noiseMaps.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[6][index]++;
                }
            }

            if (P1.colors.Length == P2.colors.Length)
            {
                for (int r = 0; r < intersection.colors.Length; r++)
                {
                    foreach (var colorExpr in intersection.colors[r])
                    {
                        int index = P1.colors[r].IndexOf(colorExpr);
                        if (index >= 0)
                        {
                            frequency.COLORS[r][index]++;
                        }
                    }
                }
            }

            if (P1.noises.Count == P2.noises.Count)
            {
                for (int n = 0; n < P1.noises.Count; n++)
                {
                    Match(P1.noises[n], P2.noises[n], frequency.NOISES[n]);
                }
            }

        }

        private static (SuperimposedARCPRogram, int) SelectBestSIProgram(SuperimposedARCPRogram program, DescriptionFrequency frequency)
        {
            int[] maxValues = new int[7];
            for (int i = 0; i < 5; i++)
            {
                maxValues[i] = frequency.SUM[i].Max();
            }

            var indicies = Enumerable.Range(0, frequency.SUM[0].Length).Where(idx => frequency.SUM[0][idx] == frequency.SUM[0].Max());

            List<Expression> groupExpr = IndiciesOf(frequency.SUM[0], program.groups);
            List<Expression> xExpr = IndiciesOf(frequency.SUM[1], program.xs);
            List<Expression> yExpr = IndiciesOf(frequency.SUM[2], program.ys);
            List<Expression> widthExpr = IndiciesOf(frequency.SUM[3], program.widths);
            List<Expression> heightExpr = IndiciesOf(frequency.SUM[4], program.heights);

            List<Expression> colorExpr = new List<Expression>();
            List<Expression> noiseExpr = new List<Expression>();

            List<Expression>[] colors = new List<Expression>[frequency.COLORS.Length];
            List<SuperimposedARCPRogram> noise = new List<SuperimposedARCPRogram>();

            int colorsMax = int.MaxValue;
            for (int i = 0; i < program.colors.Length; i++)
            {
                int max = frequency.COLORS[i].Max();
                colors[i] = IndiciesOf(frequency.COLORS[i], program.colors[i]);
                if (colorsMax > max)
                {
                    colorsMax = max;
                }
            }
            maxValues[5] = colorsMax;

            if (frequency.SUM[5].Length > 0)
            {
                int colorMapMax = frequency.SUM[5].Max();
                if (colorMapMax >= colorsMax)
                {
                    colorExpr = IndiciesOf(frequency.SUM[5], program.colorMaps);
                    maxValues[5] = colorMapMax;
                }
            }

            if (program.noises.Count > 0)
            {
                int noisesMax = int.MaxValue;
                for (int i = 0; i < program.noises.Count; i++)
                {
                    var (noiseProgram, max) = SelectBestSIProgram(program.noises[i], frequency.NOISES[i]);
                    noise.Add(noiseProgram);
                    if (noisesMax > max)
                    {
                        noisesMax = max;
                    }
                }

                maxValues[6] = noisesMax;

                if (frequency.SUM[6].Length > 0)
                {
                    int noiseMapMax = frequency.SUM[6].Max();
                    if (noiseMapMax >= noisesMax)
                    {
                        noiseExpr = IndiciesOf(frequency.SUM[6], program.noiseMaps); ;
                        maxValues[6] = noiseMapMax;
                    }
                }
            }

            return (new SuperimposedARCPRogram(
                groupExpr,
                xExpr,
                yExpr,
                widthExpr,
                heightExpr,
                colorExpr,
                colors,
                noiseExpr,
                noise
                ), maxValues.Max());
        }

        public static ARCObject FindAnchorObject(List<ARCObject> X, ARCObject y)
        {
            var scores = X.Select(x => AnchorScore(x, y)).ToList();
            if (y.noiseTo != null)
            {
                return objToAnchor[y.noiseTo];
            }
            int max = X.Max(x => AnchorScore(x, y));
            return X.First(x => AnchorScore(x, y) == max);
        }

        // We might include rot(group), shift(x), etc.
        private static int AnchorScore(ARCObject x, ARCObject y)
        {
            int score = 0;

            if (x.Group.AbstractEquals(y.Group))
            {
                score += 5;
            }
            if (x.noises.TrueForAll(nx => y.noises.Exists(ny =>
                nx.x - x.x == ny.x - y.x && nx.y - x.y == ny.y - y.y && nx.Group.AbstractEquals(ny.Group)                 )))
            {
                score += 10;
            }
            if (x.x == y.x)
            {
                score += 3;
            }
            if (x.y == y.y)
            {
                score += 3;
            }
            if (x.width == y.width)
            {
                score += 3;
            }
            if (x.height == y.height)
            {
                score += 3;
            }

            return score;
        }

        public static List<SIEquivalenceClass> TestOuter(List<SuperimposedARCPRogram> programs, List<ARCObject> objects)
        {
            // TEMP ONLY
            if (programs.Count > 100)
            {
                throw new Exception();
            }

            List<ARCObject> explained = new List<ARCObject>();
            List<ARCObject> unexplained = objects.ToList();
            List<SIEquivalenceClass> eqClasses = new List<SIEquivalenceClass>();

            List<SuperimposedARCPRogram> copy = programs.ToList();

            while (programs.Count > 0)
            {
                if (objects.First().objectIndex == 17)
                {

                }

                var best = TestInduce(copy, programs.First());
                bool classFound = false;

                foreach (var eqClass in eqClasses)
                {
                    if (eqClass.TryIntersect(best))
                    {
                        classFound = true;
                        eqClass.elements.Add(objects.First());
                        break;
                    }
                }

                if (!classFound)
                {
                    eqClasses.Add(new SIEquivalenceClass(objects.First(), best, 0));
                }

                objects.Remove(objects.First());
                programs.Remove(programs.First());
            }

            List<SIEquivalenceClass> winners = new List<SIEquivalenceClass>();

            while (unexplained.Count > 0)
            {
                var bestClass = eqClasses.OrderByDescending(eqClass => FullExplanation(eqClass.elements, explained).Count).First();
                var exp = FullExplanation(bestClass.elements, explained);
                explained.AddRange(exp);
                exp.ForEach(obj => unexplained.Remove(obj));
                winners.Add(bestClass);
            }

            return winners;
        }

        public static SuperimposedARCPRogram TestInduce(List<SuperimposedARCPRogram> programs, SuperimposedARCPRogram current)
        {
            DescriptionFrequency frequency = new DescriptionFrequency(current);
            foreach (var program in programs)
            {
                if (program != current)
                {
                    Match(current, program, frequency);
                }
            }
            var (best, _) = SelectBestSIProgram(current, frequency);
            return best;
        }
    
        public static List<Predicate> GetAnchorPredicates(List<ARCObject> positives, List<ARCObject> negatives)
        {
            List<Predicate> predicates = new List<Predicate>();
            predicates.AddRange(PredicateEngine.objToPredicates[positives.First()]);

            foreach (var positive in positives)
            {
                if (positive != positives.First())
                {
                    /*var test = PredicateEngine.objToPredicates[positive];
                    if (predicates.Count > 0 && test.Count > 0 && predicates.Last().Equals(test.Last()))
                    {

                    }*/

                    predicates = PredicateEngine.objToPredicates[positive].Where(pred => predicates.Contains(pred)).ToList();
                }
            }

            List<Predicate> antiPredicates = new List<Predicate>();
            negatives.ForEach(obj => antiPredicates.AddRange(PredicateEngine.objToPredicates[obj]));

            return predicates.Distinct().Where(pred => !antiPredicates.Contains(pred)).ToList();
            //return predicates.Distinct().Except(antiPredicates.Distinct(), ).ToList();
        }

        public static bool IsInversePredicate(List<ARCObject> positives, List<ARCObject> negatives, List<Predicate> predicates)
        {
            var ps = predicates.Where(pred => positives.TrueForAll(obj => !PredicateEngine.objToPredicates[obj].Contains(pred)));

            if (ps.Count() > 0)
            {
                return predicates.TrueForAll(pred => negatives.TrueForAll(obj => PredicateEngine.objToPredicates[obj].Contains(pred)));
            }

            return false;
        }

        private static List<Predicate> Invert(List<Predicate> predicates)
        {
            List<Predicate> inverted = new List<Predicate>();

            foreach (var pred in predicates)
            {
                inverted.Add(new Predicate(pred));
                inverted.Last().Inverse = true;
            }

            return inverted;
        }

        public static List<Predicate>[] FindRolePredicates(List<ARCObject> all, List<ARCObject>[] roles)
        {
            // X anchors
            List<Predicate>[] rolePredicates = new List<Predicate>[roles.Length];

            // Y -> X
            List<ARCObject>[] roleAnchors = roles.Select(role => role.Select(obj => objToAnchor[obj]).ToList()).ToArray();

            for (int i = 0; i < roles.Length; i++)
            {
                var negatives = all.Except(roleAnchors[i]).ToList();
                var predicates = GetAnchorPredicates(roleAnchors[i], negatives); // true for all positives and false for all negatives
                rolePredicates[i] = predicates;
            }


            for (int i = 0; i < roles.Length; i++)
            {
                if (rolePredicates[i].Count == 0)
                {
                    var negatives = all.Except(roleAnchors[i]).ToList();

                    for (int j = 0; j < roles.Length; j++)
                    {
                        if (rolePredicates[j].Count > 0 && IsInversePredicate(roleAnchors[i], negatives, rolePredicates[j]))
                        {
                            rolePredicates[i] = Invert(rolePredicates[j]);
                            break;
                        }
                    }
                }
            }

            return rolePredicates;
        }
    }
}