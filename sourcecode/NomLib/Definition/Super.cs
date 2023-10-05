// Super.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 Nom

using System.Linq;

namespace NomLib.Definition;

public struct Super
{
    public string LibraryName;
    public string FullQualifiedName;
    public Type[] TypeArguments;
    
    public Type ToType()
    {
        return new Type()
        {
            Kind = "Class",
            LibraryName = LibraryName,
            FullQualifiedName = FullQualifiedName,
            Arguments = TypeArguments.ToList()
        };
    }
    
    public override string ToString()
    {
        return ToType().ToString();
    }
}