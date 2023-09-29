using System;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace NomLib
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand("MonNom Library Generator");

            var libJsonOption = new Option<FileInfo>(name: "--file",
                description: "Path to MonNom library definition json file",
                parseArgument: result =>
                {
                    if (result.Tokens.Count == 0)
                    {
                        result.ErrorMessage = "Missing file path";
                        return null;
                    }

                    string filePath = result.Tokens.Single().Value;
                    if (!File.Exists(filePath))
                    {
                        result.ErrorMessage = "File does not exist";
                        return null;
                    }

                    return new FileInfo(filePath);
                });

            var outputOption = new Option<DirectoryInfo>(name: "--output",
                description: $"Path to output directory",
                isDefault: true,
                parseArgument: result =>
                {
                    if (result.Tokens.Count == 0)
                    {
                        return new DirectoryInfo(Environment.CurrentDirectory);
                    }

                    string filePath = result.Tokens.Single().Value;
                    if (!Directory.Exists(filePath))
                    {
                        result.ErrorMessage = "Directory does not exist";
                        return null;
                    }

                    return new DirectoryInfo(filePath);
                });

            var archiveArgument = new Argument<bool>(name: "--archive",
                description: $"Pack library as archive to output directory");
            archiveArgument.SetDefaultValue(false);

            var generateCommand = new Command("generate", "Generate MonNom library");
            libJsonOption.IsRequired = true;
            generateCommand.AddOption(libJsonOption);
            generateCommand.AddOption(outputOption);
            generateCommand.SetHandler(
                (file, output) => { CodeGenerator.GenerateLibraryTemplate(File.ReadAllText(file!.FullName), output!); },
                libJsonOption, outputOption);
            generateCommand.AddAlias("export");

            rootCommand.AddCommand(generateCommand);
            
            var compileCommand = new Command("compile", "Compile MonNom library");
            libJsonOption.IsRequired = true;
            compileCommand.AddOption(libJsonOption);
            compileCommand.AddOption(outputOption);
            compileCommand.SetHandler(
                (file, output) => { Compiler.CompileLibrary(File.ReadAllText(file!.FullName), output!); },
                libJsonOption, outputOption);
            
            rootCommand.AddCommand(compileCommand);
            
            var packCommand = new Command("pack", "Pack MonNom library");
            libJsonOption.IsRequired = true;
            packCommand.AddOption(libJsonOption);
            packCommand.AddOption(outputOption);
            packCommand.AddArgument(archiveArgument);
            packCommand.SetHandler(
                (file, output, archive) => { Packer.PackLibrary(File.ReadAllText(file!.FullName), output!, archive); },
                libJsonOption, outputOption, archiveArgument);
            
            rootCommand.AddCommand(packCommand);

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}