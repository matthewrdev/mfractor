using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Scaffolding;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.XamlPlatforms.Maui;
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

        readonly Lazy<MauiXamlPlatform> mauiPlatform;
        public MauiXamlPlatform MauiPlatform => mauiPlatform.Value;

        [ImportingConstructor]
        public XamlControlScaffolder(Lazy<MauiXamlPlatform> mauiPlatform)
        {
            this.mauiPlatform = mauiPlatform;
        }

        public override bool IsAvailable(IScaffoldingContext context)
        {
            return MauiPlatform.Supports(context.Project);
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

            var platform = MauiPlatform.Resolve(project, compilation);
            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, input.FolderPath);

            return XamlViewWithCodeBehindGenerator.Generate(input.NameNoExtension, @namespace, "local", context.Project, platform, input.FolderPath, platform.Grid.MetaType);
        }
    }
}
