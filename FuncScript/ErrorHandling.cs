using System;

namespace FuncScript;

public static class ErrorHandling
{
    internal static void ThrowError(string message)
    {
        Console.WriteLine($"Error: {message}");
        Environment.Exit(1);
    }
    
    internal static void ThrowError(string message, int line)
    {
        Console.WriteLine($"Error: {message} at line {line}");
        Environment.Exit(1);
    }
}