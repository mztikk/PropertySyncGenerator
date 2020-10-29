using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PropertySyncGenerator.Extensions;
using Sharpie;
using Sharpie.Writer;

namespace PropertySyncGenerator
{
    [Generator]
    public class PropertySyncGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Class c = new Class("PropertySync")
                .SetNamespace(context.Compilation.AssemblyName)
                .SetStatic(true)
                .WithAccessibility(Accessibility.Public);

            //Debugger.Launch();

            foreach (INamedTypeSymbol t1 in GetAllPublicTypesWithProperties(context.Compilation))
            {
                IEnumerable<IPropertySymbol> t1members = t1.GetAccessibleProperties();

                var dictionaryTargetArguments = new List<Argument> {
                    new Argument(t1.ToString(), "source"),
                    new Argument("System.Collections.Generic.Dictionary<string, string>", "target")
                };

                Action<BodyWriter> dictionaryTargetMethodBodyWriter = (bodyWriter) =>
                {
                    foreach (IPropertySymbol prop in t1members)
                    {
                        bodyWriter.WriteLine($"if ({dictionaryTargetArguments[1].Name}.ContainsKey(\"{prop.Name}\"))");
                        bodyWriter.WriteLine("{");
                        bodyWriter.IndentationLevel++;
                        bodyWriter.Write($"{dictionaryTargetArguments[1].Name}[\"{prop.Name}\"] = ");
                        bodyWriter.Write($"{dictionaryTargetArguments[0].Name}.{prop.Name}");
                        if (prop.Type.Name != "String")
                        {
                            bodyWriter.Write(".ToString()");
                        }
                        bodyWriter.WriteLine(";");
                        bodyWriter.IndentationLevel--;
                        bodyWriter.WriteLine("}");
                    }
                };

                var dictionaryTargetMethod = new Method(Accessibility.Public, true, false, "void", "Sync", dictionaryTargetArguments, dictionaryTargetMethodBodyWriter);

                if (t1members.Any(x => x.Type.HasStringParse()))
                {
                    var dictionarySourceArguments = new List<Argument> { new Argument("System.Collections.Generic.Dictionary<string, string>", "target"), new Argument(t1.ToString(), "source") };

                    Action<BodyWriter> dictionarySourceMethodBodyWriter = (bodyWriter) =>
                    {
                        bodyWriter.WriteLine($"foreach (System.Collections.Generic.KeyValuePair<string, string> item in {dictionarySourceArguments[0].Name})");
                        bodyWriter.WriteLine("{");
                        bodyWriter.IndentationLevel++;

                        bodyWriter.WriteLine($"switch (item.Key)");
                        bodyWriter.WriteLine("{");
                        bodyWriter.IndentationLevel++;

                        foreach (IPropertySymbol prop in t1members.Where(prop => prop.Type.HasStringParse() || prop.Type.Name == "String"))
                        {
                            bodyWriter.WriteLine($"case \"{prop.Name}\":");
                            bodyWriter.IndentationLevel++;

                            bodyWriter.Write($"{dictionarySourceArguments[1].Name}.{prop.Name} = ");
                            string fullTypeName = prop.Type.ToString().TrimEnd('?');
                            switch (prop.Type.Name)
                            {
                                case "String":
                                    bodyWriter.Write("item.Value");
                                    break;
                                default:
                                    bodyWriter.Write($"{fullTypeName}.Parse(item.Value)");
                                    break;
                            }
                            bodyWriter.WriteLine(";");

                            bodyWriter.WriteLine("break;");
                            bodyWriter.IndentationLevel--;
                        }

                        bodyWriter.IndentationLevel--;
                        bodyWriter.WriteLine("}");

                        bodyWriter.IndentationLevel--;
                        bodyWriter.WriteLine("}");
                    };

                    var dictionarySourceMethod = new Method(Accessibility.Public, true, false, "void", "Sync", dictionarySourceArguments, dictionarySourceMethodBodyWriter);

                    c.WithMethod(dictionarySourceMethod);
                }

                c.WithMethod(dictionaryTargetMethod);

                foreach (INamedTypeSymbol t2 in GetAllPublicTypesWithProperties(context.Compilation).Where(x => !SymbolEqualityComparer.Default.Equals(t1, x) && t1.HasMatchingProperties(x)))
                {
                    IEnumerable<IPropertySymbol> t2members = t2.GetAccessibleProperties();

                    var arguments = new List<Argument> { new Argument(t1.ToString(), "source"), new Argument(t2.ToString(), "target") };

                    Action<BodyWriter> bodyWriter = (bodyWriter) =>
                    {
                        foreach (IPropertySymbol item in t2members)
                        {
                            if (t1members.Any(x => x.Name == item.Name && SymbolEqualityComparer.Default.Equals(x.Type, item.Type)))
                            {
                                bodyWriter.WriteLine($"{arguments[1].Name}.{item.Name} = {arguments[0].Name}.{t1members.First(x => x.Name == item.Name).Name};");
                            }
                        }
                    };

                    var m = new Method(Accessibility.Public, true, false, "void", "Sync", arguments, bodyWriter);
                    c.WithMethod(m);
                }
            }

            string str = ClassWriter.WriteAsync(c).GetAwaiter().GetResult();
            //File.WriteAllText("test.cs", str);

            context.AddSource("PropertySync", SourceText.From(str, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private IEnumerable<INamedTypeSymbol> GetAllPublicTypesWithProperties(Compilation compilation) => GetAllTypesWithProperties(compilation).Where(x => x.DeclaredAccessibility == Accessibility.Public && x.TypeParameters.Length == 0);
        private IEnumerable<INamedTypeSymbol> GetAllTypesWithProperties(Compilation compilation) => GetAllTypes(compilation).Where(x => !x.IsStatic && x.GetAccessibleProperties().Any());

        private IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
        {
            foreach (INamedTypeSymbol symbol in GetAllPublicTypes(compilation.Assembly.GlobalNamespace))
            {
                yield return symbol;
            }

            foreach (MetadataReference item in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(item) is IAssemblySymbol assemblySymbol)
                {
                    foreach (INamedTypeSymbol symbol in GetAllPublicTypes(assemblySymbol.GlobalNamespace))
                    {
                        yield return symbol;
                    }
                }
            }
        }

        private IEnumerable<INamedTypeSymbol> GetAllPublicTypes(params INamespaceOrTypeSymbol[] symbols)
        {
            var stack = new Stack<INamespaceOrTypeSymbol>(symbols);

            while (stack.Count > 0)
            {
                INamespaceOrTypeSymbol item = stack.Pop();

                if (item is INamedTypeSymbol type && type.DeclaredAccessibility == Accessibility.Public)
                {
                    yield return type;
                }

                foreach (ISymbol member in item.GetMembers())
                {
                    if (member is INamespaceOrTypeSymbol child && child.DeclaredAccessibility == Accessibility.Public && (member is not INamedTypeSymbol typeSymbol || typeSymbol.TypeParameters.Length == 0))
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        private IEnumerable<INamedTypeSymbol> GetAllTypes(params INamespaceOrTypeSymbol[] symbols)
        {
            var stack = new Stack<INamespaceOrTypeSymbol>(symbols);

            while (stack.Count > 0)
            {
                INamespaceOrTypeSymbol item = stack.Pop();

                if (item is INamedTypeSymbol type)
                {
                    yield return type;
                }

                foreach (ISymbol member in item.GetMembers())
                {
                    if (member is INamespaceOrTypeSymbol child)
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }
}
