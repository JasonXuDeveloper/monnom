// Packer.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 Nom

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Nom;
using Nom.Bytecode;
using Nom.Language;
using Nom.Project;
using Nom.TypeChecker;
using Nom.TypeChecker.StdLib;
using NomLib.Definition;
using Object = Nom.TypeChecker.StdLib.Object;

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
            if (!llvmFile.Exists)
            {
                Console.WriteLine($"File {llvmFile.FullName} does not exist, please generate first");
                continue;
            }

            PackClass(defClass, unit);
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

    private static void PackClass(Class nomClass, AssemblyUnit unit)
    {
        BytecodeUnit bytecodeUnit = new BytecodeUnit(nomClass.FullQualifiedName, unit);
        List<TDTypeArgDecl> typeArgs = new List<TDTypeArgDecl>();
        for (int i = 0; i < nomClass.TypeParameters.Count; i++)
        {
            typeArgs.Add(new TDTypeArgDecl(nomClass.TypeParameters[i].Name, i));
        }

        ClassRep classRep = new ClassRep(bytecodeUnit.GetStringConstant(nomClass.FullQualifiedName),
            bytecodeUnit.GetTypeParametersConstant(new TypeParametersSpec(typeArgs)),
            bytecodeUnit.GetSuperClassConstant(cls.SuperClass.Elem), //TODO how to get this constant?
            bytecodeUnit.GetSuperInterfacesConstant(cls.GetParamRef<Language.IClassSpec, Language.IType>()
                .AllImplementedInterfaces()
                .Distinct(Language.ParamRefEqualityComparer<Language.IInterfaceSpec, Language.IType>
                    .Instance)), //TODO how to get this constant?
            true, false, nomClass.IsShape, Enum.Parse<Visibility>(nomClass.Visibility), bytecodeUnit.AssemblyUnit);
        
        foreach (var field in nomClass.Fields)
        {
            FieldRep fieldRep = new FieldRep(classRep, bytecodeUnit.GetStringConstant(field.Name),
                bytecodeUnit.GetTypeConstant(field.Type), //TODO how to get this constant?
                field.IsReadOnly, field.IsVolatile,
                Enum.Parse<Visibility>(field.Visibility));
            classRep.AddField(fieldRep);
        }

        foreach (var constructor in nomClass.Constructors)
        {
            var ctorName =
                $"{nomClass.FullQualifiedName.ToLowerInvariant().Replace(".", "_")}_ctor_{nomClass.Constructors.IndexOf(constructor)}";

            CppConstructorDefRep constructorRep = new CppConstructorDefRep(
                bytecodeUnit.GetStringConstant(ctorName),
                bytecodeUnit.GetTypeListConstant(
                    cd.Parameters.Entries.Select(ps => ps.Type)), //TODO how to get this constant?
                Visibility.Public,
                cd.SuperConstructorArgs.Select(sca => sca.Index), //TODO how to get this?
                cd.RegisterCount //TODO how to get this?
            );
            classRep.AddConstructor(constructorRep);
        }

        foreach (var method in nomClass.Methods)
        {
            if (method.IsStatic)
            {
                //         List<IInstruction> instructions = new List<IInstruction>();
                //         foreach (TypeChecker.IInstruction instruction in smd.Instructions)
                //         {
                //             instructions.Add(instruction.Visit(InstructionConverter.Instance, bcu));
                //         }
                //         //TODO: fix type variable and parameter lists
                //         StaticMethodDefRep smdr = new StaticMethodDefRep(bcu.GetStringConstant(smd.Name), bcu.GetTypeConstant(smd.ReturnType), bcu.GetTypeParametersConstant(smd.TypeParameters), bcu.GetTypeListConstant(smd.Parameters.Entries.Select(ps => ps.Type)), smd.Visibility, instructions, smd.RegisterCount);
                //         cr.AddStaticMethod(smdr);
            }
            else
            {
                var cppMethodName = $"{nomClass.FullQualifiedName.ToLowerInvariant().Replace(".", "_")}_{method.Name}";
                CppMethodDeclRep cppMethodDeclRep = new CppMethodDeclRep(bytecodeUnit.GetStringConstant(method.Name),
                    bytecodeUnit.GetStringConstant(cppMethodName),
                    bytecodeUnit.GetTypeParametersConstant(method.TypeParameters), //TODO how to get this constant?
                    bytecodeUnit.GetTypeConstant(method.ReturnType), //TODO how to get this constant?
                    bytecodeUnit.GetTypeListConstant(
                        method.Parameters.Entries.Select(ps => ps.Type)), //TODO how to get this constant?
                    Enum.Parse<Visibility>(method.Visibility),
                    method.IsFinal);
                classRep.AddMethodDef(cppMethodDeclRep);
            }
        }
        
        //TODO somehow attach the llvm bitcode of this class

        bytecodeUnit.AddClass(classRep);
        unit.AddUnit(bytecodeUnit);
    }
}