using System.Text;
using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

namespace FuncScript;

public static class Transpiler
{
    public static StringBuilder McFunctionBuilder { get; private set; } = new();

    public static void Transpile(string funcScriptCode)
    {
        McFunctionBuilder = new();

        TokenList tokens = Lexer.Lex(funcScriptCode);

        Statement.ParseMultiple(ref tokens);

        Console.WriteLine("Done!");
    }

    /// <summary>
    /// Append a line to the function builder
    /// </summary>
    public static void Add(this string line)
    {
        McFunctionBuilder.AppendLine(line);
    }
}