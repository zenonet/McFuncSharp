using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;

namespace FuncScript.Statements;

public class EqualityOperator : Operator, IInitializable
{
    public static void Initialize()
    {
        StatementExtensionRegistration.CreateStatementExtensionRegistration<Statement, EqualityOperator>(TokenType.Equals, TokenType.Equals);
    }

    public override void OnParse(ref TokenList list, Statement baseStatement)
    {
        // Remove the 2 tokens that make up the operator
        list.Pop();
        list.Pop();
        
        // Create a new statement for the right side of the operator
        Statement? right = Parse(ref list);
        
        // If the right side is null, throw an exception
        if (right == null)
            throw new Exception("Expected expression after operator");
        
        
    }
}