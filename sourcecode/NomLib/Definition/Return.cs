// Return.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 NomLib

namespace NomLib.Definition;

public struct Return
{
    /// <summary>
    /// can be 'FullQualifiedType', 'TypeVariable', 'Dynamic'
    /// </summary>
    public string Kind;
    
    /// <summary>
    /// for kind = 'FullQualifiedType'
    /// </summary>
    public string FullQualifiedType;
    
    /// <summary>
    /// for kind = 'TypeVariable', match the typeParameter from TypeParameters in Method
    /// </summary>
    public string TypeVariable;
}