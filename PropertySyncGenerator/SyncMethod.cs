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
    internal class SyncMethod
    {
        private const string MethodName = "Sync";
        private const string ReturnType = "void";

        private readonly ITypeSymbol _srcType;
        private readonly ITypeSymbol _targetType;
        private readonly ImmutableArray<Argument> _arguments;
        private readonly Lazy<ImmutableArray<IPropertySymbol>> _srcProperties;
        private readonly Lazy<ImmutableArray<IPropertySymbol>> _targetProperties;

        public SyncMethod(ITypeSymbol srcType, ITypeSymbol targetType)
        {
            _srcType = srcType;
            _targetType = targetType;
            _arguments = Arguments(srcType.ToString(), targetType.ToString()).ToImmutableArray();

            _srcProperties = new Lazy<ImmutableArray<IPropertySymbol>>(() => _srcType.GetAccessibleProperties().ToImmutableArray());
            _targetProperties = new Lazy<ImmutableArray<IPropertySymbol>>(() => _targetType.GetAccessibleProperties().ToImmutableArray());
        }

        private static Argument[] Arguments(string srcType, string targetType) => new Argument[]
        {
            new(srcType, "source"),
            new(targetType, "target"),
            new(CommonTypes.StringICollection, "ignores")
        };

        private void Body(BodyWriter writer)
        {
            foreach (IPropertySymbol item in _targetProperties.Value)
            {
                if (_srcProperties.Value.Any(x => x.Name == item.Name && SymbolEqualityComparer.Default.Equals(x.Type, item.Type)))
                {
                    string propName = _srcProperties.Value.First(x => x.Name == item.Name).Name;
                    var ifStmt = new If($"!{_arguments[2].Name}.Contains(\"{propName}\")",
                                        (ifWriter) => ifWriter.WriteAssignment($"{_arguments[1].Name}.{item.Name}", $"{ _arguments[0].Name }.{propName}"));
                    writer.WriteIf(new IfStatement(ifStmt));
                }
            }
        }

        public Method Build() => GetMethod(_arguments, Body);

        public static Method Stub() => GetMethod(Arguments("object", "object"), PropertySyncGenerator.EmptyWriter);

        private static Method GetMethod(IEnumerable<Argument> arguments, Action<BodyWriter> body) => new Method(Accessibility.Public, true, false, ReturnType, MethodName, arguments, body);
    }
}
