using System.Text;
using FuncScript.Internal;
using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class FuncVector : FuncScriptValue
{
    public FuncSharp.Core.Vector Value { get; set; }

    [FuncPropertyList]
    public static Dictionary<string, Type> Properties { get; }= new Dictionary<string, Type>()
    {
        {"x", typeof(FuncNumber)},
        {"y", typeof(FuncNumber)},
        {"z", typeof(FuncNumber)},
    };

    public FuncVector(FuncSharp.Core.Vector value)
    {
        Value = value;
    }

    public override string Generate()
    {
        return $"[{Value.X}d,{Value.Y}d,{Value.Z}d]";
    }
    
    public static bool TryParse(ref TokenList list, out FuncVector result)
    {
        if (!list.StartsWith(TokenType.Keyword) || list.Peek().RawContent != "vector_c")
        {
            goto error;
        }
        // Remove the vector keyword
        list.Pop();
        
        if(list.Peek().Type != TokenType.OpeningSquareBrace)
        {
            goto error;
        }
        // Remove the opening bracket
        list.Pop();

        if (Parse(ref list) is not FuncNumber xValue)
        {
            LoggingManager.LogError("Invalid first parameter for vector_c. Expected constant number.");
            goto error;
        }
        list.TrimStart(TokenType.Comma);

        if (Parse(ref list) is not FuncNumber yValue)
        {
            LoggingManager.LogError("Invalid second parameter for vector_c. Expected constant number.");
            goto error;
        }
        list.TrimStart(TokenType.Comma);

        if (Parse(ref list) is not FuncNumber zValue)
        {
            LoggingManager.LogError("Invalid third parameter for vector_c. Expected constant number.");
            goto error;
        }

        result = new(new(float.Parse(xValue.Value), float.Parse(yValue.Value), float.Parse(zValue.Value)));

        if(!list.StartsWith(TokenType.ClosingSquareBrace))
        {
            LoggingManager.LogError("Invalid vector_c. Expected closing square brace.");
            goto error;
        }

        list.Pop();
        
        return true;
        
        error:
        
        result = null;
        return false;
    }
    
    

    public static string VectorAdd(string addend1, string addent2, string output)
    {
        StringBuilder cmd = new();

        // Copy the first vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addend1}[0]", "ax"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addend1}[1]", "ay"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addend1}[2]", "az"));
        
        // Copy the second vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addent2}[0]", "bx"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addent2}[1]", "by"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addent2}[2]", "bz"));
        
        // Add the vector components
        cmd.AppendLine(Computation.Add("ax", "bx", "outx"));
        cmd.AppendLine(Computation.Add("ay", "by", "outy"));
        cmd.AppendLine(Computation.Add("az", "bz", "outz"));
        
        // Move the individual components to storage
        cmd.AppendLine(MemoryManagement.MoveToStorage("vectorcopy.x", "outx"));
        cmd.AppendLine(MemoryManagement.MoveToStorage("vectorcopy.y", "outy"));
        cmd.AppendLine(MemoryManagement.MoveToStorage("vectorcopy.z", "outz"));
        
        // Write the components back into a single vector
        cmd.AppendLine($"data remove storage {MemoryManagement.MemoryTag} variables.{output}");
        cmd.AppendLine($"data modify storage {MemoryManagement.MemoryTag} variables.{output} insert 0 from storage {MemoryManagement.MemoryTag} variables.vectorcopy.x");
        cmd.AppendLine($"data modify storage {MemoryManagement.MemoryTag} variables.{output} insert 1 from storage {MemoryManagement.MemoryTag} variables.vectorcopy.y");
        cmd.AppendLine($"data modify storage {MemoryManagement.MemoryTag} variables.{output} insert 2 from storage {MemoryManagement.MemoryTag} variables.vectorcopy.z");

        return cmd.ToString();
    }

}