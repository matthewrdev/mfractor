using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Grids;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class ColumnSpanIsOutsideGridBoundaryAnalysis : XamlCodeAnalyser
	{
        public override IssueClassification Classification => IssueClassification.Warning;

		public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.columnspan_is_outside_grid_boundaries";

		public override string Name => "ColumnSpan Is Outside Grid Boundaries";

		public override string Documentation => "This code analyser inspects usages of the `Grid.ColumnSpan` attribute and validates that the span provided is within the total columns declared by the parent grid.";

        public override string DiagnosticId => "MF1025";

		public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.GridContainerExecutionFilter;

		readonly Lazy<IGridAxisResolver> gridAxisResolver;
		public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

		[ImportingConstructor]
		public ColumnSpanIsOutsideGridBoundaryAnalysis(Lazy<IGridAxisResolver> gridAxisResolver)
		{
			this.gridAxisResolver = gridAxisResolver;
		}

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var propertyName = $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}Span";
			if (syntax.Name.LocalName != propertyName
                && syntax.HasValue)
			{
				return null;
			}

            if (!int.TryParse(syntax.Value.Value, out var span))
            {
                return null;
            }

            var columnAttr = syntax.Parent.GetAttribute(attr => attr.Name.LocalName == $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}");
			if (columnAttr == null)
			{
				return null;
			}

			if (!int.TryParse(columnAttr.Value.Value, out var column))
            {
                return null;
            }

            var gridContainer = syntax.Parent?.Parent;

			if (gridContainer == null)
			{
				return null;
			}

            var gridType = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);

			var property = context.XamlSemanticModel.GetSymbol(syntax) as IFieldSymbol;

            var gridSymbol = context.XamlSemanticModel.GetSymbol(gridContainer) as INamedTypeSymbol;
			if (gridSymbol == null
				|| !SymbolHelper.DerivesFrom(gridSymbol, gridType))
			{
				return null;
			}

			if (!GridAxisResolver.DeclaresColumns(gridContainer, context.Platform))
            {
				return null;
            }

			var columns = GridAxisResolver.ResolveColumnDefinitions(gridContainer, context.Platform).ToList();

			var columnCount = columns.Count;

			var columnEnd = column + span;

			if (columnEnd <= columnCount)
			{
				return null;
			}

			return CreateIssue($"This columns span exceeds the total amount of columns declared by the parent grid.\n\nIt starts a column {column} and has a span of {span}, placing its end column at index {column + span}. The parent grid declares {columnCount} columns.  Was this intended?", syntax, syntax.Span).AsList();
		}
	}
}
