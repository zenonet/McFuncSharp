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
        StatementRegistration.Create<FunctionDefinition>(TokenType.Keyword, TokenType.Keyword, TokenType.OpeningBrace).AddCustomParser(list =>
        {
            return list.Peek().RawContent is "func" or "function";
        }).AddPriority(1).Register();
    }

    protected override bool CutTokensManually()
    {
        return true;
    }

    protected override bool OnParse(ref TokenList list)
    {
        list.Pop(); // func keyword
        
        string name = list.Pop().RawContent;
        
        list.Pop(); // opening brace

        TokenList? rawParameters = list.FindBetweenBraces(TokenType.OpeningBrace, TokenType.ClosingBrace, Logger);
        
        if (rawParameters is null)
        {
            LoggingManager.LogError($"Invalid parameter declaration for function {name}");
            return false;
        } 
        list.RemoveRange(..(rawParameters.List.Count + 1)); // Remove the parameters and the parenthesis

        // TODO: Implement parameter parsing here

        if (list.Pop().Type != TokenType.OpeningCurlyBrace)
        {
            LoggingManager.LogError($"Expected function body after parameter declaration of function {name}");
        }
        
        TokenList? rawBody = list.FindBetweenBraces(TokenType.OpeningCurlyBrace, TokenType.ClosingCurlyBrace, Logger);
        
        if (rawBody is null)
        {
            LoggingManager.LogError($"Invalid syntax at function body for function {name}");
            return false;
        }
        list.RemoveRange(..(rawBody.List.Count + 1)); // Remove the body and the curly braces

        // The following code is no nice solution, but it works for now and nobody is going to read this anyway
        
        // Buffer the main string builder
        StringBuilder bufferedStringBuilder = Transpiler.McFunctionBuilder;
        
        // Create a new string builder for the function
        Transpiler.McFunctionBuilder = new();
        
        // Parse the function body
        Statement.ParseMultiple(ref rawBody);
        
        // Get the function string builder
        StringBuilder functionStringBuilder = Transpiler.McFunctionBuilder;
        
        // Restore the main string builder
        Transpiler.McFunctionBuilder = bufferedStringBuilder;

        FunctionEntrypoint entrypoint = new FunctionEntrypoint(name, functionStringBuilder.ToString().CreateCommandArray());
        
        Transpiler.AdditionalEntrypoints.Add(entrypoint);

        return true;
    }
}