
namespace SZD_ZN8VJ5
{
    public class Solver
    {
        private static List<Func<int[][], List<ARCObject>>> interpreters;

        static Solver()
        {
            interpreters = new List<Func<int[][], List<ARCObject>>>();
            interpreters.Add(Preprocessing.Process_Image_1);
            interpreters.Add(Preprocessing.Process_Image_2);
            interpreters.Add(Preprocessing.Process_Image_3);
        }

        public static void Solve(List<ARC_File> files)
        {
            int solved = 0;
            foreach (var file in files)
            {
                bool success = Solve(file);
                if (success)
                {
                    ++solved;
                    Console.WriteLine("Solved");
                }
                else
                {
                    Console.WriteLine("Not Solved");
                }
            }

            Console.WriteLine($"Results: solved: {solved}, unsolved: {files.Count - solved}, " +
                $"accuracy: {solved / (float)files.Count}");
        }

        public static void Solve(List<ARC_File> files, List<string> fileNames)
        {
            int solved = 0;
            foreach (var (file, fileName) in files.Zip(fileNames))
            {
                Console.Write("Solving " + fileName + "...");

                bool success = Solve(file);
                if (success)
                {
                    ++solved;
                    Console.WriteLine(" -> Solved");
                }
                else
                {
                    Console.WriteLine(" -> Not Solved");
                }
            }

            Console.WriteLine($"Results: solved: {solved}, unsolved: {files.Count - solved}, " +
                $"accuracy: {solved / (float)files.Count}");
        }

        public static bool Solve(ARC_File file)
        {
            foreach (var interpreter in interpreters)
            {
                try
                {
                    ProgramInduction.objToAnchor.Clear();
                    PredicateEngine.visuals.Clear();
                    PredicateEngine.objToPredicates.Clear();
                    PredicateEngine.objToUniquePredicates.Clear();
                    Preprocessing.ExistingPseudoGroups.Clear();

                    var predictions = TrySolve(file, interpreter);
                    if (Helpers.MatchPredictions(predictions, file.test))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        public static int[][][] TrySolve(ARC_File file, Func<int[][], List<ARCObject>> interpreter)
        {
            Clipper.Instantiate(file);
            List<ARCObject> inputs = new List<ARCObject>();
            List<ARCObject> objects = new List<ARCObject>();
            List<SuperimposedARCPRogram> programs = new List<SuperimposedARCPRogram>();

            for (int i = 0; i < file.train.Length; i++)
            {
                var X = interpreter.Invoke(file.train[i].input);
                var Y = interpreter.Invoke(file.train[i].output);

                List<ARCObject> flattenedX = new List<ARCObject>();
                flattenedX.AddRange(X);
                X.ForEach(x => flattenedX.AddRange(x.noises));

                List<ARCObject> flattenedY = new List<ARCObject>();
                flattenedY.AddRange(Y);
                Y.ForEach(y => flattenedY.AddRange(y.noises));

                EquivalenceSaturation.Update(flattenedX);
                ProgramInduction.SetAnhors(flattenedX, flattenedY);
                PredicateEngine.AddScene(flattenedX);

                inputs.AddRange(X);
                objects.AddRange(Y);
                programs.AddRange(Y.Select(y => EquivalenceSaturation.Saturate(y, flattenedX)));
            }


            var winners = ProgramInduction.TestOuter(programs.ToList(), objects.ToList());
            var roles = Helpers.SelectMinsetFromWinners(winners);
            var anchorPredicates = ProgramInduction.FindRolePredicates(inputs, roles);
            int[][][] predictions = new int[file.test.Length][][];

            for (int i = 0; i < file.test.Length; i++)
            {
                var X = interpreter.Invoke(file.test[i].input);
                PredicateEngine.Environment = X;
                List<ARCObject> Y = new List<ARCObject>();

                for (int j = 0; j < roles.Length; j++)
                {
                    var currProgram = winners[j].program.CollapseFirst();
                    var currAnchorPredicates = anchorPredicates[j];

                    var anchors = PredicateEngine.AnchorsForGivenRole(currAnchorPredicates);
                    foreach (var anchor in anchors)
                    {
                        PredicateEngine.CurrentAnchor = anchor;
                        var res = ExpressionEngine.ExecuteArcProgram(currProgram);
                        Y.Add(res);
                    }

                }

                predictions[i] = Clipper.Clip(Helpers.Render(Y), file.test[i].input);
            }

            return predictions;
        }
     }
}
