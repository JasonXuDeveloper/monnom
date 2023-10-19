// Packer.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 Nom

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Nom;
using Nom.Bytecode;
using Nom.Project;
using NomLib.Definition;
using Type = NomLib.Definition.Type;

namespace NomLib;

public class Packer
{
    public static void PackLibrary(string json, DirectoryInfo outputPath, bool archive)
    {
        //deserialize json
        var libDef = JsonConvert.DeserializeObject<NomLibDef>(json);

        //get cpp files
        var input = new DirectoryInfo(Path.Combine(outputPath.FullName, libDef.LibraryName, "llvm"));
        if (!input.Exists)
        {
            Console.WriteLine($"Directory {input.FullName} does not exist, please generate first");
            return;
        }

        //create project
        NomProject project = new NomProject(libDef.LibraryName);
        project.Version = new Nom.Version(libDef.LibraryVersion);
        //assembly unit
        AssemblyUnit unit = new AssemblyUnit(project);

        //classes
        foreach (var defClass in libDef.Classes)
        {
            //check generate
            var className = defClass.FullQualifiedName;
            var filename = $"{className.ToLowerInvariant().Replace(".", "_")}.bc";
            var llvmFile = new FileInfo(Path.Combine(input.FullName, filename));
            if (!llvmFile.Exists && !defClass.IsInterface)
            {
                Console.WriteLine($"File {llvmFile.FullName} does not exist, please generate first");
                continue;
            }

            PackClass(libDef, defClass, unit);
        }

        foreach (var file in input.GetFiles("*.bc"))
        {
            unit.AddNativeFile(new LlvmFile(file));
        }

        outputPath = new DirectoryInfo(Path.Combine(outputPath.FullName, libDef.LibraryName, "Nom"));
        if (!outputPath.Exists)
        {
            outputPath.Create();
        }

        if (!archive)
        {
            unit.EmitToPath(outputPath);
        }
        else
        {
            unit.EmitArchive(new FileInfo(outputPath.FullName + "/" + libDef.LibraryName + ".mnar"));
        }

        Console.WriteLine("Done!");
    }

    private static IConstantRef<TypeListConstant> GetTypeList(IEnumerable<IConstantRef<ITypeConstant>> typeArgs,
        BytecodeUnit bytecodeUnit)
    {
        return bytecodeUnit.GetTypeListConstant(typeArgs);
    }

    private static IConstantRef<TypeParametersConstant> GetTypeParametersConstant(IEnumerable<TypeParameter> parameters,
        BytecodeUnit bytecodeUnit)
        => bytecodeUnit.GetTypeParametersConstant(parameters
            .Select(parameter => GetTypeArgument(parameter.Type, bytecodeUnit))
            .Select(upperBound => new TypeParameterEntry(bytecodeUnit.GetBottomTypeConstant(), upperBound)));

    private static IConstantRef<ITypeConstant> GetTypeArgument(Type typeArgument, BytecodeUnit bytecodeUnit)
    {
        switch (typeArgument.Kind)
        {
            case "Var":
                IConstantRef<TypeVariableConstant> typeVariableConstant =
                    bytecodeUnit.GetTypeVariableConstant(typeArgument.Index);
                return typeVariableConstant;
            case "Top":
                return BytecodeUnit.GetEmptyTypeConstant();
            case "Bottom":
                return bytecodeUnit.GetBottomTypeConstant();
            case "Dynamic":
                return bytecodeUnit.GetDynamicTypeConstant();
            case "Class":
                var c = bytecodeUnit.GetClassConstant(typeArgument.FullQualifiedName, typeArgument.LibraryName);
                var typeArgs = typeArgument.Arguments?.Select(arg => GetTypeArgument(arg, bytecodeUnit));
                var typeList = GetTypeList(typeArgs, bytecodeUnit);
                return bytecodeUnit.GetNamedTypeConstant(typeArgument.ToString(), c, typeList);
            case "Interface":
                var i = bytecodeUnit.GetInterfaceConstant(typeArgument.FullQualifiedName, typeArgument.LibraryName);
                var typeArgs2 = typeArgument.Arguments?.Select(arg => GetTypeArgument(arg, bytecodeUnit));
                var typeList2 = GetTypeList(typeArgs2, bytecodeUnit);
                return bytecodeUnit.GetNamedTypeConstant(typeArgument.ToString(), i, typeList2);
            case "Maybe":
                return bytecodeUnit.GetMaybeTypeConstant(GetTypeArgument(typeArgument.Arguments.First(), bytecodeUnit));
            default:
                throw new InvalidDataException($"Invalid Kind {typeArgument.Kind}");
        }
    }

    private static void PackClass(NomLibDef def, Class nomClass, AssemblyUnit unit)
    {
        BytecodeUnit bytecodeUnit = new BytecodeUnit(nomClass.FullQualifiedName, unit);

        //create corresponded constants
        Super superClass = nomClass.FullQualifiedSuperClass;

        //super class
        var superClassClassConstant = string.IsNullOrEmpty(superClass.FullQualifiedName)
            ? bytecodeUnit.GetEmptyClassConstant()
            : bytecodeUnit.GetClassConstant(superClass.FullQualifiedName, superClass.LibraryName);
        var superClassTypeListConstant =
            GetTypeList(superClass.TypeArguments?.Select(arg => GetTypeArgument(arg, bytecodeUnit)), bytecodeUnit);
        var superClassConstant = bytecodeUnit.GetSuperClassConstant(
            !string.IsNullOrEmpty(superClass.FullQualifiedName) ? superClass.ToString() : "default",
            superClassClassConstant,
            superClassTypeListConstant);

        //super interfaces
        List<(IConstantRef<IInterfaceConstant> iCon, IConstantRef<TypeListConstant> tCon)> superInterfaces = new();
        foreach (var superInterface in nomClass.FullQualifiedSuperInterfaces)
        {
            var superInterfaceClassConstant =
                bytecodeUnit.GetInterfaceConstant(superInterface.FullQualifiedName, superInterface.LibraryName);
            var superInterfaceTypeListConstant =
                GetTypeList(superInterface.TypeArguments.Select(arg => GetTypeArgument(arg, bytecodeUnit)),
                    bytecodeUnit);
            superInterfaces.Add((superInterfaceClassConstant, superInterfaceTypeListConstant));
        }

        var superInterfacesConstant = bytecodeUnit.GetSuperInterfacesConstant(superInterfaces);

        //typeparameters
        var typeParameters = GetTypeParametersConstant(nomClass.TypeParameters, bytecodeUnit);

        ClassRep classRep = new ClassRep(bytecodeUnit.GetStringConstant(nomClass.FullQualifiedName),
            typeParameters,
            superClassConstant,
            superInterfacesConstant,
            true, false, nomClass.IsShape, Enum.Parse<Visibility>(nomClass.Visibility), bytecodeUnit.AssemblyUnit);

        foreach (var field in nomClass.Fields)
        {
            FieldRep fieldRep = new FieldRep(classRep, bytecodeUnit.GetStringConstant(field.Name),
                GetTypeArgument(field.Type, bytecodeUnit),
                field.IsReadOnly, field.IsVolatile,
                Enum.Parse<Visibility>(field.Visibility));
            classRep.AddField(fieldRep);
        }

        foreach (var constructor in nomClass.Constructors)
        {
            var ctorName =
                $"{nomClass.FullQualifiedName.ToLowerInvariant().Replace(".", "_")}_ctor_{nomClass.Constructors.IndexOf(constructor)}";
            CppStaticMethodDefRep cppStaticMethodDefRep = new CppStaticMethodDefRep(
                bytecodeUnit.GetStringConstant(ctorName),
                bytecodeUnit.GetStringConstant(ctorName),
                GetTypeArgument(new Type()
                {
                    Kind = "Class",
                    FullQualifiedName = nomClass.FullQualifiedName,
                    LibraryName = def.LibraryName
                }, bytecodeUnit),
                typeParameters,
                GetTypeList(constructor.Params.Select(param => GetTypeArgument(param.Type, bytecodeUnit)),
                    bytecodeUnit),
                Visibility.Public);
            classRep.AddStaticMethod(cppStaticMethodDefRep);
        }

        foreach (var method in nomClass.Methods)
        {
            if (method.IsStatic)
            {
                var cppMethodName = $"{nomClass.FullQualifiedName.ToLowerInvariant().Replace(".", "_")}_{method.Name}";
                CppStaticMethodDefRep cppStaticMethodDefRep = new CppStaticMethodDefRep(
                    bytecodeUnit.GetStringConstant(method.Name),
                    bytecodeUnit.GetStringConstant(cppMethodName),
                    GetTypeArgument(method.ReturnType, bytecodeUnit),
                    GetTypeParametersConstant(method.TypeParameters, bytecodeUnit),
                    GetTypeList(method.Params.Select(param => GetTypeArgument(param.Type, bytecodeUnit)),
                        bytecodeUnit),
                    Enum.Parse<Visibility>(method.Visibility));
                classRep.AddStaticMethod(cppStaticMethodDefRep);
            }
            else
            {
                var cppMethodName = $"{nomClass.FullQualifiedName.ToLowerInvariant().Replace(".", "_")}_{method.Name}";
                CppMethodDeclRep cppMethodDeclRep = new CppMethodDeclRep(bytecodeUnit.GetStringConstant(method.Name),
                    bytecodeUnit.GetStringConstant(cppMethodName),
                    GetTypeParametersConstant(method.TypeParameters, bytecodeUnit),
                    GetTypeArgument(method.ReturnType, bytecodeUnit),
                    GetTypeList(method.Params.Select(param => GetTypeArgument(param.Type, bytecodeUnit)),
                        bytecodeUnit),
                    Enum.Parse<Visibility>(method.Visibility),
                    method.IsFinal);
                classRep.AddMethodDef(cppMethodDeclRep);
            }
        }

        bytecodeUnit.AddClass(classRep);
        unit.AddUnit(bytecodeUnit);
    }
}