using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Scaffolding;
using MFractor.Maui.CodeGeneration.ValueConversion;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace.Utilities;

namespace MFractor.Maui.Scaffolding
{
    class ValueConversionAttributeScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => "Value Conversion Attribute Scaffolder";

        public override string Identifier => "com.mfractor.Code.Scaffolding.xaml.value_conversion_attribute";

        public override string Name => "Value Conversion Attribute Scaffolder";

        public override string Documentation => "Generates new ValueConversionAttribute declaration";

        public override string Criteria => "Activates when the project is a XAML platform and the file name is 'ValueConversionAttribute'.";

        [Import]
        public IValueConversionAttributeImplementationGenerator ValueConversionAttributeImplementationGenerator { get; set; }

        [Import]
        public IXamlPlatformRepository XamlPlatforms { get; set; }

        public override bool IsAvailable(IScaffoldingContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return input.NameNoExtension.Equals("ValueConversionAttribute", StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            return ValueConversionAttributeImplementationGenerator.Generate(context.Project.GetIdentifier(), input.RawInput);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create ValueConversionAttribute", int.MaxValue).AsList();
        }
    }
}
