using System;
using System.Collections.Generic;
using System.Linq;
using FuncScript.Types;
using Microsoft.Extensions.Logging;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class FunctionCall : Statement, IInitializable
{
    public static void Initialize()
    {
        Register(StatementRegistration.Create<FunctionCall>(TokenType.Keyword, TokenType.OpeningParenthesis));
        // Function call to a function in a static class
        Register(StatementRegistration.Create<FunctionCall>(TokenType.Keyword, TokenType.Dot, TokenType.Keyword, TokenType.OpeningParenthesis));
    }

    protected override bool CutTokensManually() => true;

    private string returnValue;

    public override bool OnParse(ref TokenList list)
    {
        string name = list.Pop().RawContent;

        if (list.StartsWith(TokenType.Dot))
        {
            list.Pop();
            name += $".{list.Pop().RawContent}";
        }

        // Remove the opening brace
        list.Pop();

        TokenList? betweenBraces = list.FindBetweenBraces(TokenType.OpeningParenthesis, TokenType.ClosingParenthesis, LoggingManager.ErrorLogger);

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

        FuncScriptValue[] values = new FuncScriptValue[parameters.Count];
        
        for (int i = 0; i < parameters.Count; i++)
        {
            values[i] = (FuncScriptValue) parameters[i].Execute();
        }

        if (Resources.Functions.TryGetValue(name, out Func<FuncScriptValue[], string>? function))
        {
            // An internal function is being called
            
            // Find the correct function definition and add it
            function(values).Add();
        }else if (Transpiler.AdditionalEntrypoints.FirstOrDefault(x => x.Name == name) != null)
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