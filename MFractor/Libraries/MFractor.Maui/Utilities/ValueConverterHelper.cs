using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities.Visitors;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Utilities
{
    public static class ValueConverterHelper
    {
        public static List<ITypeSymbol> ResolveTypedValueConvertersInCompilation(Compilation compilation,
                                                                                 IXamlPlatform platform,
                                                                                 ITypeSymbol inputType,
                                                                                 ITypeSymbol outputType)
        {
            var vistor = new TypedValueConverterVisitor(inputType, outputType, platform);

            vistor.VisitModule(compilation.SourceModule);

            foreach (var r in compilation.References)
            {
                var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol;
                if (assemblySymbol != null)
                {
                    foreach (var m in assemblySymbol.Modules)
                    {
                        vistor.VisitModule(m);
                    }
                }
            }

            return vistor.Matches;
        }

        public static ITypeSymbol ResolveValueConverterInputType(IXamlFeatureContext context, ConverterExpression expression, IMarkupExpressionEvaluater expressionEvaluator)
        {
            var result = expressionEvaluator.Evaluate(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, expression.AssignmentValue);

            var defaultMetaType = context.Compilation.GetSpecialType(SpecialType.System_Object);
            if (result == null
                || result.Symbol as INamedTypeSymbol == null)
            {
                return defaultMetaType;
            }

            var converter = result.Symbol as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(converter, context.Platform.ValueConverter.MetaType))
            {
                return defaultMetaType;
            }

            if (!FormsSymbolHelper.ResolveValueConverterConstraints(converter, out var inputType, out _, out _))
            {
                return defaultMetaType;
            }

            return inputType;
        }
    }
}
