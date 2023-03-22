using SZD_ZN8VJ5.Groups;

namespace SZD_ZN8VJ5
{
    public enum ExpressionType
    {
        Number,
        IntNumber,
        Group,
        IntArr,
        ARCObjArr,
        ConstNumber,
        ArcObj,
        ArcObj_Int,
        Exp_Int
    }

    public class Expression
    {
        public ExpressionType Type;
        public int ExpressionIndex;
        public object[] Arguments;

        public Expression(ExpressionType type, int expressionIndex, object[] arguments)
        {
            Type = type;
            this.ExpressionIndex = expressionIndex;
            Arguments = arguments;
        }

        public object Execute()
        {
            switch (Type)
            {
                case ExpressionType.Number: return Execute_NumberExpression();
                case ExpressionType.IntNumber: return Execute_Int_NumberExpression();
                case ExpressionType.Group: return Execute_GroupExpression();
                case ExpressionType.IntArr: return Execute_IntArrExpression();
                case ExpressionType.ARCObjArr: return Execute_ARCObjArrExpression();
                case ExpressionType.ConstNumber: return Execute_ConstNumberExpression();
                case ExpressionType.ArcObj: return Execute_ArcObjExpression();
                case ExpressionType.ArcObj_Int: return Execute_ArcObj_Int_Expression();
                case ExpressionType.Exp_Int: return Execute_Exp_Int_Expression();
            }
            throw new Exception("Expression Type not found.");
        }

        private ARCObject Execute_ArcObj_Int_Expression()
        {
            return ExpressionEngine.ARCObj_Int_Expressions[ExpressionIndex].Invoke(Arguments[0] as Predicate, (int)Arguments[1]);
        }

        private int Execute_Exp_Int_Expression()
        {
            return ExpressionEngine.Exp_Number_Expressions[ExpressionIndex].Invoke(Arguments[0] as Expression);
        }

        private int Execute_NumberExpression()
        {
            return ExpressionEngine.Number_Expressions[ExpressionIndex].Invoke(Arguments[0] as Predicate);
        }
        private int Execute_Int_NumberExpression()
        {
            return ExpressionEngine.Int_Number_Expressions[ExpressionIndex].Invoke(Arguments[0] as Predicate, (int)Arguments[1]);
        }
        private Group Execute_GroupExpression()
        {
            return ExpressionEngine.Group_Expressions[ExpressionIndex].Invoke(Arguments[0] as Predicate);
        }
        private int[] Execute_IntArrExpression()
        {
            return ExpressionEngine.IntArr_Expressions[ExpressionIndex].Invoke(Arguments[0] as Predicate);
        }
        private List<ARCObject> Execute_ARCObjArrExpression()
        {
            return ExpressionEngine.ARCObjArr_Expressions[ExpressionIndex].Invoke(Arguments[0] as Predicate);
        }
        private int Execute_ConstNumberExpression()
        {
            return (int)Arguments[0];
        }
        private ARCObject Execute_ArcObjExpression()
        {
            return ExpressionEngine.ARCObj_Expressions[ExpressionIndex].Invoke(Arguments[0] as Predicate);
        }

        public override bool Equals(object? obj)
        {
            Expression other = obj as Expression;
            if (other != null)
            {
                return this.Type == other.Type && this.ExpressionIndex == other.ExpressionIndex && this.Arguments.SequenceEqual(other.Arguments);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() + ExpressionIndex.GetHashCode() + Arguments.Sum(arg => arg.GetHashCode());
        }
    }

    public static class ExpressionEngine
    {
        public static List<Func<Predicate, int>> Number_Expressions;
        public static List<Func<Expression, int>> Exp_Number_Expressions;
        public static List<Func<Predicate, int, int>> Int_Number_Expressions;
        public static List<Func<Predicate, Group>> Group_Expressions;
        public static List<Func<Predicate, int[]>> IntArr_Expressions;
        public static List<Func<Predicate, List<ARCObject>>> ARCObjArr_Expressions;
        public static List<Func<int, int>> ConstNumber_Expressions;
        public static List<Func<Predicate, ARCObject>> ARCObj_Expressions;
        public static List<Func<Predicate, int, ARCObject>> ARCObj_Int_Expressions;


        static ExpressionEngine()
        {
            Number_Expressions = new List<Func<Predicate, int>>();
            Int_Number_Expressions = new List<Func<Predicate, int, int>>();
            Group_Expressions = new List<Func<Predicate, Group>>();
            IntArr_Expressions = new List<Func<Predicate, int[]>>();
            ARCObjArr_Expressions = new List<Func<Predicate, List<ARCObject>>>();
            ConstNumber_Expressions = new List<Func<int, int>>();
            ARCObj_Expressions = new List<Func<Predicate, ARCObject>>();
            ARCObj_Int_Expressions = new List<Func<Predicate, int, ARCObject>>();
            Exp_Number_Expressions = new List<Func<Expression, int>>();

            // 0
            Number_Expressions.Add(XOf);
            Number_Expressions.Add(YOf);
            Number_Expressions.Add(WidthOf);
            Number_Expressions.Add(HeightOf);

            // 1
            Int_Number_Expressions.Add(ColorOf);

            // 2
            Group_Expressions.Add(GroupOf);

            // 3
            IntArr_Expressions.Add(ColorMapOf);

            // 4
            ARCObjArr_Expressions.Add(NoiseOf);

            // 5
            ConstNumber_Expressions.Add(ContsNumber);

            // 6
            ARCObj_Expressions.Add(PredicateEngine.Execute);

            // 7
            ARCObj_Int_Expressions.Add(NthNoise);

            // 8
            Exp_Number_Expressions.Add(XOf);
            Exp_Number_Expressions.Add(YOf);
            Exp_Number_Expressions.Add(WidthOf);
            Exp_Number_Expressions.Add(HeightOf);

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
                obj.noises = (List<ARCObject>)program.NoiseExpr.Execute();
            }
            else if (program.Noise != null && program.Noise.Count > 0) // set noise's anchor
            {
                obj.noises = program.Noise.Select(n => ExecuteArcProgram(n)).ToList();
            }

            return obj;
        }

        private static int XOf(Predicate predicate)
        {
            return PredicateEngine.Execute(predicate).x;
        }

        private static int XOf(Expression objExpression)
        {
            return (objExpression.Execute() as ARCObject).x;
        }

        private static int YOf(Predicate predicate)
        {
            return PredicateEngine.Execute(predicate).y;
        }

        private static int YOf(Expression objExpression)
        {
            return (objExpression.Execute() as ARCObject).y;
        }

        private static int WidthOf(Predicate predicate)
        {
            return PredicateEngine.Execute(predicate).width;
        }

        private static int WidthOf(Expression objExpression)
        {
            return (objExpression.Execute() as ARCObject).width;
        }

        private static int HeightOf(Predicate predicate)
        {
            return PredicateEngine.Execute(predicate).height;
        }

        private static int HeightOf(Expression objExpression)
        {
            return (objExpression.Execute() as ARCObject).height;
        }

        private static Group GroupOf(Predicate predicate)
        {
            return PredicateEngine.Execute(predicate).Group;
        }

        private static int ColorOf(Predicate predicate, int region)
        {
            return PredicateEngine.Execute(predicate).regionToColor[region];
        }

        private static int[] ColorMapOf(Predicate predicate)
        {
            return PredicateEngine.Execute(predicate).regionToColor;
        }

        private static List<ARCObject> NoiseOf(Predicate predicate)
        {
            return PredicateEngine.Execute(predicate).noises;
        }

        private static int ContsNumber(int number)
        {
            return number;
        }

        private static ARCObject NthNoise(Predicate predicate, int index)
        {
            return NoiseOf(predicate)[index];
        }



    }
}
