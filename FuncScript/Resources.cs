﻿using System;
using System.Collections.Generic;
using FuncSharp.Commands;

namespace FuncScript;

public static class Resources
{
    public static readonly Dictionary<string, Func<string[], string>> Functions = new()
    {/*
        {
            "kill", () =>
            {
                return "kill @e[tag=]";
            }
        },*/
        /*{
            "fill", varname =>
            {
                return $"fill {varname} ~ ~ ~ air";
            }
        },*/
        {
            "say", varname =>
            {
                return $"tellraw @a {{\"storage\":\"funcscript_memory\",\"nbt\":\"variables.{varname[0]}\"}}";
            }
        },
    };
}