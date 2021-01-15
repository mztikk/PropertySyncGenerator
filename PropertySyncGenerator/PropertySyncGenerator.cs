using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Sharpie;
using Sharpie.Writer;

namespace PropertySyncGenerator
{
    [Generator]
    public class PropertySyncGenerator : ISourceGenerator
    {
        public static readonly Action<BodyWriter> EmptyWriter = (writer) => { };

        public void Execute(GeneratorExecutionContext context)
        {
            string ns = context.Compilation.AssemblyName ?? context.Compilation.ToString();
            string className = $"PropertySync";
            string fullName = $"{ns}.{className}";

            Class stubClass = new Class(className)
                .SetStatic(true)
                .SetNamespace(ns)
                .WithAccessibility(Accessibility.Internal)
                .WithMethod(SyncMethod.Stub())
                .WithMethod(SyncMethodAll.Stub())
                .WithMethod(SyncToDictMethod.Stub())
                .WithMethod(SyncFromDictMethod.Stub());

            Compilation compilation = GetStubCompilation(context, stubClass);
            INamedTypeSymbol? stubClassType = compilation.GetTypeByMetadataName(fullName);

            IEnumerable<(ITypeSymbol, ITypeSymbol)> calls = GetStubCalls(compilation, stubClassType);

            Class generatedClass = new Class(className)
                .SetStatic(true)
                .SetNamespace(ns)
                .WithAccessibility(Accessibility.Internal);

            var generatedToTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var generatedFromTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var generatedSyncSrcTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var generatedSyncTargetTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            if (calls.Any())
            {
                foreach ((ITypeSymbol t1, ITypeSymbol t2) in calls)
                {
                    if (t1 is null || t2 is null)
                    {
                        continue;
                    }

                    if (t1.ToString() == CommonTypes.StringDict)
                    {
                        if (generatedFromTypes.Contains(t2))
                        {
                            continue;
                        }

                        generatedClass = generatedClass.WithMethod(new SyncFromDictMethod(t2).Build());
                        generatedFromTypes.Add(t2);
                    }
                    else if (t2.ToString() == CommonTypes.StringDict)
                    {
                        if (generatedToTypes.Contains(t1))
                        {
                            continue;
                        }

                        generatedClass = generatedClass.WithMethod(new SyncToDictMethod(t1).Build());
                        generatedToTypes.Add(t1);
                    }
                    else
                    {
                        if (generatedSyncSrcTypes.Contains(t1) && generatedSyncTargetTypes.Contains(t2))
                        {
                            continue;
                        }

                        generatedClass = generatedClass.WithMethod(new SyncMethod(t1, t2).Build()).WithMethod(new SyncMethodAll(t1, t2).Build());
                        generatedSyncSrcTypes.Add(t1);
                        generatedSyncTargetTypes.Add(t2);
                    }
                }

                string str = ClassWriter.Write(generatedClass);

                context.AddSource(className, SourceText.From(str, Encoding.UTF8));
            }
            else
            {
                context.AddSource(stubClass.ClassName, SourceText.From(ClassWriter.Write(stubClass), Encoding.UTF8));
            }
        }

        private static Compilation GetStubCompilation(GeneratorExecutionContext context, Class stubClass)
        {
            Compilation compilation = context.Compilation;

            var options = (compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions;

            return compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(ClassWriter.Write(stubClass), Encoding.UTF8), options));
        }

        private static IEnumerable<(ITypeSymbol, ITypeSymbol)> GetStubCalls(Compilation compilation, INamedTypeSymbol? stubClassType)
        {
            if (stubClassType is null)
            {
                yield break;
            }

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);
                foreach (InvocationExpressionSyntax invocation in tree.GetRoot().DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
                {
                    if (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol symbol && symbol.ContainingType is { })
                    {
                        if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, stubClassType))
                        {
                            SeparatedSyntaxList<ArgumentSyntax> args = invocation.ArgumentList.Arguments;
                            if (args.Count < 2)
                            {
                                continue;
                            }

                            ExpressionSyntax firstArgument = args[0].Expression;
                            ExpressionSyntax secondArgument = args[1].Expression;
                            ITypeSymbol? firstArgumentType = semanticModel.GetTypeInfo(firstArgument).Type;
                            ITypeSymbol? secondArgumentType = semanticModel.GetTypeInfo(secondArgument).Type;
                            if (firstArgumentType is null || secondArgumentType is null)
                            {
                                continue;
                            }
                            yield return (firstArgumentType, secondArgumentType);
                        }
                    }
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
