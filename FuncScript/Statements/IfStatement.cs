using System.Text;
using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class IfStatement : Statement, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<IfStatement>(list => list.Peek().RawContent == "if", TokenType.Keyword, TokenType.OpeningBrace).Register();
    }

    protected override bool CutTokensManually()
    {
        return true;
    }

    protected override bool OnParse(ref TokenList list)
    {
        // Remove the if keyword
        list.Pop();

        // Remove the opening brace
        list.Pop();

        // Parse the condition
        Statement condition = Statement.Parse(ref list)!;

        Value value = condition.Execute();

        string prefixForAllBodyStatements;
        if (value is ConstFuncScriptValue)
        {
            // TODO: Allow for const values as the condition of an if statement
            throw new NotImplementedException("Const values as the condition of an if statement is not yet implemented");
        }
        else
            // TODO: Allow for block and entity comparisons
            prefixForAllBodyStatements = $"execute if data storage {MemoryManagement.MemoryTag} {{variables:{{{value.AsVarnameProvider()}:1}}}} run ";


        // Remove the closing brace
        if (!list.StartsWith(TokenType.ClosingBrace))
            LoggingManager.LogError("Expected closing brace after if statement condition");

        list.Pop();

        if (!list.StartsWith(TokenType.OpeningCurlyBrace))
            LoggingManager.LogError("Expected opening curly brace after if statement condition");

        list.Pop();

        // Parse the body
        TokenList? bodyTokenList = list.FindBetweenBraces(TokenType.OpeningCurlyBrace, TokenType.ClosingCurlyBrace, Logger);

        if (bodyTokenList == null)
            LoggingManager.LogError("Invalid body after if statement condition");

        // Cut the body tokens from the list
        list.RemoveRange(..bodyTokenList.List.Count);
        
        // Remove the closing curly brace
        list.Pop();

        Transpiler.prefixes.Add(prefixForAllBodyStatements);    
        
        // Parse the statements in the body (manually to be able to add the conditional prefix)
        while (bodyTokenList.List.Count > 0)
        {
            Statement.Parse(ref bodyTokenList);
            bodyTokenList.TrimStart(TokenType.Semicolon);
        }
        
        Transpiler.prefixes.Remove(prefixForAllBodyStatements);

        return true;
    }
}