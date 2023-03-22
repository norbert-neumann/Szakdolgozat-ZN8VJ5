
namespace SZD_ZN8VJ5
{
    public class ARCProgram
    {
        public Expression Group;
        public Expression X;
        public Expression Y;
        public Expression Width;
        public Expression Height;
        public Expression[] Color;
        public List<ARCProgram> Noise;

        public Expression NoiseExpr;
        public Expression ColorExpr;

        public static ARCProgram Build(Expression Group, Expression X, Expression Y, Expression Width, Expression Height,
            Expression[] Color, Expression ColorExpr, List<ARCProgram> Noise, Expression NoiseExpr)
        {
            // Has noise
            if (Noise.Count > 0 || NoiseExpr != null)
            {
                if (NoiseExpr != null)
                {
                    if (ColorExpr != null)
                    {
                        return new ARCProgram(Group, X, Y, Width, Height, NoiseExpr, ColorExpr);
                    }
                    else
                    {
                        return new ARCProgram(Group, X, Y, Width, Height, Color, NoiseExpr);
                    }
                }
                else
                {
                    if (ColorExpr != null)
                    {
                        return new ARCProgram(Group, X, Y, Width, Height, Noise, ColorExpr);
                    }
                    else
                    {
                        return new ARCProgram(Group, X, Y, Width, Height, Color, Noise);
                    }
                }
            }
            // No noise
            else
            {
                if (ColorExpr != null)
                {
                    return new ARCProgram(Group, X, Y, Width, Height, ColorExpr);
                }
                else
                {
                    return new ARCProgram(Group, X, Y, Width, Height, Color);
                }
            }
        }

        public ARCProgram(Expression group, Expression x, Expression y, Expression width, Expression height, Expression[] color, List<ARCProgram> noise)
        {
            Group = group;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = color;
            Noise = noise;
        }

        public ARCProgram(Expression group, Expression x, Expression y, Expression width, Expression height, Expression[] color, Expression noiseExpr)
        {
            Group = group;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = color;
            NoiseExpr = noiseExpr;
        }

        public ARCProgram(Expression group, Expression x, Expression y, Expression width, Expression height, Expression noiseExpr, Expression colorExpr)
        {
            Group = group;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            NoiseExpr = noiseExpr;
            ColorExpr = colorExpr;
        }

        public ARCProgram(Expression group, Expression x, Expression y, Expression width, Expression height, List<ARCProgram> noise, Expression colorExpr)
        {
            Group = group;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Noise = noise;
            ColorExpr = colorExpr;
        }

        public ARCProgram(Expression group, Expression x, Expression y, Expression width, Expression height, Expression colorExpr)
        {
            Group = group;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ColorExpr = colorExpr;
        }

        public ARCProgram(Expression group, Expression x, Expression y, Expression width, Expression height, Expression[] color)
        {
            Group = group;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = color;
        }


    }
}
