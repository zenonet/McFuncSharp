using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class VariableCall : Statement, IInitializable
{
    public static List<string> Variables = new();

    public string VariableName;

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


    public override bool OnParse(ref TokenList list)
    {
        VariableName = list.Pop().RawContent;

        // If this is an accessor to a property of the variable, we need to parse that as well
        while (list.List.Count > 1 && list.Peek().Type == TokenType.Dot && list.Peek(1).Type == TokenType.Keyword)
        {
            if (list.List.Count > 2 && list.Peek(2).Type == TokenType.OpeningParenthesis)
                break;
            // TODO: Add indexer support
            list.Pop();
            VariableName += $".{list.Pop().RawContent}";
        }

        // Allow for using xyz as fields of vectors by translating them to indexers
        if (VariableName.Contains(".") && Transpiler.MemoryTypes[VariableName[..VariableName.IndexOf('.')]] == typeof(FuncVector))
        {
            if (VariableName[(VariableName.LastIndexOf('.') + 1)..] is "x" or "y" or "z")
            {
                VariableName = VariableName[..VariableName.LastIndexOf('.')] + (VariableName[(VariableName.LastIndexOf('.') + 1)..] switch
                {
                    "x" => "[0]",
                    "y" => "[1]",
                    "z" => "[2]",
                });
                // Quick and dirty solution to make this indexer pass the variable type checks
                if (VariableName.Contains('['))
                {
                    Transpiler.MemoryTypes[VariableName] = typeof(FuncNumber);
                }
            }
        }

        // Ensure that the variable has been declared already
        string baseVariableName = VariableName.Contains('.') ? VariableName[..VariableName.IndexOf('.')] : VariableName;
        if (!Transpiler.MemoryTypes.ContainsKey(baseVariableName))
            LoggingManager.LogError($"The variable '{baseVariableName}' hasn't been declared yet. Declare it using the type keyword followed by the variable name.");

        return true;
    }

    public override Value Execute()
    {
        return new VariableNameProvider(VariableName);
    }
}