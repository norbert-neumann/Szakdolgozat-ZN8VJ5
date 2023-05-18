
namespace SZD_ZN8VJ5
{
    public static class ProgramInduction
    {
        private class DescriptionFrequency
        {
            public int[][] SUM;
            public int[][] COLORS;
            public DescriptionFrequency[] NOISES;

            public List<ARCObject> members = new List<ARCObject>();

            public DescriptionFrequency(SuperimposedARCPRogram program)
            {
                SUM = new int[7][];
                SUM[0] = new int[program.Groups.Count];
                SUM[1] = new int[program.Xs.Count];
                SUM[2] = new int[program.Ys.Count];
                SUM[3] = new int[program.Widths.Count];
                SUM[4] = new int[program.Heights.Count];
                SUM[5] = new int[program.ColorMaps.Count];
                SUM[6] = new int[program.NoiseMaps.Count];

                COLORS = new int[program.Colors.Length][];
                for (int i = 0; i < COLORS.Length; i++)
                {
                    COLORS[i] = new int[program.Colors[i].Count];
                }

                NOISES = new DescriptionFrequency[program.Noises.Count];
                for (int i = 0; i < NOISES.Length; i++)
                {
                    NOISES[i] = new DescriptionFrequency(program.Noises[i]);
                }
            }
        }

        public static List<SIEquivalenceClass> Induce(List<SuperimposedARCPRogram> programs, List<ARCObject> objects)
        {
            // TEMP ONLY
            if (programs.Count > 100)
            {
                throw new InvalidDataException();
            }

            List<ARCObject> explained = new List<ARCObject>();
            List<ARCObject> unexplained = objects.ToList();
            List<SIEquivalenceClass> eqClasses = new List<SIEquivalenceClass>();

            List<SuperimposedARCPRogram> copy = programs.ToList();

            while (programs.Count > 0)
            {
                var best = TestInduce(copy, programs.First());
                bool classFound = false;

                foreach (var eqClass in eqClasses)
                {
                    if (eqClass.TryIntersect(best))
                    {
                        classFound = true;
                        eqClass.Elements.Add(objects.First());
                        break;
                    }
                }

                if (!classFound)
                {
                    eqClasses.Add(new SIEquivalenceClass(objects.First(), best));
                }

                objects.Remove(objects.First());
                programs.Remove(programs.First());
            }

            List<SIEquivalenceClass> winners = new List<SIEquivalenceClass>();

            while (unexplained.Count > 0)
            {
                var bestClass = eqClasses.OrderByDescending(eqClass => eqClass.Elements.Except(explained).Count()).First();
                explained.AddRange(bestClass.Elements);
                bestClass.Elements.ForEach(obj => unexplained.Remove(obj));
                winners.Add(bestClass);
            }

            return winners;
        }

        private static List<Expression> IndiciesOf(int[] frequencySum, List<Expression> expressions)
        {
            int max = frequencySum.Max();
            var indicies = Enumerable.Range(0, frequencySum.Length).Where(idx => frequencySum[idx] == max);
            return indicies.Select(idx => expressions[idx]).ToList();
        }

        private static void Match(SuperimposedARCPRogram P1, SuperimposedARCPRogram P2, DescriptionFrequency frequency)
        {
            var intersection = P1.Intersect(P2);

            if (intersection.Empty())
            {
                return;
            }

            foreach (var expr in intersection.Groups)
            {
                int index = P1.Groups.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[0][index]++;
                }
            }

            foreach (var expr in intersection.Xs)
            {
                int index = P1.Xs.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[1][index]++;
                }
            }

            foreach (var expr in intersection.Ys)
            {
                int index = P1.Ys.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[2][index]++;
                }
            }

            foreach (var expr in intersection.Widths)
            {
                int index = P1.Widths.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[3][index]++;
                }
            }

            foreach (var expr in intersection.Heights)
            {
                int index = P1.Heights.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[4][index]++;
                }
            }

            foreach (var expr in intersection.ColorMaps)
            {
                int index = P1.ColorMaps.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[5][index]++;
                }
            }

            foreach (var expr in intersection.NoiseMaps)
            {
                int index = P1.NoiseMaps.IndexOf(expr);
                if (index >= 0)
                {
                    frequency.SUM[6][index]++;
                }
            }

            if (P1.Colors.Length == P2.Colors.Length)
            {
                for (int r = 0; r < intersection.Colors.Length; r++)
                {
                    foreach (var colorExpr in intersection.Colors[r])
                    {
                        int index = P1.Colors[r].IndexOf(colorExpr);
                        if (index >= 0)
                        {
                            frequency.COLORS[r][index]++;
                        }
                    }
                }
            }

            if (P1.Noises.Count == P2.Noises.Count)
            {
                for (int n = 0; n < P1.Noises.Count; n++)
                {
                    Match(P1.Noises[n], P2.Noises[n], frequency.NOISES[n]);
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

            List<Expression> groupExpr = IndiciesOf(frequency.SUM[0], program.Groups);
            List<Expression> xExpr = IndiciesOf(frequency.SUM[1], program.Xs);
            List<Expression> yExpr = IndiciesOf(frequency.SUM[2], program.Ys);
            List<Expression> widthExpr = IndiciesOf(frequency.SUM[3], program.Widths);
            List<Expression> heightExpr = IndiciesOf(frequency.SUM[4], program.Heights);

            List<Expression> colorExpr = new List<Expression>();
            List<Expression> noiseExpr = new List<Expression>();

            List<Expression>[] colors = new List<Expression>[frequency.COLORS.Length];
            List<SuperimposedARCPRogram> noise = new List<SuperimposedARCPRogram>();

            int colorsMax = int.MaxValue;
            for (int i = 0; i < program.Colors.Length; i++)
            {
                int max = frequency.COLORS[i].Max();
                colors[i] = IndiciesOf(frequency.COLORS[i], program.Colors[i]);
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
                    colorExpr = IndiciesOf(frequency.SUM[5], program.ColorMaps);
                    maxValues[5] = colorMapMax;
                }
            }

            if (program.Noises.Count > 0)
            {
                int noisesMax = int.MaxValue;
                for (int i = 0; i < program.Noises.Count; i++)
                {
                    var (noiseProgram, max) = SelectBestSIProgram(program.Noises[i], frequency.NOISES[i]);
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
                        noiseExpr = IndiciesOf(frequency.SUM[6], program.NoiseMaps); ;
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

        private static SuperimposedARCPRogram TestInduce(List<SuperimposedARCPRogram> programs, SuperimposedARCPRogram current)
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
    }
}