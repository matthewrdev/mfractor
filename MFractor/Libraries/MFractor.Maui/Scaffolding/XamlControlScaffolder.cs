using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Scaffolding;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.Maui.Scaffolding
{
    class XamlControlScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.Code.Scaffolding.xaml.xaml_control";

        public override string Name => "XAML Control Scaffolder";

        public override string Documentation => "Creates a new XAML control inheriting from Grid with a code behind class";

        public override string Criteria => "Activates when the file extension is '.xaml'.";

        [Import]
        public IXamlViewWithCodeBehindGenerator XamlViewWithCodeBehindGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        public XamlControlScaffolder(Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.xamlPlatforms = xamlPlatforms;
        }

        public override bool IsAvailable(IScaffoldingContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return input.Extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create a new XAML control with code behind").AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            var project = context.Project;
            if (!project.TryGetCompilation(out var compilation))
            {
                return Array.Empty<IWorkUnit>();
            }

            var platform = XamlPlatforms.ResolvePlatform(project, compilation);
            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, input.FolderPath);

            return XamlViewWithCodeBehindGenerator.Generate(input.NameNoExtension, @namespace, "local", context.Project, platform, input.FolderPath, platform.Grid.MetaType);
        }
    }
}