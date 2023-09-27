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

public class ForLoop : Loop, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<ForLoop>(list => list.Peek().RawContent == "for", TokenType.Keyword, TokenType.OpeningParenthesis).Register();
    }
    
    protected override bool CutTokensManually()
    {
        return true;
    }

    public override bool OnParse(ref TokenList list)
    {
        // Remove the for keyword
        list.Pop();

        // Remove the opening parenthesis
        list.Pop();

        TokenList? setupTokenList = list.FindBetweenBraces(TokenType.OpeningParenthesis, TokenType.ClosingParenthesis, Logger);
        if (setupTokenList == null)
        {
            return false;
        }

        list.RemoveRange(..setupTokenList.List.Count);

        TokenList[] setupParts = setupTokenList.Split(TokenType.Semicolon);
        if (setupParts.Length != 3)
        {
            LoggingManager.LogError("Invalid setup in for loop");
            return false;
        }

        LoopFunctionName = IdManager.GetFunctionId();
        

        // Parse the initialization (e.g. i = 0)
        if(setupParts[0].List.Count != 0)
            Statement.Parse(ref setupParts[0]);

        // Parse the condition (e.g. i < x)
        Value? condition = null;
        string conditionCode = "";
        if (setupParts[1].List.Count != 0)
        {
            conditionCode = Transpiler.YoinkGeneratedCode(() => condition = Statement.Parse(ref setupParts[1])!.Execute());
        }else
        {
            MemoryManagement.SetVariable("true", "1b");
            condition = new VariableNameProvider("true");
        }

        if (condition == null)
        {
            LoggingManager.LogError("Invalid condition in for loop");
            return false;
        }

        // Parse the increment (e.g. i++)
        string incrementCode = 
            setupParts[2].List.Count != 0 
                ? Transpiler.YoinkGeneratedCode(() => Statement.Parse(ref setupParts[2])!.Execute()) 
                : "";


        string prefixForAllBodyStatements;
        if (condition is ConstFuncScriptValue)
        {
            // TODO: Allow for const values as the condition of an if statement
            throw new NotImplementedException("Const values as the condition of an if statement is not yet implemented");
        }

        // Remove the closing parenthesis
        if (!list.StartsWith(TokenType.ClosingParenthesis))
            LoggingManager.LogError("Expected closing parenthesis after for loop setup");

        list.Pop();

        if (!list.StartsWith(TokenType.OpeningCurlyBracket))
            LoggingManager.LogError("Expected opening curly bracket after for loop setup");

        list.Pop();
        
        // Parse the body
        TokenList? bodyTokenList = list.FindBetweenBraces(TokenType.OpeningCurlyBracket, TokenType.ClosingCurlyBracket, Logger);

        if (bodyTokenList == null)
            LoggingManager.LogError("Invalid body after for loop condition");

        
        // Cut the body tokens from the list
        list.RemoveRange(..bodyTokenList.List.Count);

        // Remove the closing curly bracket
        list.Pop();

        Transpiler.StackTrace.Push(this);
        string loopCode = Transpiler.YoinkGeneratedCode(() => ParseMultiple(ref bodyTokenList)) + incrementCode;
        Transpiler.StackTrace.Pop();
        

        string conditionalLoopCall = $"{conditionCode}\n" +
                                     $"execute if data storage {MemoryManagement.MemoryTag} {{variables:{{{condition.AsVarnameProvider()}:1b}}}} run function {Transpiler.Config.DataPackNameSpace}:{LoopFunctionName}\n";

        // Add a recursive call to the loop function
        loopCode += conditionalLoopCall;

        loopCode = new Regex("(\n).*?(?=\\S)").Replace(loopCode, $"$1execute unless data storage {MemoryManagement.MemoryTag} {{variables:{{break_from_{LoopFunctionName}:1b}}}} run ");

        // Create a loop entrypoint
        FunctionEntrypoint loopFunction = new(LoopFunctionName, loopCode.CreateCommandArray());
        Transpiler.AdditionalEntrypoints.Add(loopFunction);
        
        // Make the loop run recursively
        Transpiler.McFunctionBuilder.Append(conditionalLoopCall);

        return true;
    }
    
}