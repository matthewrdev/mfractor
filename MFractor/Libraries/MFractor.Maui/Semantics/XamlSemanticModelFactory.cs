using System;
using System.ComponentModel.Composition;
using MFractor.Maui.Symbols;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.XamlPlatforms.Maui;
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

        readonly Lazy<MauiXamlPlatform> mauiPlatform;
        public MauiXamlPlatform MauiPlatform => mauiPlatform.Value;

        [ImportingConstructor]
        public XamlSemanticModelFactory(Lazy<IMarkupExpressionEvaluater> expressionEvaluator,
                                        Lazy<IBindingContextResolver> bindingContextResolver,
                                        Lazy<IXamlSymbolResolver> symbolResolver,
                                        Lazy<MauiXamlPlatform> mauiPlatform)
        {
            this.expressionEvaluator = expressionEvaluator;
            this.bindingContextResolver = bindingContextResolver;
            this.symbolResolver = symbolResolver;
            this.mauiPlatform = mauiPlatform;
        }

        public IXamlSemanticModel Create(IParsedXamlDocument document, Project project)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var platform = MauiPlatform.Resolve(project, compilation, document.XamlSyntaxTree);
            if (platform == null)
            {
                return null;
            }

            return new XamlSemanticModel(document, project, compilation, platform, document.Namespaces, SymbolResolver, ExpressionEvaluator, BindingContextResolver);
        }

        public IXamlSemanticModel Create(IParsedXamlDocument document, Project project, Compilation compilation, IXamlNamespaceCollection namespaces)
        {
            var platform = MauiPlatform.Resolve(project, compilation, document.XamlSyntaxTree);
            if (platform == null)
            {
                return null;
            }

            return new XamlSemanticModel(document, project, compilation, platform, namespaces, SymbolResolver, ExpressionEvaluator, BindingContextResolver);
        }
    }
}
