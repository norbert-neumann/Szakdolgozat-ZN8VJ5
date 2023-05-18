using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SZD_ZN8VJ5
{
    public enum PredicateType
    {
        Object,
        ObjectInt,
        Anchor
    }

    public class Predicate
    {
        public PredicateType Type;
        public int PredicateIndex;
        public int Argument;
        public bool Inverse = false;

        public static Predicate Anchor = new Predicate(PredicateType.Anchor, -1);

        public Predicate(PredicateType type, int predicateIndex)
        {
            Type = type;
            PredicateIndex = predicateIndex;
            Argument = -1;
        }

        public Predicate(PredicateType type, int predicateIndex, int argument) : this(type, predicateIndex)
        {
            Argument = argument;
        }

        public Predicate(Predicate other)
        {
            this.Type = other.Type;
            this.PredicateIndex = other.PredicateIndex;
            this.Argument = other.Argument;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case PredicateType.Object: return ObjectPredicateToString();
                case PredicateType.ObjectInt: return ObjectIntPredicateToString();
                case PredicateType.Anchor: return "ANCHOR";
            }

            return string.Empty;
        }

        private string ObjectPredicateToString()
        {
            string prefix = Inverse ? "!" : "";

            switch (PredicateIndex)
            {
                case 0: return prefix + "contained";
                case 1: return prefix + "container";
                case 2: return prefix + "child";
                case 3: return prefix + "parent";
                case 4: return prefix + "unicolor";
                case 5: return prefix + "multicolor";

                case 6: return prefix + "belongs_to_color_group";
                case 7: return prefix + "belongs_to_visual_group";
                case 8: return prefix + "unqiue_color";
                case 9: return prefix + "unqiue_visual";
                case 10: return prefix + "is_max";
                case 11: return prefix + "is_min";
            }

            return string.Empty;
        }

        private string ObjectIntPredicateToString()
        {
            string prefix = Inverse ? "!" : "";

            switch (PredicateIndex)
            {
                case 0: return prefix + string.Format("has_color ({0})", Argument);
                case 1: return prefix + string.Format("const_visual ({0})", Argument);
                case 2: return prefix + string.Format("contained_by_const_visual ({0})", Argument);

                case 3: return prefix + string.Format("belongs_to_specific_color_group ({0})", Argument);
                case 4: return prefix + string.Format("belongs_to_specific_visual_group ({0})", Argument);
                case 5: return prefix + string.Format("belongs_to_visual_group_contained_by_const_visual ({0})", Argument);
                case 6: return prefix + string.Format("is_max_color ({0})", Argument);
            }

            return string.Empty;
        }

        public override bool Equals(object? obj)
        {
            Predicate other = obj as Predicate;
            if (other != null)
            {
                return this.Type == other.Type && this.PredicateIndex == other.PredicateIndex && this.Argument == other.Argument;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Inverse.GetHashCode() + this.Type.GetHashCode() + this.Argument.GetHashCode() + this.PredicateIndex.GetHashCode();
        }
    }

    public static class PredicateEngine
    {
        public static List<Predicate> CurrentAnchorPredicates = null;
        public static ARCObject CurrentAnchor = null;

        public static List<Visual> Visuals; // set this to private
        public static List<ARCObject> Environment; // move this to Solver

        private static List<Func<ARCObject, bool>> objectPredicates;
        private static List<Func<ARCObject, int, bool>> objectIntPredicates;
        private static List<Func<ARCObject, List<ARCObject>, bool>> objectContextPredicates;
        private static List<Func<ARCObject, List<ARCObject>, int, bool>> objectContextIntPredicates;


        public static Dictionary<ARCObject, List<Predicate>> ObjectToPredicates;
        public static Dictionary<ARCObject, List<Predicate>> ObjectToUniquePredicates;

        static PredicateEngine()
        {
            Visuals = new List<Visual>();
            ObjectToPredicates = new Dictionary<ARCObject, List<Predicate>>();
            ObjectToUniquePredicates = new Dictionary<ARCObject, List<Predicate>>();

            objectPredicates = new List<Func<ARCObject, bool>>();
            objectIntPredicates = new List<Func<ARCObject, int, bool>>();
            objectContextPredicates = new List<Func<ARCObject, List<ARCObject>, bool>>();
            objectContextIntPredicates = new List<Func<ARCObject, List<ARCObject>, int, bool>>();

            // Simple predicates: Contained, Container, Child, Parent, Uni-Color, Multi-Color
            objectPredicates.Add(Contained);
            objectPredicates.Add(Container);
            objectPredicates.Add(Child);
            objectPredicates.Add(Parent);
            objectPredicates.Add(UniColor);
            objectPredicates.Add(MultiColor);

            objectPredicates.Add(ObjectBelongsToColorGroup);
            objectPredicates.Add(ObjectBelongsToVisualGroup);
            objectPredicates.Add(UniqueColor);
            objectPredicates.Add(UniqueVisual);
            objectPredicates.Add(IsMax);
            objectPredicates.Add(IsMin);

            // Color predicates: Has-Color, ConstVisual
            objectIntPredicates.Add(HasColor);
            objectIntPredicates.Add(ConstVisual);
            objectIntPredicates.Add(ContainedByConstantVisual);

            objectIntPredicates.Add(ObjectBelongsToSpecificColorGroup);
            objectIntPredicates.Add(ObjectBelongsToSpecificVisualGroup);
            objectIntPredicates.Add(BelongsToShapeGroup_ContainedByConstantVisual);
            objectIntPredicates.Add(IsMaxColor);
        }

        public static void AddScene(List<ARCObject> objects)
        {
            Environment = objects;

            // Add new unique visuals
            Visuals = Visuals.Union(objects.Select(obj => obj._Visual)).Distinct().ToList(); // null here?

            foreach (var obj in objects)
            {
                ObjectToPredicates.Add(obj, AddTruePredicates(obj, objects));
            }

            foreach (var obj in objects)
            {
                ObjectToUniquePredicates.Add(obj, GetUniquePredicates(obj, objects));
            }
        }

        public static ARCObject Execute(Predicate predicate)
        {
            return Execute(new List<Predicate>() { predicate });
        }

        public static List<ARCObject> FilterByPredicates(List<Predicate> rolePredicates)
        {
            return Environment.Where(obj => rolePredicates.All(pred => PredicateEngine.ExecuteOnObject(obj, pred))).ToList();
        }

        private static List<Predicate> GetUniquePredicates(ARCObject obj, List<ARCObject> context)
        {
            List<Predicate> uniquePredicates = new List<Predicate>();
            List<ARCObject> negatives = context.ToList();
            negatives.Remove(obj);

            foreach (var predicate in ObjectToPredicates[obj])
            {
                bool isUnique = negatives.All(negative => !ObjectToPredicates[negative].Contains(predicate));
                if (isUnique)
                {
                    uniquePredicates.Add(predicate);
                }
            }

            return uniquePredicates;
        }

        private static List<Predicate> AddTruePredicates(ARCObject obj, List<ARCObject> context)
        {
            List<Predicate> truePredicates = new List<Predicate>();

            // Simple
            int index = 0;
            foreach (var predicate in objectPredicates)
            {
                if (predicate.Invoke(obj))
                {
                    truePredicates.Add(new Predicate(PredicateType.Object, index));
                }
                ++index;
            }

            // Has-color
            foreach (var color in obj.RegionToColor)
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectInt, objectIntPredicates.IndexOf(HasColor), color));

                if (IsMaxColor(obj, color))
                {
                    truePredicates.Add(new Predicate(PredicateType.ObjectInt, objectIntPredicates.IndexOf(IsMaxColor), color));
                }
            }

            // ConstVisual
            truePredicates.Add(new Predicate(PredicateType.ObjectInt, objectIntPredicates.IndexOf(ConstVisual), Visuals.IndexOf(obj._Visual)));

            // ContainedByConstVisual
            if (Contained(obj))
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectInt,
                    objectIntPredicates.IndexOf(ContainedByConstantVisual), Visuals.IndexOf(obj.ContainedBy._Visual)));
            }

            // Context predicates
            index = 0;
            foreach (var predicate in objectContextPredicates)
            {
                if (predicate.Invoke(obj, context))
                {
                    truePredicates.Add(new Predicate(PredicateType.Object, index));
                }
                ++index;
            }

            // Arged context: specific color (0) and specific visual group (1)
            if (ObjectBelongsToColorGroup(obj))
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectInt,
                    objectIntPredicates.IndexOf(ObjectBelongsToSpecificColorGroup), obj.RegionToColor[0]));
            }
            if (ObjectBelongsToVisualGroup(obj))
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectInt,
                    objectIntPredicates.IndexOf(ObjectBelongsToSpecificVisualGroup), Visuals.IndexOf(obj._Visual)));
            }

            #region BelongsToShapeGroup_ContainedByConstantVisual
            var others = context.Where(x => x._Visual.Equals(obj._Visual)).ToList();
            others.Remove(obj);
            if (others.Count > 0)
            {
                var containers = others.Select(other => other.ContainedBy).Distinct().ToList();
                containers.Remove(null);

                foreach (var container in containers)
                {
                    truePredicates.Add(
                        new Predicate(
                            PredicateType.ObjectInt,
                            objectIntPredicates.IndexOf(BelongsToShapeGroup_ContainedByConstantVisual),
                            Visuals.IndexOf(container._Visual)
                        ));
                }
            }
            #endregion

            return truePredicates.Distinct().ToList();
        }
    
        private static ARCObject Execute(List<Predicate> predicates)
        {
            List<ARCObject> all = new List<ARCObject>();
            predicates.ForEach(predicate => all.AddRange(ExecuteReturnAll(predicate)));

            var output = all.GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .ToList();

            return output.First(); // InvalidOpException 
        }

        private static bool ExecuteOnObject(ARCObject obj, Predicate predicate)
        {
            switch (predicate.Type)
            {
                case PredicateType.Object:
                    return Execute_Object_Predicate(obj, predicate);
                case PredicateType.ObjectInt:
                    return Execute_Object_Int_Predicate(obj, predicate);
            }

            return false;
        }

        private static List<ARCObject> ExecuteReturnAll(Predicate predicate)
        {
            List<ARCObject> result = null;

            switch (predicate.Type)
            {
                case PredicateType.Anchor:
                    result = new List<ARCObject>() { CurrentAnchor }; break;
                case PredicateType.Object:
                    result = Execute_Object_Predicate(predicate); break;
                case PredicateType.ObjectInt:
                    result = Execute_Object_Int_Predicate(predicate); break;
            }

            if (result == null)
            {
                throw new Exception("No such object in Environment.");
            }
            return result;
        }

        private static bool Execute_Object_Predicate(ARCObject obj, Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return !objectPredicates[predicate.PredicateIndex].Invoke(obj);
            }
            else
            {
                return objectPredicates[predicate.PredicateIndex].Invoke(obj);
            }
        }

        private static List<ARCObject> Execute_Object_Predicate(Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return Environment.Where(obj => !objectPredicates[predicate.PredicateIndex].Invoke(obj)).ToList();
            }
            else
            {
                return Environment.Where(obj => objectPredicates[predicate.PredicateIndex].Invoke(obj)).ToList();
            }
        }

        private static bool Execute_Object_Int_Predicate(ARCObject obj, Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return !objectIntPredicates[predicate.PredicateIndex].Invoke(obj, predicate.Argument);
            }
            else
            {
                return objectIntPredicates[predicate.PredicateIndex].Invoke(obj, predicate.Argument);
            }
        }

        private static List<ARCObject> Execute_Object_Int_Predicate(Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return Environment.Where(obj => !objectIntPredicates[predicate.PredicateIndex].Invoke(obj, predicate.Argument)).ToList();
            }
            else
            {
                return Environment.Where(obj => objectIntPredicates[predicate.PredicateIndex].Invoke(obj, predicate.Argument)).ToList();
            }
        }

        private static bool Execute_Object_Context_Predicate(ARCObject obj, Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return !objectContextPredicates[predicate.PredicateIndex].Invoke(obj, Environment);
            }
            else
            {
                return objectContextPredicates[predicate.PredicateIndex].Invoke(obj, Environment);
            }
        }

        private static List<ARCObject> Execute_Object_Context_Predicate(Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return Environment.Where(obj => !objectContextPredicates[predicate.PredicateIndex].Invoke(obj, Environment)).ToList();
            }
            else
            {
                return Environment.Where(obj => objectContextPredicates[predicate.PredicateIndex].Invoke(obj, Environment)).ToList();
            }
        }

        private static bool Execute_Object_Context_Int_Predicate(ARCObject obj, Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return !objectContextIntPredicates[predicate.PredicateIndex].Invoke(obj, Environment, predicate.Argument);
            }
            else
            {
                return objectContextIntPredicates[predicate.PredicateIndex].Invoke(obj, Environment, predicate.Argument);
            }
        }

        private static List<ARCObject> Execute_Object_Context_Int_Predicate(Predicate predicate)
        {
            if (predicate.Inverse)
            {
                return Environment.Where(obj => !objectContextIntPredicates[predicate.PredicateIndex].Invoke(obj, Environment, predicate.Argument)).ToList();
            }
            else
            {
                return Environment.Where(obj => objectContextIntPredicates[predicate.PredicateIndex].Invoke(obj, Environment, predicate.Argument)).ToList();
            }
        }

        #region GroupPredicates
        // Only if there is one color group
        private static bool ObjectBelongsToColorGroup(ARCObject obj)
        {
            // True only for uni-color objects
            if (obj.RegionToColor.Length == 1)
            {
                var others = Environment.Where(x => x.RegionToColor.Length == 1).Where(x => x.RegionToColor[0] == obj.RegionToColor[0]).ToList();
                others.Remove(obj);

                return others.Count > 0;
            }
            return false;
        }

        // True only for uni-color objects!
        private static bool UniqueColor(ARCObject obj)
        {
            if (obj.RegionToColor.Length == 1)
            {
                var others = Environment.Where(x => x.RegionToColor.Length == 1).Where(x => x.RegionToColor[0] == obj.RegionToColor[0]).ToList();
                others.Remove(obj);

                return others.Count == 0;
            }
            return false;
        }

        // only if there is one specific color group
        private static bool ObjectBelongsToSpecificColorGroup(ARCObject obj, int color)
        {
            // True only for uni-color objects
            if (obj.RegionToColor.Length == 1 && obj.RegionToColor[0] == color)
            {
                var others = Environment.Where(x => x.RegionToColor.Length == 1 && obj.RegionToColor[0] == color).Where(x => x.RegionToColor[0] == obj.RegionToColor[0]).ToList();
                others.Remove(obj);

                return others.Count > 0;
            }
            return false;
        }

        private static bool ObjectBelongsToVisualGroup(ARCObject obj)
        {
            var others = Environment.Where(x => x.VisualEquals(obj)).ToList();
            others.Remove(obj);
            return others.Count > 0;
        }

        private static bool UniqueVisual(ARCObject obj)
        {
            var others = Environment.Where(x => x.VisualEquals(obj)).ToList();
            others.Remove(obj);
            return others.Count == 0;
        }

        private static bool ObjectBelongsToSpecificVisualGroup(ARCObject obj, int visualId)
        {
            if (obj.VisualEquals(Visuals[visualId]))
            {
                var others = Environment.Where(x => x.VisualEquals(Visuals[visualId])).ToList();
                others.Remove(obj);
                return others.Count > 0;
            }
            return false;
        }
        
        private static bool IsMax(ARCObject obj)
        {
            return Environment.Max(x => x.Sum()) == obj.Sum();
        }

        private static bool IsMin(ARCObject obj)
        {
            return Environment.Min(x => x.Sum()) == obj.Sum();
        }

        #endregion

        #region VisualPredicates
        private static bool ConstVisual(ARCObject obj, int visualId)
        {
            return obj._Visual.Equals(Visuals[visualId]);
        }
        #endregion

        #region ColorPredicates

        private static bool HasColor(ARCObject obj, int color)
        {
            return obj.RegionToColor.Contains(color);
        }

        private static bool IsMaxColor(ARCObject obj, int color)
        {
            if (Array.IndexOf(obj.RegionToColor, color) == -1)
            {
                return false;
            }


            var maxColor = Environment.Where(x => Array.IndexOf(x.RegionToColor, color) != -1).OrderByDescending(x => x.Group.RegionCount(Array.IndexOf(x.RegionToColor, color))).First();
            return obj == maxColor;
        }

        #endregion

        #region SimplePredicates
        private static bool Contained(ARCObject obj)
        {
            return obj.ContainedBy != null;
        }

        private static bool Container(ARCObject obj)
        {
            return obj.ContainedObjects.Count > 0;
        }

        private static bool Parent(ARCObject obj)
        {
            return obj.Parent != null;
        }

        private static bool Child(ARCObject obj)
        {
            return obj.Noises.Count > 0;
        }

        private static bool UniColor(ARCObject obj)
        {
            return obj.RegionToColor.Length == 1;
        }

        private static bool MultiColor(ARCObject obj)
        {
            return obj.RegionToColor.Length > 1;
        }
        #endregion

        #region TransitivePredicates

        private static bool ContainedByConstantVisual(ARCObject obj, int visualId)
        {
            if (obj.ContainedBy != null)
            {
                return ConstVisual(obj.ContainedBy, visualId);
            }
            return false;
        }

        private static bool BelongsToShapeGroup_ContainedByConstantVisual(ARCObject obj, int visualId)
        {
            //var others = context.Where(x => x.VisualEquals(visuals[visualId])).ToList();
            var others = Environment.Where(x => x._Visual.Equals(obj._Visual)).ToList();
            others.Remove(obj);
            if (others.Count > 0)
            {
                return others.Any(x => ContainedByConstantVisual(x, visualId));
            }
            return false;
        }

        #endregion
    }
}
