// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using SZD_ZN8VJ5;

ARC_File file = JsonConvert.DeserializeObject<ARC_File>(File.ReadAllText("776ffc46.json"));

Groups.Instantiate();

//Preprocessing.Outer(Helpers.ToMatrix(file.train[0].input));
//Preprocessing.Outer(Helpers.ToMatrix(file.train[0].output));

//Preprocessing.Outer(Helpers.ToMatrix(file.train[1].input));

for (int i = 0; i < file.train.Length; i++)
{
    Preprocessing.ExistingPseudoGroups.Clear();
    var input = Preprocessing.Outer(Helpers.ToMatrix(file.train[i].input));
    var output = Preprocessing.Outer(Helpers.ToMatrix(file.train[i].output));

    File.AppendAllLines("test.txt", input.Select(obj => obj.ToTerm(i, "in")));
    File.AppendAllLines("test.txt", input.Select(obj => obj.ToTerm(i, "out")));

}

