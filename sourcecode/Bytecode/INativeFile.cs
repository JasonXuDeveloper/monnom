// INativeFile.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 Nom

using System.IO;

namespace Nom.Bytecode;

public interface INativeFile
{
    public FileInfo Path { get; set; }
    public Platform Platform { get; set; }
}