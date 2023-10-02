using System.Diagnostics;
using FuncScript;

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

Transpiler.Transpile(script, config);
return 0;