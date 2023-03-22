
namespace SZD_ZN8VJ5
{
    public class SuperimposedARCPRogram
    {
        public List<Expression> groups = new List<Expression>();
        public List<Expression> xs = new List<Expression>();
        public List<Expression> ys = new List<Expression>();
        public List<Expression> widths = new List<Expression>();
        public List<Expression> heights = new List<Expression>();
        public List<Expression> colorMaps = new List<Expression>();
        public List<Expression>[] colors;
        public List<Expression> noiseMaps;
        public List<SuperimposedARCPRogram> noises = new List<SuperimposedARCPRogram>();

        public SuperimposedARCPRogram(List<Expression> groups, List<Expression> xs, List<Expression> ys, List<Expression> widths, List<Expression> heights, List<Expression> colorMaps, List<Expression>[] colors, List<Expression> noiseMaps, List<SuperimposedARCPRogram> noises)
        {
            this.groups = groups;
            this.xs = xs;
            this.ys = ys;
            this.widths = widths;
            this.heights = heights;
            this.colorMaps = colorMaps;
            this.colors = colors;
            this.noiseMaps = noiseMaps;
            this.noises = noises;
        }

        public bool Empty()
        {
            return groups.Count == 0 || xs.Count == 0 || ys.Count == 0 || widths.Count == 0 || heights.Count == 0 ||
                (colorMaps.Count == 0 && (colors == null || colors.Any(list => list.Count == 0))) || (noiseMaps.Count == 0 && noises.Any(n => n.Empty()));
        }

        public ARCProgram CollapseFirst()
        {
            Expression[] Color = null;
            if (this.colors != null)
            {
                Color = new Expression[this.colors.Length];
                for (int i = 0; i < this.colors.Length; i++)
                {
                    Color[i] = this.colors[i].FirstOrDefault();
                }
            }

            List<ARCProgram> Noise = new List<ARCProgram>();
            for (int i = 0; i < this.noises.Count; i++)
            {
                Noise.Add(this.noises[i].CollapseFirst());
            }

            return ARCProgram.Build(
                this.groups.First(),
                this.xs.First(),
                this.ys.First(),
                this.widths.First(),
                this.heights.First(),
                Color,
                this.colorMaps.FirstOrDefault(),
                Noise,
                this.noiseMaps.FirstOrDefault()
                );
        }

        public SuperimposedARCPRogram Intersect(SuperimposedARCPRogram other)
        {
            List<Expression> groups = this.groups.Intersect(other.groups).ToList();
            List<Expression> xs = this.xs.Intersect(other.xs).ToList();
            List<Expression> ys = this.ys.Intersect(other.ys).ToList();
            List<Expression> widths = this.widths.Intersect(other.widths).ToList();
            List<Expression> heights = this.heights.Intersect(other.heights).ToList();
            List<Expression> colorMaps = this.colorMaps.Intersect(other.colorMaps).ToList();
            List<Expression> noiseMaps = this.noiseMaps.Intersect(other.noiseMaps).ToList();


            List<Expression>[] colors = null;
            if (this.colors != null && other.colors != null && this.colors.Length == other.colors.Length)
            {
                colors = new List<Expression>[this.colors.Length];

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = this.colors[i].Intersect(other.colors[i]).ToList();
                }
            }

            List<SuperimposedARCPRogram> noises = new List<SuperimposedARCPRogram>();
            if (this.noises.Count > 0 && this.noises.Count == other.noises.Count)
            {
                for (int i = 0; i < this.noises.Count; i++)
                {
                    noises.Add(this.noises[i].Intersect(other.noises[i]));
                }
            }

            return new SuperimposedARCPRogram(groups, xs, ys, widths, heights, colorMaps, colors, noiseMaps, noises);
        }
    }
}
