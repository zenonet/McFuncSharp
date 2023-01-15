using System.Collections.Generic;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;

namespace FuncScript.Statements;

public class FunctionCall : Statement, IInitializable
{
    public static void Initialize()
    {
        Register(StatementRegistration.Create<FunctionCall>(TokenType.Keyword, TokenType.OpeningBrace));
    }

    protected override bool CutTokensManually() => true;

    protected override bool OnParse(ref TokenList list)
    {
        string name = list.Pop().RawContent;

        // Remove the opening brace
        list.Pop();

        TokenList? betweenBraces = list.FindBetweenBraces(TokenType.OpeningBrace, TokenType.ClosingBrace, LoggingManager.ErrorLogger);

        if (betweenBraces == null)
            return false;

        //Remove the parameter list
        list.RemoveRange(..betweenBraces.List.Count);
        //Remove the closing brace
        list.Pop();

        List<Statement> parameters = new();
        while (betweenBraces.List.Count > 0)
        {
            //Parse the parameter
            parameters.Add(Parse(ref betweenBraces));

            if (betweenBraces.StartsWith(TokenType.Comma))
                betweenBraces.Pop();
        }


        // Find the correct function definition
        Transpiler.McFunctionBuilder.AppendLine(Resources.Functions[name](new[]
        {
            ((VariableNameProvider)parameters[0].Execute()).VariableName
        }));

        return true;
    }
}