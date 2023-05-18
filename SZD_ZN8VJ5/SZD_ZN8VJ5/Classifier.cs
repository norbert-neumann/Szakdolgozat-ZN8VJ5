using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZD_ZN8VJ5
{
    public static class Classifier
    {
        public static Dictionary<ARCObject, ARCObject> ObjectToAnchor = new Dictionary<ARCObject, ARCObject>();

        public static void SetAnchors(List<ARCObject> X, List<ARCObject> Y)
        {
            foreach (var obj in Y)
            {
                ObjectToAnchor.Add(obj, FindAnchorObject(X, obj));
            }
        }

        public static List<Predicate>[] FindRolePredicates(List<ARCObject> all, List<ARCObject>[] roles)
        {
            // X anchors
            List<Predicate>[] rolePredicates = new List<Predicate>[roles.Length];

            // Y -> X
            List<ARCObject>[] roleAnchors = roles.Select(role => role.Select(obj => ObjectToAnchor[obj]).ToList()).ToArray();

            for (int i = 0; i < roles.Length; i++)
            {
                var negatives = all.Except(roleAnchors[i]).ToList();
                var predicates = GetAnchorPredicates(roleAnchors[i], negatives); // true for all positives and false for all negatives
                rolePredicates[i] = predicates;
            }

            List<Predicate> aggregateExisitngPredicates = new List<Predicate>();
            for (int i = 0; i < roles.Length; i++)
            {
                aggregateExisitngPredicates.AddRange(rolePredicates[i]);
            }
            aggregateExisitngPredicates = aggregateExisitngPredicates.Distinct().ToList();

            for (int i = 0; i < roles.Length; i++)
            {
                if (rolePredicates[i].Count == 0)
                {
                    var negatives = all.Except(roleAnchors[i]).ToList();

                    if (IsInverseOtherPredicate(roleAnchors[i], negatives, aggregateExisitngPredicates))
                    {
                        rolePredicates[i] = Invert(aggregateExisitngPredicates);
                        break;
                    }

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

        private static ARCObject FindAnchorObject(List<ARCObject> X, ARCObject y)
        {
            var scores = X.Select(x => AnchorScore(x, y)).ToList();
            if (y.Parent != null)
            {
                return ObjectToAnchor[y.Parent];
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
            if (x.Noises.Count > 0 && x.Noises.TrueForAll(nx => y.Noises.Exists(ny =>
                nx.X - x.X == ny.X - y.X && nx.Y - x.Y == ny.Y - y.Y && nx.Group.AbstractEquals(ny.Group))))
            {
                score += 10;
            }
            if (x.X == y.X && x.Y == y.Y)
            {
                score += 10;
            }
            else if (x.X == y.X)
            {
                score += 3;
            }
            else if (x.Y == y.Y)
            {
                score += 3;
            }

            if (x.Width == y.Width)
            {
                score += 3;
            }
            if (x.Height == y.Height)
            {
                score += 3;
            }
            if (x.RegionToColor.SequenceEqual(y.RegionToColor))
            {
                score += 4;
            }

            return score;
        }

        private static List<Predicate> GetAnchorPredicates(List<ARCObject> positives, List<ARCObject> negatives)
        {
            List<Predicate> predicates = new List<Predicate>();
            predicates.AddRange(PredicateEngine.ObjectToPredicates[positives.First()]);

            foreach (var positive in positives)
            {
                if (positive != positives.First())
                {
                    /*var test = PredicateEngine.objToPredicates[positive];
                    if (predicates.Count > 0 && test.Count > 0 && predicates.Last().Equals(test.Last()))
                    {

                    }*/

                    predicates = PredicateEngine.ObjectToPredicates[positive].Where(pred => predicates.Contains(pred)).ToList();
                }
            }

            List<Predicate> antiPredicates = new List<Predicate>();
            negatives.ForEach(obj => antiPredicates.AddRange(PredicateEngine.ObjectToPredicates[obj]));

            return predicates.Distinct().Where(pred => !antiPredicates.Contains(pred)).ToList();
            //return predicates.Distinct().Except(antiPredicates.Distinct(), ).ToList();
        }

        private static bool IsInversePredicate(List<ARCObject> positives, List<ARCObject> negatives, List<Predicate> predicates)
        {
            var ps = predicates.Where(pred => positives.TrueForAll(obj => !PredicateEngine.ObjectToPredicates[obj].Contains(pred)));

            if (ps.Count() > 0)
            {
                return predicates.TrueForAll(pred => negatives.TrueForAll(obj => PredicateEngine.ObjectToPredicates[obj].Contains(pred)));
            }

            return false;
        }

        private static bool IsInverseOtherPredicate(List<ARCObject> positives, List<ARCObject> negatives, List<Predicate> predicates)
        {
            var ps = predicates.Where(pred => positives.TrueForAll(obj => !PredicateEngine.ObjectToPredicates[obj].Contains(pred)));

            if (ps.Count() > 0)
            {
                return predicates.TrueForAll(pred => negatives.Where(obj => PredicateEngine.ObjectToPredicates[obj].Contains(pred)).Count() > 0);
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
    }
}
