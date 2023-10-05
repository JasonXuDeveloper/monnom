// Method.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 NomLib

using System.Collections.Generic;

namespace NomLib.Definition;

public struct Method
{
    public string Name;
    public string Visibility;
    public bool IsStatic;
    public bool IsVirtual;
    public bool IsFinal;
    public List<TypeParameter> TypeParameters;
    public Type ReturnType;
    public List<TypeParameter> Params;
}