// CodeGenerator.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 NomLib

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NomLib.Definition;
using NomLib.Misc;

namespace NomLib;

public static class CodeGenerator
{
    public static void GenerateLibraryTemplate(string json, DirectoryInfo outputPath)
    {
        //deserialize json
        var libDef = JsonConvert.DeserializeObject<NomLibDef>(json);

        //create output
        var output = new DirectoryInfo(Path.Combine(outputPath.FullName, libDef.LibraryName, "cpp"));
        if (!output.Exists)
        {
            output.Create();
            Console.WriteLine($"Created directory {output.FullName}");
        }

        //references 
        GenerateReferences(libDef, output);

        //classes
        foreach (var defClass in libDef.Classes)
        {
            var dir = new DirectoryInfo(Path.Combine(output.FullName, defClass.FullQualifiedName));
            if (!dir.Exists)
            {
                dir.Create();
                Console.WriteLine($"Created directory {dir.FullName}");
            }
            
            GenerateHeader(libDef, defClass, dir);
            GenerateImpl(defClass, dir);
        }
    }

    private static void GenerateReferences(NomLibDef libDef, DirectoryInfo output)
    {
        var refTemplate =
            $"""
//#include "monnom_lib.h"

{string.Join("\n", libDef.References?.Select(r => $"{Config.Decl} void* {r.Alias};") ??
                   Array.Empty<string>())}
""";

        var refFile = new FileInfo(Path.Combine(output.FullName,
            $"{libDef.LibraryName.ToLowerInvariant()}_references.h"));
        File.WriteAllText(refFile.FullName, refTemplate);
        Console.WriteLine($"Created file {refFile.FullName}");
    }

    private static void GenerateHeader(NomLibDef libDef, Class defClass, DirectoryInfo output)
    {
        var className = defClass.FullQualifiedName;

        //typeParameters
        var typeParams = string.Join("\n",
            Enumerable.Range(0, defClass.TypeParameters.Count)
                .Select(i => $"{Config.Decl} void* typeParameter_{defClass.TypeParameters[i].Name};"));

        //fields
        var fields = string.Join("\n",
            Enumerable.Range(0, defClass.Fields?.Count ?? 0)
                .Select(i =>
                {
                    var field = defClass.Fields[i];
                    var fieldName = $"{className.ToLowerInvariant().Replace(".", "_")}_{field.Name}";

                    return
                        $"{Config.Decl} void* {fieldName};";
                }));

        //constructors
        var ctors = string.Join("\n",
            Enumerable.Range(0, defClass.Constructors?.Count ?? 0)
                .Select(i =>
                {
                    var ctor = defClass.Constructors[i];
                    var ctorName = $"{className.ToLowerInvariant().Replace(".", "_")}_ctor_{i}";
                    List<(string type, string name)> ctorParams = ctor.Params?
                        .Select(ctorParam => (ctorParam.Type.FullQualifiedName.GetCppType(), ctorParam.Name)).ToList();

                    if (ctorParams == null) return $"{Config.Decl} void* {ctorName}();";

                    return
                        $"{Config.Decl} void* {ctorName}({string.Join(", ",
                            ctorParams.Select(p => $"{p.type} {p.name.ToCamelCase()}"))});";
                }));
        //empty ctor
        if (defClass.Constructors == null || defClass.Constructors.Count == 0)
        {
            ctors = $"{Config.Decl} void* {className.ToLowerInvariant().Replace(".", "_")}_ctor_0();";
        }

        //methods
        var methods = string.Join("\n",
            Enumerable.Range(0, defClass.Methods?.Count ?? 0)
                .Select(i =>
                {
                    var method = defClass.Methods[i];
                    var methodName = $"{className.ToLowerInvariant().Replace(".", "_")}_{method.Name}";
                    List<(string type, string name)> methodTypeParams = method.TypeParameters?
                        .Select(typeParam => ("void*", $"typeParameter_{typeParam.Name.ToCamelCase()}")).ToList();

                    List<(string type, string name)> methodParams = method.Params?.Select(methodParam =>
                        (methodParam.Type.FullQualifiedName.GetCppType(), methodParam.Name)).ToList();

                    var returnType = method.ReturnType.FullQualifiedName.GetCppType();

                    List<(string type, string name)> args = new();
                    if (!method.IsStatic)
                    {
                        args.Add(("void*", "instance"));
                    }

                    if (methodTypeParams != null) args.AddRange(methodTypeParams);
                    if (methodParams != null) args.AddRange(methodParams);

                    return
                        $"{Config.Decl} {returnType} {methodName}({string.Join(", ", args.Select(p => $"{p.type} {p.name.ToCamelCase()}"))});";
                }));

        var template =
            $"""
#include "../{libDef.LibraryName.ToLowerInvariant()}_references.h"

/*
Type Parameters
*/
{typeParams}

/*
Fields
*/
{fields}

/*
Constructors
*/
{ctors}

/*
Methods
*/
{methods}
""";

        var file = new FileInfo(Path.Combine(output.FullName,
            $"{className.ToLowerInvariant().Replace(".", "_")}.h"));
        File.WriteAllText(file.FullName, template);
        Console.WriteLine($"Created file {file.FullName}");
    }

    private static void GenerateImpl(Class defClass, DirectoryInfo output)
    {
        var className = defClass.FullQualifiedName;

        //constructors
        var ctors = string.Join("\n",
            Enumerable.Range(0, defClass.Constructors?.Count ?? 0)
                .Select(i =>
                {
                    var ctor = defClass.Constructors[i];
                    var ctorName = $"{className.ToLowerInvariant().Replace(".", "_")}_ctor_{i}";
                    List<(string type, string name)> ctorParams = ctor.Params?
                        .Select(ctorParam => (ctorParam.Type.FullQualifiedName.GetCppType(), ctorParam.Name)).ToList();

                    if (ctorParams == null)
                        return $"{Config.Decl} void* {ctorName}()\n" +
                               "{\n" +
                               "    return nullptr;\n" +
                               "\n}";

                    return
                        $"{Config.Decl} void* {ctorName}({string.Join(", ",
                            ctorParams.Select(p => $"{p.type} {p.name.ToCamelCase()}"))})\n" +
                        "{\n" +
                        "    return nullptr;\n" +
                        "}\n";
                }));
        //empty ctor
        if (defClass.Constructors == null || defClass.Constructors.Count == 0)
        {
            ctors = $"{Config.Decl} void* {className.ToLowerInvariant().Replace(".", "_")}_ctor_0()\n" +
                    "{\n" +
                    "    return nullptr;\n" +
                    "}\n";
        }

        //methods
        var methods = string.Join("\n",
            Enumerable.Range(0, defClass.Methods?.Count ?? 0)
                .Select(i =>
                {
                    var method = defClass.Methods[i];
                    var methodName = $"{className.ToLowerInvariant().Replace(".", "_")}_{method.Name}";
                    List<(string type, string name)> methodTypeParams = method.TypeParameters?
                        .Select(typeParam => ("void*", $"typeParameter_{typeParam.Name.ToCamelCase()}")).ToList();

                    List<(string type, string name)> methodParams = method.Params?.Select(methodParam =>
                        (methodParam.Type.FullQualifiedName.GetCppType(), methodParam.Name)).ToList();

                    var returnType = method.ReturnType.FullQualifiedName.GetCppType();

                    List<(string type, string name)> args = new();
                    if (!method.IsStatic)
                    {
                        args.Add(("void*", "instance"));
                    }

                    if (methodTypeParams != null) args.AddRange(methodTypeParams);
                    if (methodParams != null) args.AddRange(methodParams);

                    return
                        $"{Config.Decl} {returnType} {methodName}({string.Join(", ", args.Select(p => $"{p.type} {p.name.ToCamelCase()}"))})\n" +
                        "{\n" +
                        $"    return {method.ReturnType.FullQualifiedName.GetDefaultReturnValue()};\n" +
                        "}\n";
                }));

        var template =
            $"""
#include "{className.ToLowerInvariant().Replace(".", "_")}.h"

/*
Constructors
*/
{ctors}
/*
Methods
*/
{methods}
""";

        var fileName = $"{className.ToLowerInvariant().Replace(".", "_")}";
        var filePath = Path.Combine(output.FullName,
            $"{fileName}.cpp");
        if (File.Exists(filePath))
        {
            var backupFile = new FileInfo(Path.Combine(output.FullName,
                $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}_backup.cpp"));
            File.Move(filePath, backupFile.FullName);
            Console.WriteLine($"Renamed file {filePath} to {backupFile.FullName}");
        }
        File.WriteAllText(filePath, template);
        Console.WriteLine($"Created file {filePath}");
    }
}