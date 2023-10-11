using System;
using System.Collections.Generic;
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
    public static Dictionary<string, Type> Properties { get; }= new()
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
        
        if(list.Peek().Type != TokenType.OpeningSquareBracket)
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

        if(!list.StartsWith(TokenType.ClosingSquareBracket))
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
    
    

    public static string VectorAdd(string addend1, string addent2, string output, float scale = 1)
    {
        StringBuilder cmd = new();

        // Copy the first vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addend1}[0] {scale}", "ax"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addend1}[1] {scale}", "ay"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addend1}[2] {scale}", "az"));
        
        // Copy the second vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addent2}[0] {scale}", "bx"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addent2}[1] {scale}", "by"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{addent2}[2] {scale}", "bz"));
        
        // Add the vector components
        cmd.AppendLine(Computation.Add("ax", "bx", "outx"));
        cmd.AppendLine(Computation.Add("ay", "by", "outy"));
        cmd.AppendLine(Computation.Add("az", "bz", "outz"));
        
        // Move the individual components to storage
        cmd.AppendLine(MemoryManagement.MoveToStorage("vectorcopy.x", "outx", scale:1/scale));
        cmd.AppendLine(MemoryManagement.MoveToStorage("vectorcopy.y", "outy", scale:1/scale));
        cmd.AppendLine(MemoryManagement.MoveToStorage("vectorcopy.z", "outz", scale:1/scale));
        
        // Write the components back into a single vector
        cmd.AppendLine($"data remove storage {MemoryManagement.MemoryTag} variables.{output}");
        cmd.AppendLine($"data modify storage {MemoryManagement.MemoryTag} variables.{output} insert 0 from storage {MemoryManagement.MemoryTag} variables.vectorcopy.x");
        cmd.AppendLine($"data modify storage {MemoryManagement.MemoryTag} variables.{output} insert 1 from storage {MemoryManagement.MemoryTag} variables.vectorcopy.y");
        cmd.AppendLine($"data modify storage {MemoryManagement.MemoryTag} variables.{output} insert 2 from storage {MemoryManagement.MemoryTag} variables.vectorcopy.z");

        return cmd.ToString();
    }
    
    public static string VectorSubtract(string minuend, string subtrahend, string output)
    {
        StringBuilder cmd = new();

        // Copy the first vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{minuend}[0]", "ax"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{minuend}[1]", "ay"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{minuend}[2]", "az"));
        
        // Copy the second vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{subtrahend}[0]", "bx"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{subtrahend}[1]", "by"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{subtrahend}[2]", "bz"));
        
        // Subtract the vector components
        cmd.AppendLine(Computation.Subtract("ax", "bx", "outx"));
        cmd.AppendLine(Computation.Subtract("ay", "by", "outy"));
        cmd.AppendLine(Computation.Subtract("az", "bz", "outz"));
        
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
    
    public static string VectorMultiply(string factor1, string factor2, string output)
    {
        StringBuilder cmd = new();

        // Copy the first vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{factor1}[0]", "ax"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{factor1}[1]", "ay"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{factor1}[2]", "az"));
        
        // Copy the second vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{factor2}[0]", "bx"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{factor2}[1]", "by"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{factor2}[2]", "bz"));
        
        // Multiply the vector components
        cmd.AppendLine(Computation.Multiply("ax", "bx", "outx"));
        cmd.AppendLine(Computation.Multiply("ay", "by", "outy"));
        cmd.AppendLine(Computation.Multiply("az", "bz", "outz"));
        
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
    
    public static string VectorDivide(string dividend, string divisor, string output)
    {
        StringBuilder cmd = new();

        // Copy the first vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{dividend}[0]", "ax"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{dividend}[1]", "ay"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{dividend}[2]", "az"));
        
        // Copy the second vector to the scoreboard
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{divisor}[0]", "bx"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{divisor}[1]", "by"));
        cmd.AppendLine(MemoryManagement.MoveToComputationScoreboard($"{divisor}[2]", "bz"));
        
        // Divide the vector components
        cmd.AppendLine(Computation.Divide("ax", "bx", "outx"));
        cmd.AppendLine(Computation.Divide("ay", "by", "outy"));
        cmd.AppendLine(Computation.Divide("az", "bz", "outz"));
        
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