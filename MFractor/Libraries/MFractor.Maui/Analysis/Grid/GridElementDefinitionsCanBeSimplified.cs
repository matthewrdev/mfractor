using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Grids;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Grid
{
    class GridAxisDefinitionsCanBeSimplified : XamlCodeAnalyser
    {
        readonly Lazy<IGridAxisResolver> gridAxisResolver;
        public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.grid_axis_definitions_can_be_simplified";

        public override string Name => "Grid Axis Definition Can Be Simplified";

        public override string Documentation => "This code analyser inspects usages of the `Grid.Column` and `Grid.Row` attributes and suggests if they can be converted into the simplified attribute syntax.";

        public override string DiagnosticId => "MF1102";

        [ImportingConstructor]
        public GridAxisDefinitionsCanBeSimplified(Lazy<IGridAxisResolver> gridAxisResolver)
        {
            this.gridAxisResolver = gridAxisResolver;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var platform = context.Platform;
            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;

            if (property is null
                || !SymbolHelper.DerivesFrom(property.ContainingType, context.Platform.Grid.MetaType))
            {
                return null;
            }

            var isAxisSetter = property.Name == platform.RowDefinitionsProperty || property.Name == platform.ColumnDefinitionsProperty;
            if (!isAxisSetter)
            {
                return null;
            }

            // Is simplified syntax supported?
            if (!FormsSymbolHelper.HasTypeConverterAttribute(property, context.Platform))
            {
                return null;
            }

            IEnumerable<IGridAxisDefinition> axisDefinitions = null;
            if (property.Name == platform.RowDefinitionsProperty)
            {
                axisDefinitions = GridAxisResolver.ResolveRowDefinitions(syntax.Parent, platform);
            }
            else if (property.Name == platform.ColumnDefinitionsProperty)
            {
                axisDefinitions = GridAxisResolver.ResolveColumnDefinitions(syntax.Parent, platform);
            }

            if (axisDefinitions is null
                || !axisDefinitions.Any()
                || axisDefinitions.Any(d => ExpressionParserHelper.IsExpression(d.Value) || d.HasName))
            {
                return null;
            }

            var simpliedValue = string.Join(", ", axisDefinitions.Select(e => e.Value.Trim()));
            var bundle = new GridAxisDefinitionsCanBeSimplifiedBundle(simpliedValue);

            return CreateIssue($"This {property.Name} declaration can be simplified to '{property.Name}=\"{simpliedValue}\".", syntax, syntax.OpeningTagSpan, bundle).AsList();
        }
    }
}