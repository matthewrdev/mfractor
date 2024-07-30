using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Grids;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Grid
{
    class RowSpanIsOutsideGridBoundaryAnalysis : XamlCodeAnalyser
	{
		public override IssueClassification Classification => IssueClassification.Warning;

		public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.rowspan_is_outside_grid_boundaries";

		public override string Name => "RowSpan Is Outside Grid Boundaries";

		public override string Documentation => "This code analyser inspects usages of the `Grid.RowSpan` attribute and validates that the span provided is within the total rows declared by the parent grid.";

        public override string DiagnosticId => "MF1033";

		readonly Lazy<IGridAxisResolver> gridAxisResolver;
		public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

		public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.GridContainerExecutionFilter;

		[ImportingConstructor]
		public RowSpanIsOutsideGridBoundaryAnalysis(Lazy<IGridAxisResolver> gridAxisResolver)
		{
			this.gridAxisResolver = gridAxisResolver;
		}

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var rowPropertyName = $"{context.Platform.Grid.Name}.{context.Platform.RowProperty}";
			var rowSpanPropertyName = $"{context.Platform.Grid.Name}.{context.Platform.RowProperty}Span";

			if (syntax.Name.LocalName != rowSpanPropertyName
                && syntax.HasValue)
			{
				return null;
			}

            if (!int.TryParse(syntax.Value.Value, out var span))
			{
				return null;
			}

			var rowAttr = syntax.Parent.GetAttribute(attr => attr.Name.LocalName == rowPropertyName);
			if (rowAttr == null)
			{
				return null;
			}

            if (!int.TryParse(rowAttr.Value.Value, out var row))
			{
				return null;
			}

			var gridContainer = syntax.Parent?.Parent;

			if (gridContainer == null)
			{
				return null;
			}

            var gridType = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);
			var rowDefinitionType = context.Compilation.GetTypeByMetadataName(context.Platform.RowDefinition.MetaType);

			var property = context.XamlSemanticModel.GetSymbol(syntax) as IFieldSymbol;

            var gridSymbol = context.XamlSemanticModel.GetSymbol(gridContainer) as INamedTypeSymbol;
			if (gridSymbol == null
				|| !SymbolHelper.DerivesFrom(gridSymbol, gridType))
			{
				return null;
			}

			if (!GridAxisResolver.DeclaresRows(gridContainer, context.Platform))
			{
				return null;
			}

			var rows = GridAxisResolver.ResolveRowDefinitions(gridContainer, context.Platform).ToList();

			var rowCount = rows.Count;

			var rowEnd = row + span;

			if (rowEnd <= rowCount)
			{
				return null;
			}

            return CreateIssue($"This rows span exceeds the total amount of rows declared by the parent grid.\n\nIt starts a row {row} and has a span of {span}, placing its end row at index {row + span}. The parent grid declares {rowCount} rows.  Was this intended?", syntax, syntax.Span).AsList();
		}
	}
}
