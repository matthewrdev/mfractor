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
    class ColumnIsOutsideGridBoundaryAnalysis : XamlCodeAnalyser
	{
        public override IssueClassification Classification => IssueClassification.Warning;

		public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.column_is_outside_grid_boundaries";

		public override string Name => "Column Is Outside Grid Boundaries";

		public override string Documentation => "This code analyser inspects usages of the `Grid.Column` attribute and validates that the column provided is within the total columns declared by the parent grid.";

        public override string DiagnosticId => "MF1024";

		public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.GridContainerExecutionFilter;

		readonly Lazy<IGridAxisResolver> gridAxisResolver;
		public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

		[ImportingConstructor]
		public ColumnIsOutsideGridBoundaryAnalysis(Lazy<IGridAxisResolver> gridAxisResolver)
		{
			this.gridAxisResolver = gridAxisResolver;
		}

		protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var columnPropertyName = $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}";

			if (syntax.Name.LocalName != columnPropertyName
                && syntax.HasValue)
			{
				return null;
			}

            if (!int.TryParse(syntax.Value.Value, out var column))
			{
				return null;
			}

			var gridContainer = syntax.Parent?.Parent;

			if (gridContainer == null)
			{
				return null;
			}

			var gridType = context.Compilation.GetTypeByMetadataName(context.Platform.Grid.MetaType);

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

			var columns = GridAxisResolver.ResolveColumnDefinitions(gridContainer, context.Platform);

			var columnCount = columns.Count();

			if (column < columnCount)
			{
				return null;
			}

            return CreateIssue($"The column '{column}' is outside the total columns declared by the parent grid ({column} columns). Was this intended?", syntax, syntax.Span).AsList();
		}
	}
}
