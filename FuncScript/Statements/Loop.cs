using System.Text.RegularExpressions;
using FuncScript.Internal;
using SlowLang.Engine.Statements;

namespace FuncScript.Statements;

public class Loop : Statement
{
    public string LoopFunctionName = null!;
    
    public bool ContainsBreakStatement = false;

    protected string AddBreakabilityToLoopCode(string loopCode)
    {
        loopCode = new Regex("(\n).*?(?=\\S)").Replace(loopCode, $"$1execute unless data storage {MemoryManagement.MemoryTag} {{variables:{{break_from_{LoopFunctionName}:1b}}}} run ");
        return loopCode;
    }

    protected string AddBreakabilityIfNeccessary(string loopCode)
    {
        if (ContainsBreakStatement)
            return AddBreakabilityToLoopCode(loopCode);
        return loopCode;
    }
}