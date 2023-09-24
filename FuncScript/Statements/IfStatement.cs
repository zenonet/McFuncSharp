using System;
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
        StatementRegistration.Create<IfStatement>(list => list.Peek().RawContent == "if", TokenType.Keyword, TokenType.OpeningParenthesis).Register();
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
        Statement condition = Statement.Parse(ref list)!;

        Value value = condition.Execute();

        string prefixForAllBodyStatements;
        if (value is ConstFuncScriptValue)
        {
            // TODO: Allow for const values as the condition of an if statement
            throw new NotImplementedException("Const values as the condition of an if statement is not yet implemented");
        }
        else
            prefixForAllBodyStatements = $"execute if data storage {MemoryManagement.MemoryTag} {{variables:{{{value.AsVarnameProvider()}:1b}}}} run ";


        // Remove the closing brace
        if (!list.StartsWith(TokenType.ClosingParenthesis))
            LoggingManager.LogError("Expected closing brace after if statement condition");

        list.Pop();

        if (!list.StartsWith(TokenType.OpeningCurlyBracket))
            LoggingManager.LogError("Expected opening curly brace after if statement condition");

        list.Pop();

        // Parse the body
        TokenList? bodyTokenList = list.FindBetweenBraces(TokenType.OpeningCurlyBracket, TokenType.ClosingCurlyBracket, Logger);

        if (bodyTokenList == null)
            LoggingManager.LogError("Invalid body after if statement condition");

        // Cut the body tokens from the list
        list.RemoveRange(..bodyTokenList.List.Count);

        // Remove the closing curly brace
        list.Pop();

        Transpiler.prefixes.Add(prefixForAllBodyStatements);

        // Parse the statements in the body
        Statement.ParseMultiple(ref bodyTokenList);

        Transpiler.prefixes.Remove(prefixForAllBodyStatements);

        if (list.List.Count <= 0 || list.Peek().RawContent != "else")
            return true;


        #region Else Block Parsing

        // Remove the else keyword
        list.Pop();

        if (!list.StartsWith(TokenType.OpeningCurlyBracket))
            LoggingManager.LogError("Expected opening curly brace after else keyword");
        list.Pop();

        // Parse the body
        bodyTokenList = list.FindBetweenBraces(TokenType.OpeningCurlyBracket, TokenType.ClosingCurlyBracket, Logger);

        if (bodyTokenList == null)
            LoggingManager.LogError("Invalid body after else keyword");

        // Cut the body tokens from the list
        list.RemoveRange(..bodyTokenList.List.Count);

        // Remove the closing curly brace
        list.Pop();

        // Set the prefix to the else block
        prefixForAllBodyStatements = $"execute unless data storage {MemoryManagement.MemoryTag} {{variables:{{{value.AsVarnameProvider()}:1}}}} run ";
        Transpiler.prefixes.Add(prefixForAllBodyStatements);

        // Parse the statements in the body
        Statement.ParseMultiple(ref bodyTokenList);


        // Remove the prefix
        Transpiler.prefixes.Remove(prefixForAllBodyStatements);

        #endregion


        return true;
    }
}