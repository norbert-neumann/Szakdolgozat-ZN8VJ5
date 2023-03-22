
namespace SZD_ZN8VJ5
{
    public enum PredicateType
    {
        Object,
        ObjectInt,
        ObjectContext,
        ObjectContextInt,
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
            if (Argument >= 0)
            {
                return string.Format("{0}[{1} {2} {3}]", Inverse ? "!" : "", (int)Type, PredicateIndex, Argument);
            }
            else
            {
                return string.Format("{0}[{1} {2}]", Inverse ? "!" : "", (int)Type, PredicateIndex);
            }
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

        public static List<Visual> visuals;
        public static List<ARCObject> Environment;

        private static List<Func<ARCObject, bool>> objectPredicates;
        private static List<Func<ARCObject, int, bool>> objectIntPredicates;
        private static List<Func<ARCObject, List<ARCObject>, bool>> objectContextPredicates;
        private static List<Func<ARCObject, List<ARCObject>, int, bool>> objectContextIntPredicates;


        public static Dictionary<ARCObject, List<Predicate>> objToPredicates;
        public static Dictionary<ARCObject, List<Predicate>> objToUniquePredicates;

        static PredicateEngine()
        {
            visuals = new List<Visual>();
            objToPredicates = new Dictionary<ARCObject, List<Predicate>>();
            objToUniquePredicates = new Dictionary<ARCObject, List<Predicate>>();

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

            // Color predicates: Has-Color, ConstVisual
            objectIntPredicates.Add(HasColor);
            objectIntPredicates.Add(ConstVisual);
            objectIntPredicates.Add(ContainedByConstantVisual);

            // Context predicates: Group predicates, Unique predicates
            objectContextPredicates.Add(ObjectBelongsToColorGroup);
            objectContextPredicates.Add(ObjectBelongsToVisualGroup);
            objectContextPredicates.Add(UniqueColor);
            objectContextPredicates.Add(UniqueVisual);

            // Arged context predicates: Arged group predicates
            objectContextIntPredicates.Add(ObjectBelongsToSpecificColorGroup);
            objectContextIntPredicates.Add(ObjectBelongsToSpecificVisualGroup);
            objectContextIntPredicates.Add(BelongsToShapeGroup_ContainedByConstantVisual);
        }

        public static void AddScene(List<ARCObject> objects)
        {
            // Add new unique visuals
            visuals = visuals.Union(objects.Select(obj => obj._Visual)).Distinct().ToList(); // null here?

            foreach (var obj in objects)
            {
                objToPredicates.Add(obj, AddTruePredicates(obj, objects));
            }

            foreach (var obj in objects)
            {
                objToUniquePredicates.Add(obj, GetUniquePredicates(obj, objects));
            }
        }

        public static void ReloadReducesScene(List<ARCObject> objects)
        {
            objToPredicates.Clear();
            objToUniquePredicates.Clear();

            foreach (var obj in objects)
            {
                objToPredicates.Add(obj, AddTruePredicates(obj, objects));
            }

            foreach (var obj in objects)
            {
                objToUniquePredicates.Add(obj, GetUniquePredicates(obj, objects));
            }
        }

        private static List<Predicate> GetUniquePredicates(ARCObject obj, List<ARCObject> context)
        {
            List<Predicate> uniquePredicates = new List<Predicate>();
            List<ARCObject> negatives = context.Where(x => Helpers.Coexists(obj, x)).ToList();
            negatives.Remove(obj);

            foreach (var predicate in objToPredicates[obj])
            {
                bool isUnique = negatives.All(negative => !objToPredicates[negative].Contains(predicate));
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
            foreach (var color in obj.regionToColor)
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectInt, objectIntPredicates.IndexOf(HasColor), color));
            }

            // ConstVisual
            truePredicates.Add(new Predicate(PredicateType.ObjectInt, objectIntPredicates.IndexOf(ConstVisual), visuals.IndexOf(obj._Visual)));

            // ContainedByConstVisual
            if (Contained(obj))
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectInt,
                    objectIntPredicates.IndexOf(ContainedByConstantVisual), visuals.IndexOf(obj.containedBy._Visual)));
            }

            // Context predicates
            index = 0;
            foreach (var predicate in objectContextPredicates)
            {
                if (predicate.Invoke(obj, context))
                {
                    truePredicates.Add(new Predicate(PredicateType.ObjectContext, index));
                }
                ++index;
            }

            // Arged context: specific color (0) and specific visual group (1)
            if (ObjectBelongsToColorGroup(obj, context))
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectContextInt,
                    objectContextIntPredicates.IndexOf(ObjectBelongsToSpecificColorGroup), obj.regionToColor[0]));
            }
            if (ObjectBelongsToVisualGroup(obj, context))
            {
                truePredicates.Add(new Predicate(PredicateType.ObjectContextInt,
                    objectContextIntPredicates.IndexOf(ObjectBelongsToSpecificVisualGroup), visuals.IndexOf(obj._Visual)));
            }

            #region BelongsToShapeGroup_ContainedByConstantVisual
            var others = context.Where(x => x._Visual.Equals(obj._Visual)).ToList();
            others.Remove(obj);
            if (others.Count > 0)
            {
                var containers = others.Select(other => other.containedBy).Distinct().ToList();
                containers.Remove(null);

                foreach (var container in containers)
                {
                    truePredicates.Add(
                        new Predicate(
                            PredicateType.ObjectContextInt,
                            objectContextIntPredicates.IndexOf(BelongsToShapeGroup_ContainedByConstantVisual),
                            visuals.IndexOf(container._Visual)
                        ));
                }
            }
            #endregion

            return truePredicates;
        }

        public static ARCObject Execute(Predicate predicate)
        {
            return Execute(new List<Predicate>() { predicate });
        }

        public static ARCObject Execute(List<Predicate> predicates) // *
        {
            List<ARCObject> all = new List<ARCObject>();
            predicates.ForEach(predicate => all.AddRange(ExecuteReturnAll(predicate)));

            var output = all.GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .ToList();

            return output.First();
        }

        public static List<ARCObject> AnchorsForGivenRole(List<Predicate> rolePredicates)
        {
            foreach (var o in Environment)
            {
                bool v = rolePredicates.All(pred => PredicateEngine.ExecuteOnObject(o, pred));
            }

            return Environment.Where(obj => rolePredicates.All(pred => PredicateEngine.ExecuteOnObject(obj, pred))).ToList();
        }

        public static bool ExecuteOnObject(ARCObject obj, Predicate predicate)
        {
            switch (predicate.Type)
            {
                case PredicateType.Object:
                    return Execute_Object_Predicate(obj, predicate);
                case PredicateType.ObjectInt:
                    return Execute_Object_Int_Predicate(obj, predicate);
                case PredicateType.ObjectContext:
                    return Execute_Object_Context_Predicate(obj, predicate);
                case PredicateType.ObjectContextInt:
                    return Execute_Object_Context_Int_Predicate(obj, predicate);
            }

            return false;
        }

        public static List<ARCObject> ExecuteReturnAll(Predicate predicate)
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
                case PredicateType.ObjectContext:
                    result = Execute_Object_Context_Predicate(predicate); break;
                case PredicateType.ObjectContextInt:
                    result = Execute_Object_Context_Int_Predicate(predicate); break;
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
        private static bool ObjectBelongsToColorGroup(ARCObject obj, List<ARCObject> context)
        {
            // True only for uni-color objects
            if (obj.regionToColor.Length == 1)
            {
                var others = context.Where(x => x.regionToColor.Length == 1).Where(x => x.regionToColor[0] == obj.regionToColor[0]).ToList();
                others.Remove(obj);

                return others.Count > 0;
            }
            return false;
        }

        // True only for uni-color objects!
        private static bool UniqueColor(ARCObject obj, List<ARCObject> context)
        {
            if (obj.regionToColor.Length == 1)
            {
                var others = context.Where(x => x.regionToColor.Length == 1).Where(x => x.regionToColor[0] == obj.regionToColor[0]).ToList();
                others.Remove(obj);

                return others.Count == 0;
            }
            return false;
        }

        // only if there is one specific color group
        private static bool ObjectBelongsToSpecificColorGroup(ARCObject obj, List<ARCObject> context, int color)
        {
            // True only for uni-color objects
            if (obj.regionToColor.Length == 1 && obj.regionToColor[0] == color)
            {
                var others = context.Where(x => x.regionToColor.Length == 1 && obj.regionToColor[0] == color).Where(x => x.regionToColor[0] == obj.regionToColor[0]).ToList();
                others.Remove(obj);

                return others.Count > 0;
            }
            return false;
        }

        private static bool ObjectBelongsToVisualGroup(ARCObject obj, List<ARCObject> context)
        {
            var others = context.Where(x => x.VisualEquals(obj)).ToList();
            others.Remove(obj);
            return others.Count > 0;
        }

        private static bool UniqueVisual(ARCObject obj, List<ARCObject> context)
        {
            var others = context.Where(x => x.VisualEquals(obj)).ToList();
            others.Remove(obj);
            return others.Count == 0;
        }

        private static bool ObjectBelongsToSpecificVisualGroup(ARCObject obj, List<ARCObject> context, int visualId)
        {
            if (obj.VisualEquals(visuals[visualId]))
            {
                var others = context.Where(x => x.VisualEquals(visuals[visualId])).ToList();
                others.Remove(obj);
                return others.Count > 0;
            }
            return false;
        }
        #endregion
        
        #region VisualPredicates
        private static bool ConstVisual(ARCObject obj, int visualId)
        {
            return obj._Visual.Equals(visuals[visualId]);
        }
        #endregion

        #region ColorPredicates
        private static bool HasColor(ARCObject obj, int color)
        {
            return obj.regionToColor.Contains(color);
        }
        #endregion

        #region SimplePredicates
        private static bool Contained(ARCObject obj)
        {
            return obj.containedBy != null;
        }

        private static bool Container(ARCObject obj)
        {
            return obj.contains.Count > 0;
        }

        private static bool Parent(ARCObject obj)
        {
            return obj.noiseTo != null;
        }

        private static bool Child(ARCObject obj)
        {
            return obj.noises.Count > 0;
        }

        private static bool UniColor(ARCObject obj)
        {
            return obj.regionToColor.Length == 1;
        }

        private static bool MultiColor(ARCObject obj)
        {
            return obj.regionToColor.Length > 1;
        }
        #endregion

        #region TransitivePredicates

        private static bool ContainedByConstantVisual(ARCObject obj, int visualId)
        {
            if (obj.containedBy != null)
            {
                return ConstVisual(obj.containedBy, visualId);
            }
            return false;
        }

        private static bool BelongsToShapeGroup_ContainedByConstantVisual(ARCObject obj, List<ARCObject> context, int visualId)
        {
            //var others = context.Where(x => x.VisualEquals(visuals[visualId])).ToList();
            var others = context.Where(x => x._Visual.Equals(obj._Visual)).ToList();
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
