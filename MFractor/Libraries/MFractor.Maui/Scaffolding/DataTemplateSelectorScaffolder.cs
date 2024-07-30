using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.DataTemplates;
using MFractor.Code.Scaffolding;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Scaffolding
{
    class DataTemplateSelectorScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => "Data Template Selector Scaffolder";

        public override string Identifier => "com.mfractor.Code.Scaffolding.xaml.data_template_selector_scaffolder";

        public override string Name => "Generate Data Template Selector";

        public override string Documentation => "Generates a new Data Template Selector declaration";

        public override string Criteria => "Activates when the project is a XAML platform and the file name ends with 'DataTemplateSelector'.";

        [Import]
        public IDataTemplateSelectorGenerator DataTemplateSelectorGenerator { get; set; }

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
            return input.NameNoExtension.EndsWith("DataTemplateSelector", StringComparison.OrdinalIgnoreCase)
                   || input.NameNoExtension.EndsWith("DataTemplateSelecter", StringComparison.OrdinalIgnoreCase); // 'Selecter' == typo tolerance.
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            var @namespace = DataTemplateSelectorGenerator.GetNamespaceFor(context.Project, input.FolderPath);

            return DataTemplateSelectorGenerator.Generate(context.Project, @namespace, input.RawInput, input.NameNoExtension);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create a DataTemplateSelector named", int.MaxValue).AsList();
        }
    }
}
