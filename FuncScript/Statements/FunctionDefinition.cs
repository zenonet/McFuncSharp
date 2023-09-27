using System.Text;
using FuncSharp;
using FuncSharp.Commands;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;

namespace FuncScript.Statements;

public class FunctionDefinition : Statement, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<FunctionDefinition>(TokenType.Keyword, TokenType.Keyword, TokenType.OpeningParenthesis).AddCustomParser(list => list.Peek().RawContent is "func" or "function").AddPriority(1).Register();
    }

    protected override bool CutTokensManually()
    {
        return true;
    }

    public override bool OnParse(ref TokenList list)
    {
        list.Pop(); // func keyword

        string name = list.Pop().RawContent;

        list.Pop(); // opening brace

        TokenList? rawParameters = list.FindBetweenBraces(TokenType.OpeningParenthesis, TokenType.ClosingParenthesis, Logger);

        if (rawParameters is null)
        {
            LoggingManager.LogError($"Invalid parameter declaration for function {name}");
            return false;
        }

        list.RemoveRange(..(rawParameters.List.Count + 1)); // Remove the parameters and the parenthesis

        // TODO: Implement parameter parsing here

        if (list.Pop().Type != TokenType.OpeningCurlyBracket)
        {
            LoggingManager.LogError($"Expected function body after parameter declaration of function {name}");
        }

        TokenList? rawBody = list.FindBetweenBraces(TokenType.OpeningCurlyBracket, TokenType.ClosingCurlyBracket, Logger);

        if (rawBody is null)
        {
            LoggingManager.LogError($"Invalid syntax at function body for function {name}");
            return false;
        }

        list.RemoveRange(..(rawBody.List.Count + 1)); // Remove the body and the curly braces

        Transpiler.StackTrace.Push(this);
        string functionCode = Transpiler.YoinkGeneratedCode(() => Statement.ParseMultiple(ref rawBody));
        Transpiler.StackTrace.Pop();

        Entrypoint entrypoint;

        if (name == "tick")
        {
            entrypoint = new TickEntrypoint(name, functionCode.CreateCommandArray());
        }
        else
        {
            entrypoint = new FunctionEntrypoint(name, functionCode.CreateCommandArray());
        }

        Transpiler.AdditionalEntrypoints.Add(entrypoint);

        return true;
    }
}