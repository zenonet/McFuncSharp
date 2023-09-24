using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class ParenthesesStatement : Statement, IInitializable
{
    private VariableNameProvider variableNameProvider = null!;

    public static void Initialize()
    {
        StatementRegistration.Create<ParenthesesStatement>(list => list.Peek().Type == TokenType.OpeningParenthesis).Register();
    }

    protected override bool CutTokensManually() => true;

    public override bool OnParse(ref TokenList list)
    {
        list.Pop();
        TokenList? innerList = list.FindBetweenBraces(TokenType.OpeningParenthesis, TokenType.ClosingParenthesis, Logger);
        if (innerList == null)
            return false;

        list.RemoveRange(..innerList.List.Count);

        Statement statement = Parse(ref innerList);
        if (statement == null)
            return false;

        variableNameProvider = (VariableNameProvider) statement.Execute();

        
        if(!list.StartsWith(TokenType.ClosingParenthesis))
            LoggingManager.LogError("Expected closing brace after statement in parentheses");
        
        list.Pop();
        
        return true;
    }

    public override Value Execute()
    {
        return variableNameProvider;
    }
}