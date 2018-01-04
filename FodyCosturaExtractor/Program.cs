using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;

namespace FodyCosturaExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication { Name = "Fody Costura Extractor" };
            app.HelpOption("-?|-h|-help|--help");

            app.OnExecute(() => {
                Console.WriteLine("A simple utility to extract assemblies embedded by Fody Costura");
                return 0;
            });

            app.Command("list", command =>
            {
                command.Description = "Extract all embedded assemblies";
                command.HelpOption("-?|-h|-help|--help");

                var assemblyArgument =
                    command.Argument("[assembly]", "A full path to assembly with embedded assemblies");

                command.OnExecute(() =>
                {
                    if (string.IsNullOrWhiteSpace(assemblyArgument.Value))
                    {
                        throw new ArgumentException("The [assembly] argument should not be empty..");
                    }
                    var asm = Assembly.LoadFile(assemblyArgument.Value);
                    var resourceNames = asm.GetManifestResourceNames();
                    if (resourceNames.Length == 0)
                    {
                        Console.WriteLine("Didn't find any Fody Costura embedded resources");
                    }
                    else
                    {
                        Console.WriteLine("Fody Costura embedded resource names:");
                        foreach (var name in resourceNames.Where(n => n.ToLowerInvariant().EndsWith(".zip")))
                        {
                            Console.WriteLine(name);
                        }
                    }
                    return 0;
                });

            });

            app.Command("extract-all", command =>
            {
                command.Description = "Extract all embedded assemblies";
                command.HelpOption("-?|-h|-help|--help");

                var assemblyArgument = command.Argument("[assembly]", "A full path to assembly with embedded assemblies");
                
                command.OnExecute(() =>
                {
                    if (string.IsNullOrWhiteSpace(assemblyArgument.Value))
                    {
                        throw new ArgumentException("The [assembly] argument should not be empty..");
                    }
                    var asm = Assembly.LoadFile(assemblyArgument.Value);
                    var resourceNames = asm.GetManifestResourceNames();
                    foreach (var name in resourceNames.Where(n => n.ToLowerInvariant().EndsWith(".zip")))
                    {
                        using (var resourceStream = asm.GetManifestResourceStream(name))
                        using (var deflateStream = new DeflateStream(resourceStream, CompressionMode.Decompress))
                        {
                            Directory.CreateDirectory("Extracted Assemblies");
                            var dllName = Path.Combine("Extracted Assemblies",name.Replace("costura.",string.Empty).Replace(".zip",string.Empty));
                            using (var fileStream = new FileStream(dllName,FileMode.Create))
                            {
                                deflateStream.CopyTo(fileStream);
                                fileStream.Flush(true);
                            }

                            Console.WriteLine("Extracted " + dllName);
                        }
                    }

                    return 0;
                });
            });

            app.Execute(args);
        }
    }
}
