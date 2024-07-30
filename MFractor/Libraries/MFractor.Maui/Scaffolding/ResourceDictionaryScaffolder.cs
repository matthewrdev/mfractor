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
    class ResourceDictionaryScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.Code.Scaffolding.xaml.resource_dictionary";

        public override string Name => "Resource Dictionary Scaffolder";

        public override string Documentation => "Creates a new ResourceDictionary within the Resources folder.";

        public override string Criteria => "Activates when the project is a XAML platform, the folder path ends with Resources and the file extension is '.xaml'.";

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
            if (!string.Equals(input.VirtualFolderPath.LastOrDefault(), "Resources", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return input.Extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create a new ResourceDictionary").AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            var platform = XamlPlatforms.ResolvePlatform(context.Project);
            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, input.FolderPath);

            return XamlViewWithCodeBehindGenerator.Generate(input.NameNoExtension, @namespace, "local", context.Project, platform, input.FolderPath, platform.ResourceDictionary.MetaType);
        }
    }
}