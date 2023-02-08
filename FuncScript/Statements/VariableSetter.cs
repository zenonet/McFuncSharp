using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;

namespace FuncScript.Statements;

public class VariableSetter : Statement, IInitializable
{
    public string variableName;

    public static void Initialize()
    {
        StatementRegistration.Create<VariableSetter>(TokenType.Keyword).AddCustomParser(list =>
        {
            for (int i = 0; i < list.List.Count; i++)
            {
                // Ensure that dots and keywords are alternating
                if (list.Peek(i).Type != (i % 2 == 0 ? TokenType.Keyword : TokenType.Dot))
                {
                    // If that is not the case, there is the option that it is an indexer
                    // TODO: Implement indexer detection
                    
                    
                    // Of course, the last option is that the path ends here and there is one equal sign
                    return list.Peek(i).Type == SlowLang.Engine.Tokens.TokenType.Equals && list.Peek(i+1).Type != SlowLang.Engine.Tokens.TokenType.Equals;
                }
            }
            
            
            return false;
        }).AddPriority(1).Register();
    }

    protected override bool CutTokensManually() => true;

    protected override bool OnParse(ref TokenList list)
    {
        variableName = list.Pop().RawContent;

        // If this is an accessor to a property of the variable, we need to parse that as well
        while (list.List.Count > 1 && list.Peek().Type == TokenType.Dot && list.Peek(1).Type == TokenType.Keyword)
        {
            // TODO: Add indexer support
            list.Pop();
            variableName += $".{list.Pop().RawContent}";
        }

        list.Pop(); // Equals

        Statement? value = Parse(ref list);


        if (value == null)
            return false;

        MemoryManagement.MoveVariable(((VariableNameProvider) value.Execute()).VariableName, variableName).Add();
        VariableCall.Variables.Add(variableName);
        return true;
    }
}