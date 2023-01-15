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

        Lexer.DefineTokens(new()
        {
            {"\".*?\"", TokenType.String},

            {@"\(", TokenType.OpeningBrace},
            {@"\)", TokenType.ClosingBrace},

            {
                @"\{|^block", TokenType.OpeningCurlyBrace
            },
            {
                @"\}|^end", TokenType.ClosingCurlyBrace
            },

            {
                @"\d+", TokenType.Int
            },
            {
                @"\d+.?\d*(?:f|F)", TokenType.Float
            },
            {
                @"(?:(?:t|T)(?:rue|RUE))|(?:(?:f|F)(?:alse|ALSE))", TokenType.Bool
            },

            {
                @";", TokenType.Semicolon
            },
            {
                @",", TokenType.Comma
            },

            {
                @"\s*=\s*", TokenType.Equals
            },
            {
                @"\s*<\s*", TokenType.LessThan
            },
            {
                @"\s*>\s*", TokenType.GreaterThan
            },

            {
                @"\s*\+\s*", TokenType.Plus
            },
            {
                @"\s*-\s*", TokenType.Minus
            },
            {
                @"\s*\*\s*", TokenType.Multiply
            },
            {
                @"\s*/\s*", TokenType.Divide
            },

            {
                @"\w+", TokenType.Keyword
            }, //Needs to be the last one because it would accept nearly anything);
        });

        TokenList tokens = Lexer.Lex(funcScriptCode);

        Statement.ParseMultiple(ref tokens);
    }

    /// <summary>
    /// Append a line to the function builder
    /// </summary>
    public static void Add(this string line)
    {
        McFunctionBuilder.AppendLine(line);
    }
}