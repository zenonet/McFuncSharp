using System;
using System.IO;
using FuncScript;
using FuncScript.Internal;
using FuncSharp;
using FuncSharp.DataPackGen;
using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;


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


if(args.Length != 1)
{
    Console.WriteLine("Usage: FuncScript.exe <filename>");
    return;
}

string script = File.ReadAllText(args[0]);

Transpiler.Transpile(script);


LoadEntrypoint loadEntrypoint = new("load", Transpiler.McFunctionBuilder.ToString().CreateCommandArray());

DataPackGenerator generator = new(@"C:\Users\zeno\MultiMC\instances\Funcsharp\.minecraft\saves\FuncSharp LOOOOL\datapacks\first_funcsharp\", "first_funcsharp");

generator.AddEntrypoint(loadEntrypoint);

generator.Generate();
//File.WriteAllText(args[0], Transpiler.McFunctionBuilder.ToString());
