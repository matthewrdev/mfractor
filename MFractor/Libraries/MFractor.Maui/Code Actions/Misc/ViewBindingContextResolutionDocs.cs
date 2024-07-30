using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Misc
{

    class ViewBindingContextResolutionDocs : XamlCodeAction
    {
        [ImportingConstructor]
        public ViewBindingContextResolutionDocs(Lazy<IBindingContextResolver> bindingContextResolver,
                                                Lazy<IUrlLauncher> urlLauncher)
        {
            this.bindingContextResolver = bindingContextResolver;
            this.urlLauncher = urlLauncher;
        }

        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        protected IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        public override CodeActionCategory Category => CodeActionCategory.Misc;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_actions.xaml.view_binding_context_resolution_docs";

        public override string Name => string.Empty;

        public override string Documentation => string.Empty;

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression == null)
            {
                return false;
            }

            var binding = expression as BindingExpression;
            if (binding == null)
            {
                return false;
            }

            var bindingContext = BindingContextResolver.ResolveBindingContext(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, binding, binding.ParentAttribute.Parent);

            return bindingContext == null;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Learn how MFractor detects your ViewModels and binding contexts.").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            UrlLauncher.OpenUrl("https://docs.mfractor.com/xamarin-forms/binding-context-resolution/overview/");

            return null;
        }
    }
}
