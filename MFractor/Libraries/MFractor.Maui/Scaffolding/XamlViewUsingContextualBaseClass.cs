using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.CSharp;
using MFractor.CSharp.CodeGeneration;
using MFractor.Code;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Code.Scaffolding;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Scaffolding
{
    class XamlViewUsingContextualBaseClass : Scaffolder
    {
        readonly Lazy<IContextualBaseClassResolver> contextualBaseClassResolver;
        public IContextualBaseClassResolver ContextualBaseClassResolver => contextualBaseClassResolver.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.Code.Scaffolding.xaml.contextual_xaml_control";

        public override string Name => "Contextual XAML Control Scaffolder";

        public override string Documentation => "Creates a new XAML control inheriting from the most used view base class type in the current folder path.";

        public override string Criteria => "Activates when the file extension is '.xaml'.";

        [Import]
        public IXamlViewWithCodeBehindGenerator XamlViewWithCodeBehindGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [ImportingConstructor]
        public XamlViewUsingContextualBaseClass(Lazy<IContextualBaseClassResolver> contextualBaseClassResolver, Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.contextualBaseClassResolver = contextualBaseClassResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        public override bool IsAvailable(IScaffoldingContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            if (context.Project == null)
            {
                return false;
            }

            var platform = XamlPlatforms.ResolvePlatform(context.Project);
            var baseType = ContextualBaseClassResolver.GetSuggestedBaseClass(context.Project, input.VirtualFolderPath, (type) =>
            {
                return IsVisualElement(type, platform);
            });

            return baseType != null;
        }

        bool IsVisualElement(INamedTypeSymbol namedType, IXamlPlatform platform)
        {
            return SymbolHelper.DerivesFrom(namedType, platform.VisualElement.MetaType);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            var platform = XamlPlatforms.ResolvePlatform(context.Project);
            var baseType = ContextualBaseClassResolver.GetSuggestedBaseClass(context.Project, input.VirtualFolderPath, (type) =>
            {
                return IsVisualElement(type, platform);
            });

            var className = CSharpNameHelper.ConvertToValidCSharpName(input.NameNoExtension);

            return CreateSuggestion("Create a new XAML view named " + className + " that derives from " + baseType.ToString(), int.MaxValue - 30).AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state, IScaffoldingSuggestion suggestion)
        {
            var platform = XamlPlatforms.ResolvePlatform(context.Project);
            var baseType = ContextualBaseClassResolver.GetSuggestedBaseClass(context.Project, input.VirtualFolderPath, (type) =>
            {
                return IsVisualElement(type, platform);
            });

            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, input.FolderPath);

            return XamlViewWithCodeBehindGenerator.Generate(input.NameNoExtension, @namespace, "local", context.Project, platform, input.FolderPath, baseType.ToString());
        }
    }
}
