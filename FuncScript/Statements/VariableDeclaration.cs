using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class VariableDeclaration : Statement, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<VariableDeclaration>(TokenType.Keyword, TokenType.Keyword).AddCustomParser(list => Value.ParseTypeKeyword(list.Peek()) != null).Register();
    }

    protected override bool CutTokensManually() => true;

    public override bool OnParse(ref TokenList list)
    {
        Type type = Value.ParseTypeKeyword(list.Pop())!;
        string name = list.Peek().RawContent;

        if (Transpiler.MemoryTypes.ContainsKey(name))
        {
            LoggingManager.LogError($"Variable '{name}' is already declared.");
            return false;
        }
        
        Transpiler.MemoryTypes[name] = type;

        if (list.Peek(1).Type == TokenType.Colon)
        {
            LoggingManager.LogError("Variable declaration can't declare a member of a class.");
            return false;
        }

        // After matching the variable name with the type, we can just parse it as a Variable setter
        if (list.Peek(1).Type == TokenType.Equals)
        {
            VariableSetter setter = new();
            setter.OnParse(ref list);
        }
        else
        {
            // Pop the variable name
            list.Pop();
        }

        return true;
    }
}