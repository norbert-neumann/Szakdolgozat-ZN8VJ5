
namespace SZD_ZN8VJ5
{
    public class SuperimposedARCPRogram
    {
        public List<Expression> Groups = new List<Expression>();
        public List<Expression> Xs = new List<Expression>();
        public List<Expression> Ys = new List<Expression>();
        public List<Expression> Widths = new List<Expression>();
        public List<Expression> Heights = new List<Expression>();
        public List<Expression> ColorMaps = new List<Expression>();
        public List<Expression>[] Colors;
        public List<Expression> NoiseMaps;
        public List<SuperimposedARCPRogram> Noises = new List<SuperimposedARCPRogram>();

        public SuperimposedARCPRogram(List<Expression> groups, List<Expression> xs, List<Expression> ys, List<Expression> widths, List<Expression> heights, List<Expression> colorMaps, List<Expression>[] colors, List<Expression> noiseMaps, List<SuperimposedARCPRogram> noises)
        {
            this.Groups = groups;
            this.Xs = xs;
            this.Ys = ys;
            this.Widths = widths;
            this.Heights = heights;
            this.ColorMaps = colorMaps;
            this.Colors = colors;
            this.NoiseMaps = noiseMaps;
            this.Noises = noises;
        }

        public bool Empty()
        {
            return Groups.Count == 0 || Xs.Count == 0 || Ys.Count == 0 || Widths.Count == 0 || Heights.Count == 0 ||
                (ColorMaps.Count == 0 && (Colors == null || Colors.Any(list => list.Count == 0))) || (NoiseMaps.Count == 0 && Noises.Any(n => n.Empty()));
        }

        public ARCProgram CollapseFirst()
        {
            Expression[] Color = null;
            if (this.Colors != null)
            {
                Color = new Expression[this.Colors.Length];
                for (int i = 0; i < this.Colors.Length; i++)
                {
                    Color[i] = this.Colors[i].FirstOrDefault();
                }
            }

            List<ARCProgram> Noise = new List<ARCProgram>();
            for (int i = 0; i < this.Noises.Count; i++)
            {
                Noise.Add(this.Noises[i].CollapseFirst());
            }

            return ARCProgram.Build(
                this.Groups.First(),
                this.Xs.First(),
                this.Ys.First(),
                this.Widths.First(),
                this.Heights.First(),
                Color,
                this.ColorMaps.FirstOrDefault(),
                Noise,
                this.NoiseMaps.FirstOrDefault()
                );
        }

        public ARCProgram CollapseBestFit()
        {
            Expression[] Color = null;
            if (this.Colors != null)
            {
                Color = new Expression[this.Colors.Length];
                for (int i = 0; i < this.Colors.Length; i++)
                {
                    Color[i] = this.Colors[i].FirstOrDefault();
                }
            }

            List<ARCProgram> Noise = new List<ARCProgram>();
            for (int i = 0; i < this.Noises.Count; i++)
            {
                Noise.Add(this.Noises[i].CollapseBestFit());
            }

            var xExpr = this.Xs.Where(expr => expr.ExpressionIndex == 1).FirstOrDefault();
            if (xExpr == null)
            {
                xExpr = this.Xs.First();
            }

            var yExpr = this.Ys.Where(expr => expr.ExpressionIndex == 2).FirstOrDefault();
            if (yExpr == null)
            {
                yExpr = this.Ys.First();
            }

            var widthExpr = this.Widths.Where(expr => expr.ExpressionIndex == 3).FirstOrDefault();
            if (widthExpr == null)
            {
                widthExpr = this.Widths.First();
            }

            var heightExpr = this.Heights.Where(expr => expr.ExpressionIndex == 4).FirstOrDefault();
            if (heightExpr == null)
            {
                heightExpr = this.Heights.First();
            }

            return ARCProgram.Build(
                this.Groups.First(),
                xExpr,
                yExpr,
                widthExpr,
                heightExpr,
                Color,
                this.ColorMaps.FirstOrDefault(),
                Noise,
                this.NoiseMaps.FirstOrDefault()
                );
        }

        public SuperimposedARCPRogram Intersect(SuperimposedARCPRogram other)
        {
            List<Expression> groups = this.Groups.Intersect(other.Groups).ToList();
            List<Expression> xs = this.Xs.Intersect(other.Xs).ToList();
            List<Expression> ys = this.Ys.Intersect(other.Ys).ToList();
            List<Expression> widths = this.Widths.Intersect(other.Widths).ToList();
            List<Expression> heights = this.Heights.Intersect(other.Heights).ToList();
            List<Expression> colorMaps = this.ColorMaps.Intersect(other.ColorMaps).ToList();
            List<Expression> noiseMaps = this.NoiseMaps.Intersect(other.NoiseMaps).ToList();


            List<Expression>[] colors = null;
            if (this.Colors != null && other.Colors != null && this.Colors.Length == other.Colors.Length)
            {
                colors = new List<Expression>[this.Colors.Length];

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = this.Colors[i].Intersect(other.Colors[i]).ToList();
                }
            }

            List<SuperimposedARCPRogram> noises = new List<SuperimposedARCPRogram>();
            if (this.Noises.Count > 0 && this.Noises.Count == other.Noises.Count)
            {
                for (int i = 0; i < this.Noises.Count; i++)
                {
                    noises.Add(this.Noises[i].Intersect(other.Noises[i]));
                }
            }

            return new SuperimposedARCPRogram(groups, xs, ys, widths, heights, colorMaps, colors, noiseMaps, noises);
        }
    }
}
