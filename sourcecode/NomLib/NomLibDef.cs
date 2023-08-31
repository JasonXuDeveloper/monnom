// NomLibDef.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 NomLib

using System.Collections.Generic;
using NomLib.Definition;

namespace NomLib;

public class NomLibDef
{
    public string LibraryName;
    public string LibraryVersion;
    public List<string> RuntimeVersion;
    public List<Reference> References;
    public List<Class> Classes;
}