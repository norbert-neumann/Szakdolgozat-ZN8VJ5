using System;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using SZD_ZN8VJ5.Groups;
using static System.Net.Mime.MediaTypeNames;

namespace SZD_ZN8VJ5
{
    public enum ExpressionType
    {
        Predicate,
        Xof,
        Yof,
        WidthOf,
        HeightOf,
        ColorOf,
        ColorMapOf,
        NoiseOf,
        NthNoise,
        ConstNumber,
        GroupOf,
        Object,
        DominantColor,
        FlattenGroup,
        RotateRigth,
        RotateLeft,
        VerticalReflect,
        MaxColor,
        ConstGroup,
        ClipNoise,
        Empty
    }

    public class Expression
    {
        public int ExpressionIndex;
        public object[] Arguments;
        public Type ReturnType;

        public Expression(Type type, int expressionIndex, object[] arguments)
        {
            ReturnType = type;
            this.ExpressionIndex = expressionIndex;
            Arguments = arguments;
        }

        public object Execute()
        {
            return ExpressionEngine.Expressions[ExpressionIndex].Invoke(Arguments);
        }

        public override bool Equals(object? obj)
        {
            Expression other = obj as Expression;
            if (other != null)
            {
                return this.ReturnType == other.ReturnType && this.ExpressionIndex == other.ExpressionIndex && this.Arguments.SequenceEqual(other.Arguments);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ReturnType.GetHashCode() + ExpressionIndex.GetHashCode() + Arguments.Sum(arg => arg.GetHashCode());
        }

        public override string ToString()
        {
            if (ExpressionIndex == 0)
            {
                return Arguments[0].ToString();
            }

            string result = ExpressionEngine.ExpressionNames[ExpressionIndex];

            // nth_noise_of [ANCHOR] [2]

            if (Arguments.Length == 1)
            {
                result += String.Format(" [{0}]", Arguments[0].ToString());
            }
            else if (Arguments.Length == 2)
            {
                result += String.Format(" [{0}] [{1}]", Arguments[0].ToString(), Arguments[1].ToString());
            }


            return result;
        }
    }

    public static class ExpressionEngine
    {

        public static List<Func<object[], object>> Expressions;
        public static List<Type> ReturnTypes;
        public static List<string> ExpressionNames;

        static ExpressionEngine()
        {
            Expressions = new List<Func<object[], object>>();
            ReturnTypes = new List<Type>();
            ExpressionNames = new List<string>();

            Expressions.Add(PredicateToObject); ReturnTypes.Add(typeof(ARCObject)); ExpressionNames.Add("predicate");

            Expressions.Add(XOf); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("x_of");
            Expressions.Add(YOf); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("y_of");
            Expressions.Add(WidthOf); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("width_of");
            Expressions.Add(HeightOf); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("height_of");
            Expressions.Add(ColorOf); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("color_of");

            Expressions.Add(ColorMapOf); ReturnTypes.Add(typeof(int[])); ExpressionNames.Add("color_map_of");

            Expressions.Add(NoiseOf); ReturnTypes.Add(typeof(List<ARCObject>)); ExpressionNames.Add("noise_of");
            Expressions.Add(NthNoise); ReturnTypes.Add(typeof(ARCObject)); ExpressionNames.Add("nth_noise_of");

            Expressions.Add(ConstNumber); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("const");

            Expressions.Add(GroupOf); ReturnTypes.Add(typeof(Group)); ExpressionNames.Add("shape_of");

            Expressions.Add(PredicateToObject); ReturnTypes.Add(typeof(ARCObject)); ExpressionNames.Add("_predicate_");

            Expressions.Add(DominantColor); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("dominant_color");

            Expressions.Add(FlattenGroup); ReturnTypes.Add(typeof(Group)); ExpressionNames.Add("flatten");

            Expressions.Add(RotateRight); ReturnTypes.Add(typeof(Group)); ExpressionNames.Add("rotate_right");
            Expressions.Add(RotateLeft); ReturnTypes.Add(typeof(Group)); ExpressionNames.Add("rotate_left");
            Expressions.Add(VerticalReflect); ReturnTypes.Add(typeof(Group)); ExpressionNames.Add("vertical_reflect");

            Expressions.Add(MaxColor); ReturnTypes.Add(typeof(int)); ExpressionNames.Add("max_color");

            Expressions.Add(ConstGroup); ReturnTypes.Add(typeof(Group)); ExpressionNames.Add("const_shape");
            
            Expressions.Add(ClipNoise); ReturnTypes.Add(typeof(ARCObject)); ExpressionNames.Add("clip_noise");
            Expressions.Add(Empty); ReturnTypes.Add(typeof(List<ARCObject>)); ExpressionNames.Add("empty");
        }

        public static ARCObject ExecuteArcProgram(ARCProgram program)
        {
            int[] regionToColor;
            if (program.ColorExpr != null)
            {
                regionToColor = (int[])program.ColorExpr.Execute();
            }
            else
            {
                regionToColor = program.Color.Select(expr => (int)expr.Execute()).ToArray();
            }

            ARCObject obj = new ARCObject
                (
                    group: (Group)program.Group.Execute(),
                    x: (int)program.X.Execute(),
                    y: (int)program.Y.Execute(),
                    width: (int)program.Width.Execute(),
                    height: (int)program.Height.Execute(),
                    regionToColor: regionToColor
                );

            if (program.NoiseExpr != null) // set noise's anchor
            {
                obj.Noises = (List<ARCObject>)program.NoiseExpr.Execute();
            }
            else if (program.Noise != null && program.Noise.Count > 0) // set noise's anchor
            {
                int noiseCount = PredicateEngine.CurrentAnchor.Noises.Count;
                for (int i = 0; i < noiseCount; i++)
                {
                    obj.Noises.Add(ExecuteArcProgram(program.Noise[i]));
                }
                //obj.Noises = program.Noise.Select(n => ExecuteArcProgram(n)).ToList();
            }

            return obj;
        }

        public static Expression Make(ExpressionType type, object[] args)
        {
            return new Expression(ReturnTypes[(int)type], (int)type, args);
        }

        public static Expression Make(Predicate predicate)
        {
            return new Expression(typeof(ARCObject), 0, new object[] { predicate });
        }

        private static object XOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();

            return obj.X;
        }

        private static object YOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();


            return obj.Y;
        }

        private static object WidthOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();


            return obj.Width;
        }

        private static object HeightOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();


            return obj.Height;
        }

        private static object GroupOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();

            return obj.Group;
        }

        private static object ColorOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();


            return obj.RegionToColor[(int)args[1]];
        }

        private static object ColorMapOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();

            return obj.RegionToColor;
        }

        private static object NoiseOf(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();

            return obj.Noises;
        }

        private static object NthNoise(object[] args)
        {
            ARCObject obj = (ARCObject)(args[0] as Expression).Execute();
            return obj.Noises[(int)args[1]]; // AOORE
        }

        private static object ConstNumber(object[] args)
        {
            return (int)args[0];
        }

        private static object ConstGroup(object[] args)
        {
            return (Group)args[0];
        }

        // object expression
        private static object PredicateToObject(object[] args)
        {
            return PredicateEngine.Execute(args[0] as Predicate);
        } 
        
        private static object DominantColor(object[] args)
        {
            return DSL.DominantColor((ARCObject)(args[0] as Expression).Execute());
        }

        private static object MaxColor(object[] args)
        {
            var obj = (ARCObject)(args[0] as Expression).Execute();
            return DSL.MaxColor(obj);
        }

        private static object FlattenGroup(object[] args)
        {
            Group grp = (Group)(args[0] as Expression).Execute();

            return DSL.FlattenGroup(grp); 
        }

        private static object RotateRight(object[] args)
        {
            Group grp = (Group)(args[0] as Expression).Execute();

            return DSL.RotateRigth(grp);
        }

        private static object RotateLeft(object[] args)
        {
            Group grp = (Group)(args[0] as Expression).Execute();

            return DSL.RotateLeft(grp);
        }
        private static object VerticalReflect(object[] args)
        {
            Group grp = (Group)(args[0] as Expression).Execute();

            return DSL.VerticalReflect(grp);
        }

        private static object ClipNoise(object[] args)
        {
            var obj = args[0] as ARCObject;
            return DSL.ClipOverlappedPart(obj, obj.Noises.First());
        }

        private static object Empty(object[] args)
        {
            return new List<ARCObject>();
        }
    }
}
