﻿using System;
using System.Diagnostics.CodeAnalysis;
using FuncSharp.Core;
using SlowLang.Engine;
using SlowLang.Engine.Tokens;

namespace FuncScript.Types;

public class FuncEntityType : ConstFuncScriptValue
{
    public Entity Value { get; }

    public FuncEntityType(Entity value)
    {
        Value = value;
    }

    public static string GetKeyword() => "EntityType";

    public static bool TryParse(ref TokenList list, [MaybeNullWhen(false)] out FuncEntityType result)
    {
        if (list.Peek().Type != TokenType.Keyword || list.Peek().RawContent != "entity")
        {
            result = null;
            return false;
        }

        list.Pop();

        if (!list.StartsWith(TokenType.Dot))
        {
            result = null;
            return false;
        }

        list.Pop();

        if (!Enum.TryParse(list.Peek().RawContent, out Entity entity))
        {
            LoggingManager.LogError("Unknown entity type: " + list.Peek().RawContent);
            result = null;
            return false;
        }

        list.Pop();
        result = new(entity);

        return true;
    }

    public override string Generate()
    {
        return $"{Value}";
    }
}