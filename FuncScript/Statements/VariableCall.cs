using FuncScript.Types;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class VariableCall : Statement, IInitializable
{
    public static List<string> Variables = new();

    private string VariableName;

    public static void Initialize()
    {
        StatementRegistration.Create<VariableCall>(list =>
            {
                // Check if that variable exists
                return Variables.Contains(list.Peek().RawContent);
            }, TokenType.Keyword
        ).Register();
    }

    protected override bool CutTokensManually()
    {
        return true;
    }


    protected override bool OnParse(ref TokenList list)
    {
        VariableName = list.Pop().RawContent;

        // If this is an accessor to a property of the variable, we need to parse that as well
        while (list.List.Count > 1 && list.Peek().Type == TokenType.Dot && list.Peek(1).Type == TokenType.Keyword)
        {
            // TODO: Add indexer support
            list.Pop();
            VariableName += $".{list.Pop().RawContent}";
        }

        return true;
    }

    public override Value Execute()
    {
        return new VariableNameProvider(VariableName);
    }
}