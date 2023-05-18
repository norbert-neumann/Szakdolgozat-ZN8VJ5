
using System.Diagnostics;

namespace SZD_ZN8VJ5
{
    public class Solver
    {
        public static List<Func<int[][], List<ARCObject>>> interpreters;

        static Solver()
        {
            interpreters = new List<Func<int[][], List<ARCObject>>>();
            interpreters.Add(Preprocessing.Process_Image_Unicolor);
            interpreters.Add(Preprocessing.Process_Image_UniformPattern);
            interpreters.Add(Preprocessing.Process_Image_Multicolor);
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
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<ARC_File> copiedFiles = new List<ARC_File>(files);

            foreach (var (file, fileName) in files.Zip(fileNames))
            {
                Console.Write("Solving " + fileName + "...");

                bool success = Solve(file);
                if (success)
                {
                    ++solved;
                    copiedFiles.Remove(file);
                    Console.WriteLine(" -> Solved");
                }
                else
                {
                    Console.WriteLine(" -> Not Solved");
                }
            }

            sw.Stop();

            Console.WriteLine($"\nResults:\n\tsolved: {solved},\n\tunsolved: {files.Count - solved},\n\t" +
                $"accuracy: {solved / (float)files.Count},\n\ttotal runtime (seconds): {sw.Elapsed.TotalSeconds},\n\t" +
                $"average task time (seconds): {sw.Elapsed.TotalSeconds / (float)files.Count}");

        }

        public static bool Solve(ARC_File file)
        {
            foreach (var interpreter in interpreters)
            {
                try
                {
                    Classifier.ObjectToAnchor.Clear();
                    PredicateEngine.Visuals.Clear();
                    PredicateEngine.ObjectToPredicates.Clear();
                    PredicateEngine.ObjectToUniquePredicates.Clear();
                    Preprocessing.ExistingPseudoGroups.Clear();

                    var predictions = TrySolve(file, interpreter);
                    if (ImageProcessing.MatchPredictions(predictions, file.test))
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
            Postprocessing.SetClipMode(file);
            List<ARCObject> inputs = new List<ARCObject>();
            List<ARCObject> objects = new List<ARCObject>();
            List<SuperimposedARCPRogram> programs = new List<SuperimposedARCPRogram>();

            for (int i = 0; i < file.train.Length; i++)
            {
                var X = interpreter.Invoke(file.train[i].input);
                var Y = interpreter.Invoke(file.train[i].output);

                List<ARCObject> flattenedX = new List<ARCObject>();
                flattenedX.AddRange(X);
                X.ForEach(x => flattenedX.AddRange(x.Noises));

                List<ARCObject> flattenedY = new List<ARCObject>();
                flattenedY.AddRange(Y);
                Y.ForEach(y => flattenedY.AddRange(y.Noises));

                EquivalenceSaturation.Update(flattenedX);
                Classifier.SetAnchors(flattenedX, flattenedY);
                PredicateEngine.AddScene(flattenedX);

                inputs.AddRange(X);
                objects.AddRange(Y);
                programs.AddRange(Y.Select(y => EquivalenceSaturation.Saturate(y)));
            }


            var winners = ProgramInduction.Induce(programs.ToList(), objects.ToList());
            var roles = winners.Select(cl => cl.Elements).ToArray();
            var anchorPredicates = Classifier.FindRolePredicates(inputs, roles);
            int[][][] predictions = new int[file.test.Length][][];

            for (int i = 0; i < file.test.Length; i++)
            {
                var X = interpreter.Invoke(file.test[i].input);
                PredicateEngine.Environment = X;
                List<ARCObject> Y = new List<ARCObject>();

                for (int j = 0; j < roles.Length; j++)
                {
                    var currProgram = winners[j].Program.CollapseBestFit();
                    var currAnchorPredicates = anchorPredicates[j];

                    var anchors = PredicateEngine.FilterByPredicates(currAnchorPredicates);
                    foreach (var anchor in anchors)
                    {
                        PredicateEngine.CurrentAnchor = anchor;
                        var res = ExpressionEngine.ExecuteArcProgram(currProgram);
                        Y.Add(res);
                    }

                }

                predictions[i] = Postprocessing.Clip(Postprocessing.Render(Y), file.test[i].input);
            }

            return predictions;
        }
     }
}
