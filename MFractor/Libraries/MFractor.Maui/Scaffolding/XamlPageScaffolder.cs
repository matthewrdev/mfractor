using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Scaffolding;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.Maui.Scaffolding
{
    class XamlPageScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.code.scaffolding.xaml.xaml_page";

        public override string Name => "XAML Page Scaffolder";

        public override string Documentation => "Creates a new XAML page with a code behind class";

        public override string Criteria => "Activates when the file name ends with 'Page' and the file extension is '.xaml'.";

        [Import]
        public IXamlViewWithCodeBehindGenerator XamlViewWithCodeBehindGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        public IXamlPlatformRepository XamlPlatforms { get; set; }

        public override bool IsAvailable(IScaffoldingContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return input.Extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase)
                   && input.NameNoExtension.EndsWith("page", StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create a new XAML page with code behind").AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            var platform = XamlPlatforms.ResolvePlatform(context.Project);
            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, input.FolderPath);

            return XamlViewWithCodeBehindGenerator.Generate(input.NameNoExtension, @namespace, "local", context.Project, platform, input.FolderPath, platform.ContentPage.MetaType);
        }
    }
}