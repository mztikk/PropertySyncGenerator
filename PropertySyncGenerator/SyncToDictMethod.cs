using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using PropertySyncGenerator.Extensions;
using Sharpie;
using Sharpie.Writer;

namespace PropertySyncGenerator
{
    internal class SyncToDictMethod
    {
        private const string MethodName = "Sync";
        private const string ReturnType = "void";

        private readonly ITypeSymbol _type;
        private readonly ImmutableArray<Parameter> _arguments;
        private readonly Lazy<ImmutableArray<IPropertySymbol>> _properties;

        public SyncToDictMethod(ITypeSymbol type)
        {
            _type = type;
            _arguments = Arguments(type.ToString()).ToImmutableArray();

            _properties = new Lazy<ImmutableArray<IPropertySymbol>>(() => _type.GetAccessibleProperties().ToImmutableArray());
        }

        private static Parameter[] Arguments(string type) => new Parameter[]
            {
                new (type, "source"),
                new (CommonTypes.StringDict, "target"),
                new ("bool", "force", "false")
            };

        private void Body(BodyWriter writer)
        {
            foreach (IPropertySymbol? prop in _properties.Value)
            {
                var ifStmt = new If($"{_arguments[1].Name}.ContainsKey(\"{prop.Name}\") || {_arguments[2].Name}",
                                    (ifbody) =>
                                    {
                                        string value = $"{_arguments[0].Name}.{prop.Name}";
                                        if (prop.Type.Name != "String")
                                        {
                                            value += ".ToString()";
                                        }
                                        ifbody.WriteAssignment($"{_arguments[1].Name}[\"{prop.Name}\"]", value);
                                    });

                writer.WriteIf(new IfStatement(ifStmt));
            }
        }

        public Method Build() => GetMethod(_arguments, Body);

        public static Method Stub() => GetMethod(Arguments("object"), PropertySyncGenerator.EmptyWriter);

        private static Method GetMethod(IEnumerable<Parameter> arguments, Action<BodyWriter> body) => new Method(Accessibility.Public, true, false, ReturnType, MethodName, arguments, body);
    }
}
