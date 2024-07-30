using System;
using System.ComponentModel.Composition;
using MFractor.Maui.Symbols;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Semantics
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlSemanticModelFactory))]
    class XamlSemanticModelFactory : IXamlSemanticModelFactory
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluator;
        IMarkupExpressionEvaluater ExpressionEvaluator => expressionEvaluator.Value;

        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        readonly Lazy<IXamlSymbolResolver> symbolResolver;
        IXamlSymbolResolver SymbolResolver => symbolResolver.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        public XamlSemanticModelFactory(Lazy<IMarkupExpressionEvaluater> expressionEvaluator,
                                        Lazy<IBindingContextResolver> bindingContextResolver,
                                        Lazy<IXamlSymbolResolver> symbolResolver,
                                        Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.expressionEvaluator = expressionEvaluator;
            this.bindingContextResolver = bindingContextResolver;
            this.symbolResolver = symbolResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        public IXamlSemanticModel Create(IParsedXamlDocument document, Project project)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var platform = XamlPlatforms.ResolvePlatform(project, compilation, document.XamlSyntaxTree);

            return new XamlSemanticModel(document, project, compilation, platform, document.Namespaces, SymbolResolver, ExpressionEvaluator, BindingContextResolver);
        }

        public IXamlSemanticModel Create(IParsedXamlDocument document, Project project, Compilation compilation, IXamlNamespaceCollection namespaces)
        {
            var platform = XamlPlatforms.ResolvePlatform(project, compilation, document.XamlSyntaxTree);

            return new XamlSemanticModel(document, project, compilation, platform, namespaces, SymbolResolver, ExpressionEvaluator, BindingContextResolver);
        }
    }
}