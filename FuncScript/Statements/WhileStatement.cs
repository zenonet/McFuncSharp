using System.Text.RegularExpressions;
using FuncScript.Internal;
using FuncScript.Types;
using FuncSharp;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class WhileStatement : Loop, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<WhileStatement>(list => list.Peek().RawContent == "while", TokenType.Keyword, TokenType.OpeningParenthesis).Register();
    }

    protected override bool CutTokensManually()
    {
        return true;
    }

    public override bool OnParse(ref TokenList list)
    {
        // Remove the if keyword
        list.Pop();

        // Remove the opening brace
        list.Pop();

        // Parse the condition
        Value? condition = null;

        TokenList listCopy = list;
        string conditionCode = Transpiler.YoinkGeneratedCode(() => condition = Statement.Parse(ref listCopy)!.Execute());
        list = listCopy;

        if (condition == null)
        {
            LoggingManager.LogError("Invalid condition in while loop");
            return false;
        }
        
        LoopFunctionName = IdManager.GetFunctionId();

        if (condition is ConstFuncScriptValue)
        {
            // TODO: Allow for const values as the condition of an if statement
            throw new NotImplementedException("Const values as the condition of an if statement is not yet implemented");
        }

        // Remove the closing brace
        if (!list.StartsWith(TokenType.ClosingParenthesis))
            LoggingManager.LogError("Expected closing brace after while statement condition");

        list.Pop();

        if (!list.StartsWith(TokenType.OpeningCurlyBracket))
            LoggingManager.LogError("Expected opening curly brace after while statement condition");

        list.Pop();

        // Parse the body
        TokenList? bodyTokenList = list.FindBetweenBraces(TokenType.OpeningCurlyBracket, TokenType.ClosingCurlyBracket, Logger);

        if (bodyTokenList == null)
            LoggingManager.LogError("Invalid body after while statement condition");

        // Cut the body tokens from the list
        list.RemoveRange(..bodyTokenList.List.Count);

        // Remove the closing curly brace
        list.Pop();

        Transpiler.StackTrace.Push(this);
        string loopCode = Transpiler.YoinkGeneratedCode(() => ParseMultiple(ref bodyTokenList));
        Transpiler.StackTrace.Pop();

        
        string conditionalLoopCall = $"{conditionCode}\n" +
                                     $"execute if data storage {MemoryManagement.MemoryTag} {{variables:{{{condition.AsVarnameProvider()}:1b}}}} run function {Transpiler.Config.DataPackNameSpace}:{LoopFunctionName}\n";

        // Add a recursive call to the loop function
        loopCode += conditionalLoopCall;

        loopCode = AddBreakabilityIfNeccessary(loopCode);
        
        // Create a loop entrypoint
        FunctionEntrypoint loopFunction = new(LoopFunctionName, loopCode.CreateCommandArray());
        Transpiler.AdditionalEntrypoints.Add(loopFunction);

        // Make the loop run recursively
        Transpiler.McFunctionBuilder.Append(conditionalLoopCall);

        return true;
    }
}