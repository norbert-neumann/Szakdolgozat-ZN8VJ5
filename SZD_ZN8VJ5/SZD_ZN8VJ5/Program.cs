using Newtonsoft.Json;
using SZD_ZN8VJ5;
using SZD_ZN8VJ5.Groups;

static void Solve(ARC_File file)
{
    List<ARCObject> inputs = new List<ARCObject>();
    List<ARCObject> objects = new List<ARCObject>();
    List<SuperimposedARCPRogram> programs = new List<SuperimposedARCPRogram>();

    for (int i = 0; i < file.train.Length; i++)
    {
        var X = Preprocessing.Process_Image_1(file.train[i].input);
        var Y = Preprocessing.Process_Image_1(file.train[i].output);

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

    for (int i = 0; i < file.test.Length; i++)
    {
        var X = Preprocessing.Process_Image_1(file.test[i].input);
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

        int[][] result = Helpers.Render(Y);
        //Helpers.Display(result);
        Clipper.Instantiate(file);
        Helpers.Display(Clipper.Clip(result, file.test[0].input));
        Console.WriteLine();
    }

    Console.WriteLine();
}

Groups.Instantiate();

//0b148d64
//776ffc46
//3aa6fb7a
//0b148d64
//7e0986d6

//ARC_File file = JsonConvert.DeserializeObject<ARC_File>(File.ReadAllText(@"776ffc46.json"));


List<ARC_File> files = new List<ARC_File>();
List<string> fileNames = new List<string>();

foreach (string fileName in Directory.GetFiles(@"training", "*.json"))
{
    files.Add(JsonConvert.DeserializeObject<ARC_File>(File.ReadAllText(fileName)));
    fileNames.Add(fileName.Split(@"\").Last().Split('.').First());
}

Solver.Solve(files, fileNames);

Console.WriteLine();