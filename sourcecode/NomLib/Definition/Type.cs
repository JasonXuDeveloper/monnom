// Type.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 Nom

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NomLib.Definition;

public struct Type
{
    public string Kind;
    public int Index;
    public string LibraryName;
    public string FullQualifiedName;
    public List<Type> Arguments;

    public override string ToString()
    {
        switch (Kind)
        {
            case "Var":
                return Index.ToString();
            case "Class":
            case "Interface":
                return
                    $"{LibraryName}:{FullQualifiedName}" +
                    $"[{string.Join(",", Arguments?.Select(a => a.ToString()) ??
                                         Array.Empty<string>())}]";
            case "Top":
            case "Bottom":
            case "Dynamic":
                return Kind;
            case "Maybe":
                return $"{Arguments.First()}?";
            default:
                throw new InvalidDataException($"Invalid Kind {Kind}");
        }
    }
}