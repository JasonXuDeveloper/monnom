// LlvmFile.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 Nom

using System.IO;

namespace Nom.Bytecode;

public class LlvmFile: INativeFile
{
    public FileInfo Path { get; set; }
    public Platform Platform { get; set; }

    public LlvmFile(FileInfo path, Platform platform = Platform.x64)
    {
        Path = path;
        Platform = platform;
    }
}