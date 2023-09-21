using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class EqualityOperator : Statement, IInitializable
{
    
    public static void Initialize()
    {
        StatementRegistration.Create<EqualityOperator>(TokenType.OpeningBrace).AddPriority(2).Register();
    }

    protected override bool CutTokensManually() => true;

    private VariableNameProvider _variableNameProvider;

    public override bool OnParse(ref TokenList list)
    {
        // Remove "op" keyword
        //list.Pop();

        // Remove opening brace
        list.Pop();

        Statement left = Parse(ref list)!;

        if (list.Peek().Type is not (TokenType.GreaterThan or TokenType.Equals or TokenType.LessThan))
            return false;

        if(list.Peek().Type == TokenType.Equals)
        {
            list.Pop();
            
            if (list.Peek().Type != TokenType.Equals)
                LoggingManager.LogError("Expected second equals sign after first equals sign in equality operator");
        }

        TokenType operation = list.Pop().Type;
        

        Statement right = Parse(ref list)!;

        // Move data to the computation scoreboard
        MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) left.Execute()).VariableName, "a").Add();
        MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) right.Execute()).VariableName, "b").Add();

        // Execute operation and save the result in c on the computation scoreboard
        (operation switch
        {
            TokenType.GreaterThan => Computation.GreaterThan("a", "b", "c"),
            TokenType.LessThan => Computation.LessThan("a", "b", "c"),
            TokenType.Equals => Computation.Equal("a", "b", "c"),
        }).Add();

        // Get a name for the result variable
        string variableName = IdManager.GetDataId();

        // Move result to the variable scoreboard
        MemoryManagement.MoveToStorage(variableName, "c", "byte").Add();


        // Create a variable name provider for the result
        _variableNameProvider = new(variableName);

        // Remove closing brace
        list.Pop();

        return true;
    }

    public override Value Execute()
    {
        return _variableNameProvider;
    }
}