using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class MathematicOperator : Statement, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<MathematicOperator>(TokenType.OpeningBrace).Register();
    }

    protected override bool CutTokensManually() => true;

    private VariableNameProvider _variableNameProvider;

    protected override bool OnParse(ref TokenList list)
    {
        // Remove "op" keyword
        //list.Pop();

        // Remove opening brace
        list.Pop();

        Statement left = Parse(ref list)!;

        if (list.Peek().Type is not (TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide))
            LoggingManager.LogError("Expected operator, got " + list.Peek().RawContent, LineNumber);

        // Remove operator
        TokenType operation = list.Pop().Type;

        Statement right = Parse(ref list)!;

        // Move data to the computation scoreboard
        MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) left.Execute()).VariableName, "a").Add();
        MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) right.Execute()).VariableName, "b").Add();

        // Execute operation and save the result in c on the computation scoreboard
        (operation switch
        {
            TokenType.Plus => Computation.Add("a", "b", "c"),
            TokenType.Minus => Computation.Subtract("a", "b", "c"),
            TokenType.Multiply => Computation.Multiply("a", "b", "c"),
            TokenType.Divide => Computation.Divide("a", "b", "c"),
        }).Add();

        // Get a name for the result variable
        string variableName = IdManager.GetId();

        // Move result to the variable scoreboard
        MemoryManagement.MoveToStorage(variableName, "c").Add();


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