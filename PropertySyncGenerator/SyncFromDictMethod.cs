using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using PropertySyncGenerator.Extensions;
using Sharpie;
using Sharpie.Writer;

namespace PropertySyncGenerator
{
    internal class SyncFromDictMethod
    {
        private const string MethodName = "Sync";
        private const string ReturnType = "void";

        private readonly ITypeSymbol _type;
        private readonly ImmutableArray<Argument> _arguments;
        private readonly Lazy<ImmutableArray<IPropertySymbol>> _properties;

        public SyncFromDictMethod(ITypeSymbol type)
        {
            _type = type;
            _arguments = Arguments(type.ToString()).ToImmutableArray();

            _properties = new Lazy<ImmutableArray<IPropertySymbol>>(() => _type.GetAccessibleProperties().ToImmutableArray());
        }

        private static Argument[] Arguments(string type) => new Argument[]
            {
                new (CommonTypes.StringDict, "source"),
                new (type, "target")
            };

        public void Body(BodyWriter writer) => writer.WriteForEachLoop(new ForEachLoop(
                        "System.Collections.Generic.KeyValuePair<string, string> item",
                        _arguments[0].Name,
                        (forEachLoopWriter) =>
                        {
                            var caseStatements = new List<CaseStatement>();

                            foreach (IPropertySymbol prop in _properties.Value.Where(prop => prop.Type.HasStringParse() || prop.Type.Name == "String"))
                            {
                                var caseStmt = new CaseStatement(
                                    $"\"{prop.Name}\"",
                                    (caseWriter) =>
                                    {
                                        caseWriter.Write($"{_arguments[1].Name}.{prop.Name} = ");
                                        string fullTypeName = prop.Type.ToString().TrimEnd('?');
                                        switch (prop.Type.Name)
                                        {
                                            case "String":
                                                caseWriter.Write("item.Value");
                                                break;
                                            default:
                                                caseWriter.Write($"{fullTypeName}.Parse(item.Value)");
                                                break;
                                        }
                                        caseWriter.WriteLine(";");
                                        caseWriter.WriteBreak();
                                    });

                                caseStatements.Add(caseStmt);
                            }

                            forEachLoopWriter.WriteSwitchCaseStatement(new SwitchCaseStatement("item.Key", caseStatements));
                        }));

        public Method Build() => GetMethod(_arguments, Body);

        public static Method Stub() => GetMethod(Arguments("object"), PropertySyncGenerator.EmptyWriter);

        private static Method GetMethod(IEnumerable<Argument> arguments, Action<BodyWriter> body) => new Method(Accessibility.Public, true, false, ReturnType, MethodName, arguments, body);
    }
}
