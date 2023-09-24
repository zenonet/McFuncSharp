using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class Operator : StatementExtension, IInitializable
{
    public static void Initialize()
    {
        StatementExtensionRegistration.CreateStatementExtensionRegistration<Statement, Operator>(
            list => list.List.Count != 0 && list.Peek().Type.IsMathematicOperator()
        ).Register();
    }

    protected override bool CutTokensManually() => true;

    private VariableNameProvider variableNameProvider;

    private static readonly HashSet<Statement> ChildStatements = new();
    private TokenType Operation { get; set; }

    private Statement baseStatement = null!;
    private Statement rightSide = null!;

    public override bool OnParse(ref TokenList list, Statement baseStatement)
    {
        this.baseStatement = baseStatement;
        Operation = list.Pop().Type;

        Statement? rightSideOrNull = Statement.Parse(ref list, onStatementInstantiated: statement => { ChildStatements.Add(statement); });
        if (rightSideOrNull == null)
            return false;

        rightSide = rightSideOrNull;

        ChildStatements.Remove(rightSide);

        // If this is not the root operator, we don't need to do anything
        if (ChildStatements.Contains(baseStatement))
            return true;

        // If this is the root operator, we need to parse the whole expression:

        // Get all statements and operations that are part of this expression
        ExtractExpression(baseStatement, out List<Statement> statements, out List<TokenType> operations);

        // Merge all operations by priority
        while (statements.Count > 1)
        {
            MergeOperation(operations, statements);
        }

        Statement expression = statements[0];
        variableNameProvider = (VariableNameProvider) expression.Execute();

        return true;
    }

    private void MergeOperation(List<TokenType> operations, List<Statement> statements)
    {
        int highestPriority = 0;
        int highestPriorityOperationIndex = -1;
        for (int i = operations.Count - 1; i >= 0; i--)
        {
            int priority = GetOperatorPriority(operations[i]);

            // The priority does not need to be higher than max, because we are parsing in reverse meaning that the last operator is the first one to be executed
            if (priority >= highestPriority)
            {
                highestPriority = priority;
                highestPriorityOperationIndex = i;
            }
        }

        // Combine the highest priority operation with the statements around it
        ExpressionCompound compound = new(statements[highestPriorityOperationIndex], statements[highestPriorityOperationIndex + 1], operations[highestPriorityOperationIndex]);
        statements.RemoveAt(highestPriorityOperationIndex);
        statements.RemoveAt(highestPriorityOperationIndex);
        operations.RemoveAt(highestPriorityOperationIndex);
        statements.Insert(highestPriorityOperationIndex, compound);
    }

    private void ExtractExpression(Statement leftmostSide, out List<Statement> statements, out List<TokenType> operations)
    {
        Statement r = rightSide;
        statements = new();
        operations = new();
        statements.Add(leftmostSide);
        operations.Add(Operation);
        while (r is Operator mathematicOperator)
        {
            operations.Add(mathematicOperator.Operation);
            statements.Add(mathematicOperator.baseStatement);

            r = mathematicOperator.rightSide;
        }

        statements.Add(r);
    }

    private int GetOperatorPriority(TokenType operation)
    {
        return operation switch
        {
            TokenType.Multiply or TokenType.Divide => 2,
            TokenType.Plus or TokenType.Minus => 1,
            TokenType.GreaterThan or TokenType.LessThan or TokenType.DoubleEquals => 0,
            _ => throw new ArgumentException("Invalid operation"),
        };
    }

    private static string MulitplyVectorWithNumber(string vector, string number, string variableName)
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

    private class ExpressionCompound : Statement
    {
        public readonly Statement LeftSide;
        public readonly TokenType Operation;
        public readonly Statement RightSide;
        private readonly string variableName;

        public ExpressionCompound(Statement leftSide, Statement rightSide, TokenType operation)
        {
            LeftSide = leftSide;
            RightSide = rightSide;
            Operation = operation;
            variableName = IdManager.GetDataId();
        }

        public override string ToString() => $"({LeftSide} {Operation} {RightSide})";

        public override Value Execute()
        {
            // This is an exception to the idea, that Statements should add their generated mcfunction code 

            FuncScriptValue leftValue = (FuncScriptValue) LeftSide.Execute();
            FuncScriptValue rightValue = (FuncScriptValue) RightSide.Execute();
            if (leftValue.IsOfType<FuncNumber>() && rightValue.IsOfType<FuncNumber>() && Operation is TokenType.GreaterThan or TokenType.LessThan or TokenType.DoubleEquals)
            {

                (Operation switch
                {
                    TokenType.GreaterThan => Computation.GreaterThan("a", "b", "c"),
                    TokenType.LessThan => Computation.LessThan("a", "b", "c"),
                    TokenType.DoubleEquals => Computation.Equal("a", "b", "c"),
                }).Add();

                Transpiler.MemoryTypes.Add(variableName, typeof(FuncBool));
                MemoryManagement.MoveToStorage(variableName, "c", "byte").Add();
            }
            else if (leftValue.IsOfType<FuncNumber>() && rightValue.IsOfType<FuncNumber>())
            {
                // Move data to the computation scoreboard
                MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) leftValue).VariableName, "a").Add();
                MemoryManagement.MoveToComputationScoreboard(((VariableNameProvider) rightValue).VariableName, "b").Add();

                // Execute operation and save the result in c on the computation scoreboard
                (Operation switch
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
                (Operation switch
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

                if (Operation is TokenType.Multiply)
                {
                    MulitplyVectorWithNumber(vector.AsVarnameProvider(), number.AsVarnameProvider(), variableName).Add();
                }
                else
                {
                    LoggingManager.LogError($"Unable to apply the operation {Operation} to a vector and a number");
                }
            }
            else
            {
                LoggingManager.LogError($"Unable to apply the {Operation} operator to {leftValue.GetFuncTypeName()} and {rightValue.GetFuncTypeName()}");
            }

            return new VariableNameProvider(variableName);
        }
    }

    public override Value Execute()
    {
        return variableNameProvider;
    }

    public override string ToString()
    {
        return Operation.ToString();
    }
}