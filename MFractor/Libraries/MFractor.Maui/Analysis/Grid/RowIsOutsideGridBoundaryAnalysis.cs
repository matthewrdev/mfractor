using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Maui.Grids;

namespace MFractor.Maui.Analysis.Grid
{
    class RowIsOutsideGridBoundaryAnalysis : XamlCodeAnalyser
	{
		public override IssueClassification Classification => IssueClassification.Warning;

		public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.row_is_outside_grid_boundaries";

		public override string Name => "Row Is Outside Grid Boundaries";

		public override string Documentation => "This code analyser inspects usages of the `Grid.Row` attribute and validates that the row provided is within the total rows declared by the parent grid.";

        public override string DiagnosticId => "MF1032";

        readonly Lazy<IGridAxisResolver> gridAxisResolver;
        public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

		public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.GridContainerExecutionFilter;

		[ImportingConstructor]
		public RowIsOutsideGridBoundaryAnalysis(Lazy<IGridAxisResolver> gridAxisResolver)
        {
            this.gridAxisResolver = gridAxisResolver;
        }

		protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var rowPropertyName = $"{context.Platform.Grid.Name}.{context.Platform.RowProperty}";
			if (syntax.Name.LocalName != rowPropertyName
                && syntax.HasValue)
			{
				return null;
			}

            if (!int.TryParse(syntax.Value.Value, out var row))
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
			
			if (!GridAxisResolver.DeclaresRows(gridContainer, context.Platform))
			{
				return null;
			}

			var rows = GridAxisResolver.ResolveRowDefinitions(gridContainer, context.Platform);

			var rowCount = rows.Count();

			if (row < rowCount)
			{
				return null;
			}

            return CreateIssue($"The row '{row}' is outside the total rows declared by the parent grid ({rowCount} rows). Was this intended?", syntax, syntax.Span).AsList();
		}
	}
}