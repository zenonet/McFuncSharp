using FuncScript.Types;
using Microsoft.Extensions.Logging;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class MethodCall : StatementExtension, IInitializable
{
    public static void Initialize()
    {
        StatementExtensionRegistration.CreateStatementExtensionRegistration<VariableCall, MethodCall>(TokenType.Dot, TokenType.Keyword, TokenType.OpeningParenthesis).Register();
    }

    private string returnValue = null!;

    protected override bool CutTokensManually()
    {
        return true;
    }

    public override bool OnParse(ref TokenList list, Statement baseStatement)
    {
        // Remove the dot
        list.Pop();
        
        string name = list.Pop().RawContent;

        // Remove the opening brace
        list.Pop();

        TokenList? betweenBraces = list.FindBetweenBraces(TokenType.OpeningParenthesis, TokenType.ClosingParenthesis, LoggingManager.ErrorLogger);

        if (betweenBraces == null)
            return false;

        //Remove the parameter list
        if(betweenBraces.List.Count > 0)
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

        FuncScriptValue[] values = new FuncScriptValue[parameters.Count + 1];

        values[0] = (FuncScriptValue) baseStatement.Execute();
        for (int i = 1; i < parameters.Count; i++)
        {
            values[i] = (FuncScriptValue) parameters[i].Execute();
        }

        Func<FuncScriptValue[],string>? function = Resources.Functions.FirstOrDefault(x => x.Key.Contains('.') && x.Key[(x.Key.LastIndexOf('.')+1)..] == name).Value;
        if (function != null)
        {
            // An internal function is being called

            // Find the correct function definition and add it
            function(values).Add();
        }
        else if (Transpiler.AdditionalEntrypoints.FirstOrDefault(x => x.Name == name) != null)
        {
            // A user defined function is being called
            $"function {Transpiler.Config.DataPackNameSpace}:{name}".Add();
        }
        else
        {
            LoggingManager.ErrorLogger.LogWarning($"Function {name} not found. Assuming the function is defined somewhere else.");
            $"function {Transpiler.Config.DataPackNameSpace}:{name}".Add();
        }

        returnValue = Resources.ReturnValue;

        return true;
    }

    public override Value Execute()
    {
        return new VariableNameProvider(returnValue);
    }
}