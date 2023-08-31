// Compiler.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 Nom

using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace NomLib;

public static class Compiler
{
    public static void CompileLibrary(string json, DirectoryInfo outputPath)
    {
        //deserialize json
        var libDef = JsonConvert.DeserializeObject<NomLibDef>(json);

        //get cpp files
        var input = new DirectoryInfo(Path.Combine(outputPath.FullName, libDef.LibraryName, "cpp"));
        if (!input.Exists)
        {
            Console.WriteLine($"Directory {input.FullName} does not exist, please generate first");
            return;
        }

        //create output
        var output = new DirectoryInfo(Path.Combine(outputPath.FullName, libDef.LibraryName, "llvm"));
        if (!output.Exists)
        {
            output.Create();
            Console.WriteLine($"Created directory {output.FullName}");
        }

        //classes
        foreach (var defClass in libDef.Classes)
        {
            //corresponded cpp file
            var dir = new DirectoryInfo(Path.Combine(input.FullName, defClass.FullQualifiedName));
            if (!dir.Exists)
            {
                Console.WriteLine($"Directory {dir.FullName} does not exist, please generate first");
                return;
            }

            //check generate
            var className = defClass.FullQualifiedName;
            var filename = $"{className.ToLowerInvariant().Replace(".", "_")}.cpp";
            var cppFile = new FileInfo(Path.Combine(dir.FullName, filename));
            if (!cppFile.Exists)
            {
                Console.WriteLine($"File {cppFile.FullName} does not exist, please generate first");
                return;
            }

            //compile
            Console.WriteLine($"Compiling {defClass.FullQualifiedName}");
            var args =
                $"-S -std=c++17 -emit-llvm \"{cppFile.FullName}\" -o \"{Path.Combine(output.FullName, cppFile.Name)}.ll\"";
            Console.WriteLine($"clang {args}");
            Process clangProc = new Process();
            clangProc.StartInfo.UseShellExecute = true;
            clangProc.StartInfo.FileName = "clang";
            clangProc.StartInfo.CreateNoWindow = true;
            clangProc.StartInfo.Arguments = args;
            clangProc.Start();
            clangProc.WaitForExit();
            Console.WriteLine(clangProc.ExitCode == 0
                ? $"Compiled {cppFile.Name}.ll"
                : $"Failed to compile {cppFile.Name}.ll");
            Console.WriteLine("==================================");
        }
    }
}