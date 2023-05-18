using Newtonsoft.Json;
using SZD_ZN8VJ5;
using SZD_ZN8VJ5.Groups;

static void Solve(ARC_File file, Func<int[][], List<ARCObject>> interpreter)
{
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

    Postprocessing.ToArcFile("f8ff0b80", winners.Select(w => w.Program.CollapseBestFit()).ToList(), anchorPredicates);

    for (int i = 0; i < file.test.Length; i++)
    {
        var X = interpreter.Invoke(file.test[i].input);

        PredicateEngine.Environment = X;
        List<ARCObject> Y = new List<ARCObject>();

        for (int j = 0; j < roles.Length; j++)
        {
            var currProgram = winners[j].Program.CollapseBestFit();
            Console.WriteLine(currProgram.ToFileString());
            var currAnchorPredicates = anchorPredicates[j];

            var anchors = PredicateEngine.FilterByPredicates(currAnchorPredicates);
            foreach (var anchor in anchors)
            {
                PredicateEngine.CurrentAnchor = anchor;
                var res = ExpressionEngine.ExecuteArcProgram(currProgram);
                Y.Add(res);
            }

        }

        int[][] result = Postprocessing.Render(Y);
        Postprocessing.SetClipMode(file);
        result = Postprocessing.Clip(result, file.test[i].input);
        Postprocessing.Display(result);
        Console.WriteLine();

        if (ImageProcessing.MatchPredictions(new int[][][] { result }, file.test))
        {
        }
    }

    Console.WriteLine();
}

Groups.Instantiate();


List<ARC_File> files = new List<ARC_File>();
List<string> fileNames = new List<string>();

foreach (string fileName in Directory.GetFiles(@"data\training", "*.json"))
{
    files.Add(JsonConvert.DeserializeObject<ARC_File>(File.ReadAllText(fileName)));
    fileNames.Add(fileName.Split(@"\").Last().Split('.').First());
}

Solver.Solve(files, fileNames);
