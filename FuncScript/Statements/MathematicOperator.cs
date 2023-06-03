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
            return false;

        // Remove operator
        TokenType operation = list.Pop().Type;

        Statement right = Parse(ref list)!;

        FuncScriptValue leftValue = (FuncScriptValue) left.Execute();
        FuncScriptValue rightValue = (FuncScriptValue) right.Execute();

        // Get a name for the result variable
        string variableName = IdManager.GetId();
        // Create a variable name provider for the result
        _variableNameProvider = new(variableName);

        if (leftValue.IsOfType<FuncNumber>() && rightValue.IsOfType<FuncNumber>())
        {
            // Move data to the computation scoreboard
            MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) leftValue).VariableName, "a").Add();
            MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) rightValue).VariableName, "b").Add();

            // Execute operation and save the result in c on the computation scoreboard
            (operation switch
            {
                TokenType.Plus => Computation.Add("a", "b", "c"),
                TokenType.Minus => Computation.Subtract("a", "b", "c"),
                TokenType.Multiply => Computation.Multiply("a", "b", "c"),
                TokenType.Divide => Computation.Divide("a", "b", "c"),
            }).Add();

            // Move result to the variable scoreboard
            MemoryManagement.MoveToStorage(variableName, "c").Add();
            Transpiler.MemoryTypes.Add(variableName, typeof(FuncNumber));
        }
        else if (leftValue.IsOfType<FuncVector>() && rightValue.IsOfType<FuncVector>())
        {
            // Execute operation and save the result
            (operation switch
            {
                TokenType.Plus => FuncVector.VectorAdd(leftValue.AsVarnameProvider(), rightValue.AsVarnameProvider(), variableName),
                TokenType.Minus => FuncVector.VectorSubtract(leftValue.AsVarnameProvider(), rightValue.AsVarnameProvider(), variableName),
                TokenType.Multiply => FuncVector.VectorMultiply(leftValue.AsVarnameProvider(), rightValue.AsVarnameProvider(), variableName),
                TokenType.Divide => FuncVector.VectorDivide(leftValue.AsVarnameProvider(), rightValue.AsVarnameProvider(), variableName),
            }).Add();

            Transpiler.MemoryTypes.Add(variableName, typeof(FuncVector));
        }
        else if (leftValue.IsOfType<FuncVector>() && rightValue.IsOfType<FuncNumber>() || leftValue.IsOfType<FuncNumber>() && rightValue.IsOfType<FuncVector>())
        {
            FuncScriptValue number = leftValue.IsOfType<FuncNumber>() ? leftValue : rightValue;
            FuncScriptValue vector = leftValue.IsOfType<FuncVector>() ? leftValue : rightValue;

            Transpiler.MemoryTypes.Add(variableName, typeof(FuncVector));

            if (operation is TokenType.Multiply)
            {
                MulitplyVectorWithNumber(vector.AsVarnameProvider(), number.AsVarnameProvider(), variableName).Add();
            }
            else
            {
                LoggingManager.LogError($"Unable to apply the operation {operation} to a vector and a number");
            }
        }
        else
        {
            LoggingManager.LogError($"Unable to apply the {operation} operator to {leftValue.GetFuncTypeName()} and {rightValue.GetFuncTypeName()}");
        }

        // Remove closing brace
        list.Pop();

        return true;
    }

    private string MulitplyVectorWithNumber(string vector, string number, string variableName)
    {
        return
            // Create a vector with the number in all components
            $"data remove storage {MemoryManagement.MemoryTag} variables.vectorized_number\n" +
            $"data modify storage {MemoryManagement.MemoryTag} variables.vectorized_number insert 0 from storage {MemoryManagement.MemoryTag} variables.{number}\n" +
            $"data modify storage {MemoryManagement.MemoryTag} variables.vectorized_number insert 1 from storage {MemoryManagement.MemoryTag} variables.{number}\n" +
            $"data modify storage {MemoryManagement.MemoryTag} variables.vectorized_number insert 2 from storage {MemoryManagement.MemoryTag} variables.{number}\n" +
            // Multiply the 2 vectors
            FuncVector.VectorMultiply(vector, "vectorized_number", variableName);
    }

    public override Value Execute()
    {
        return _variableNameProvider;
    }
}