using System;
using System.Collections.Generic;
using System.Threading;
using MFractor.Maui.Analysis;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Utilities.Visitors
{
    public class TypedValueConverterVisitor : SymbolVisitor
    {
        public CancellationToken Token = default(CancellationToken);

        public readonly List<ITypeSymbol> Matches = new List<ITypeSymbol>();

        public bool ContinueSearch
        {
            get;
            set;
        } = true;

        readonly ITypeSymbol inputType;
        readonly ITypeSymbol outputType;
        readonly IXamlPlatform platform;

        public TypedValueConverterVisitor(ITypeSymbol inputType,
                                          ITypeSymbol outputType,
                                          IXamlPlatform platform,
                                          CancellationToken token = default(CancellationToken))
        {
            this.outputType = outputType;
            this.platform = platform;
            this.inputType = inputType;
            Token = token;
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            Token.ThrowIfCancellationRequested();

            foreach (var m in symbol.Modules)
            {
                if (!ContinueSearch)
                {
                    break;
                }

                m.Accept(this);
            }
        }

        public override void VisitModule(IModuleSymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            Token.ThrowIfCancellationRequested();

            foreach (var type in symbol.GetMembers())
            {
                if (!ContinueSearch)
                {
                    break;
                }

                var namespaceType = type as INamespaceSymbol;
                var typeSymbol = type as ITypeSymbol;

                if (namespaceType != null)
                {
                    type.Accept(this);
                }
                else if (typeSymbol != null)
                {
                    if (IsMatch(typeSymbol))
                    {
                        this.Matches.Add(typeSymbol);
                    }
                }
            }
        }

        bool IsMatch(ITypeSymbol typeSymbol)
        {
            var namedType = typeSymbol as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(typeSymbol, platform.ValueConverter.MetaType))
            {
                return false;
            }

            ITypeSymbol expectedInput, expectedOutput, parameterType;

            if (!FormsSymbolHelper.ResolveValueConverterConstraints(namedType, out expectedInput, out expectedOutput, out parameterType))
            {
                return false;
            }

            return SymbolHelper.DerivesFrom(this.inputType, expectedInput)
                               && SymbolHelper.DerivesFrom(outputType, expectedOutput);
        }
    }
}

