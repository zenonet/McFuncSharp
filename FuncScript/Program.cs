using System.Diagnostics;
using FuncScript;



/*
Computation.Initialize();

StreamWriter writer = File.CreateText(@"C:\Users\zeno\MultiMC\instances\Funcsharp\.minecraft\saves\FuncSharp LOOOOL\datapacks\first_funcsharp\data\first_funcsharp\functions\load.mcfunction");

writer.WriteLine(Computation.Initialize());

writer.WriteLine(MemoryManagement.SetVariable("amogus", "6"));
writer.WriteLine(MemoryManagement.SetVariable("abobus", "2"));

writer.WriteLine(MemoryManagement.MoveToComputationScoreboard("amogus", "amogus"));
writer.WriteLine(MemoryManagement.MoveToComputationScoreboard("abobus", "abobus"));

writer.WriteLine(Computation.Divide("amogus", "abobus", "asbestos"));

writer.WriteLine(MemoryManagement.MoveToStorage("asbestos", "asbestos"));

writer.Close();
*/


if(args.Length != 2)
{
    Console.WriteLine("Usage: FuncScript.exe <filename> <pathToDatapackDirectory>");
    return 1;
}

if (!File.Exists(args[0]))
{
    Console.WriteLine($"Your source file at {args[0]} does not exist.");
    return 1;
}

string script = File.ReadAllText(args[0]);
string datapackPath = args[1];

Config config = new(
    datapackPath,
    "first_funcsharp", ReloadBehavior.KillOld);

Stopwatch sw = Stopwatch.StartNew();
Transpiler.Transpile(script, config);
sw.Stop();

Console.WriteLine($"Successfully transpiled in {sw.ElapsedMilliseconds}ms");
return 0;

/*LoadEntrypoint loadEntrypoint = new("load", Transpiler.McFunctionBuilder.ToString().CreateCommandArray());

DataPackGenerator generator = new(@"C:\Users\zeno\MultiMC\instances\Funcsharp\.minecraft\saves\FuncSharp LOOOOL\datapacks\first_funcsharp\", "first_funcsharp");

generator.AddEntrypoint(loadEntrypoint);

generator.Generate();*/
//File.WriteAllText(args[0], Transpiler.McFunctionBuilder.ToString());
