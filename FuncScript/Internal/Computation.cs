using System.Diagnostics.Contracts;

namespace FuncScript.Internal;

public static class Computation
{
    public const string ComputationScoreboard = "funcscript_computation";

    public static string Initialize()
    {
        return "scoreboard objectives add " + ComputationScoreboard + " dummy";
    }

    [Pure]
    public static string Copy(string source, string target)
    {
        return "scoreboard players operation " + target + " " + ComputationScoreboard + " = " + source + " " + ComputationScoreboard;
    }

    [Pure]
    public static string Add(string addent1, string addent2, string output)
    {
        return Copy(addent1, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " += " + addent2 + " " + ComputationScoreboard;
    }

    [Pure]
    public static string Subtract(string minuend, string subtractor, string output)
    {
        return Copy(minuend, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " -= " + subtractor + " " + ComputationScoreboard;
    }

    [Pure]
    public static string Multiply(string factor1, string factor2, string output)
    {
        return Copy(factor1, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " *= " + factor2 + " " + ComputationScoreboard;
    }

    [Pure]
    public static string Divide(string divident, string divisor, string output)
    {
        return Copy(divident, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " /= " + divisor + " " + ComputationScoreboard;
    }

    [Pure]
    public static string Modulo(string divident, string divisor, string output)
    {
        return Copy(divident, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " %= " + divisor + " " + ComputationScoreboard;
    }

    #region Comparison

    [Pure]
    public static string Equal(string a, string b, string output)
    {
        return $"execute if score {a} {ComputationScoreboard} = {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 1\n" +
               $"execute unless score {a} {ComputationScoreboard} = {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 0";
    }

    [Pure]
    public static string NotEqual(string a, string b, string output)
    {
        return $"execute if score {a} {ComputationScoreboard} = {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 0\n" +
               $"execute unless score {a} {ComputationScoreboard} = {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 1";
    }

    [Pure]
    public static string GreaterThan(string a, string b, string output)
    {
        return $"execute if score {a} {ComputationScoreboard} > {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 1\n" +
               $"execute unless score {a} {ComputationScoreboard} > {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 0";
    }

    [Pure]
    public static string GreaterThanOrEqual(string a, string b, string output)
    {
        return $"execute if score {a} {ComputationScoreboard} >= {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 1\n" +
               $"execute unless score {a} {ComputationScoreboard} >= {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 0";
    }

    [Pure]
    public static string LessThan(string a, string b, string output)
    {
        return $"execute if score {a} {ComputationScoreboard} < {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 1\n" +
               $"execute unless score {a} {ComputationScoreboard} < {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 0";
    }

    [Pure]
    public static string LessThanOrEqual(string a, string b, string output)
    {
        return $"execute if score {a} {ComputationScoreboard} <= {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 1\n" +
               $"execute unless score {a} {ComputationScoreboard} <= {b} {ComputationScoreboard} run scoreboard players set {output} {ComputationScoreboard} 0";
    }

    #endregion
}