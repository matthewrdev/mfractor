using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Scaffolding;
using MFractor.Maui.CodeGeneration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.Maui.Scaffolding
{
    class DesignTimeBindingContextAttributeScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => "Design Time Binding Context Attribute Scaffolder";

        public override string Identifier => "com.mfractor.Code.Scaffolding.xaml.design_time_binding_context_attribute";

        public override string Name => "Generate DesignTimeBindingContextAttribute";

        public override string Documentation => "Generates new DesignTimeBindingContextAttribute declaration";

        public override string Criteria => "Activates when the project is a XAML-based project and the file name is 'DesignTimeBindingContextAttribute'.";

        [Import]
        public IDesignTimeBindingContextAttributeGenerator DesignTimeBindingContextAttributeGenerator { get; set; }

        [Import]
        public IXamlPlatformRepository XamlPlatforms { get; set; }

        public override bool IsAvailable(IScaffoldingContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return input.NameNoExtension.Equals("DesignTimeBindingContextAttribute", StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            var @namespace = DesignTimeBindingContextAttributeGenerator.GetNamespaceFor(context.Project, input.FolderPath);

            return DesignTimeBindingContextAttributeGenerator.Generate(context.Project, @namespace, input.RawInput);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create Design Time Binding Context Attribute", int.MaxValue).AsList();
        }
    }
}
