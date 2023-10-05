// Field.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 NomLib

namespace NomLib.Definition;

public struct Field
{
    public string Name;
    public Type Type;
    public string Visibility;
    public bool IsStatic;
    public bool IsReadOnly;
    public bool IsVolatile;
}