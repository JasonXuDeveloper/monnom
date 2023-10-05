// Class.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 NomLib

using System.Collections.Generic;

namespace NomLib.Definition;

public struct Class
{
    public string FullQualifiedName;
    public Super FullQualifiedSuperClass;
    public List<Super> FullQualifiedSuperInterfaces;
    public bool IsInterface;
    public bool IsExpando;
    public bool IsShape;
    public string Visibility;
    public List<Field> Fields;
    public List<TypeParameter> TypeParameters;
    public List<Constructor> Constructors;
    public List<Method> Methods;
}