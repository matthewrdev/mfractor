using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Data;
using MFractor.Maui.Data.Repositories;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Colors
{
    class ColorValueCanBeConsolidated : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects color values and verifies if there are other colors in the projects that also declare that specific color.";

        public override IssueClassification Classification => IssueClassification.Improvement;

        public override string Identifier => "com.mfractor.code.analysis.xaml.color_value_can_be_consolidated";

        public override string Name => "Color Value Can Be Consolidated";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1094";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ColorExecutionFilter;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public ColorValueCanBeConsolidated(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return Array.Empty<ICodeIssue>();
            }

            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (property == null)
            {
                return Array.Empty<ICodeIssue>();
            }

            var colorType = context.Compilation.GetTypeByMetadataName(context.Platform.Color.MetaType);
            if (!property.Type.Equals(colorType))
            {
                return Array.Empty<ICodeIssue>();
            }

            if (syntax.HasValue == false
                || !ColorHelper.TryParseHexColor(syntax.Value.Value, out var color, out var hasAlpha))
            {
                return Array.Empty<ICodeIssue>();
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            if (!database.IsValid)
            {
                return Array.Empty<ICodeIssue>();
            }

            var colorInteger = color.ToArgb();
            var colourDefinitionRepo = database.GetRepository<ColorDefinitionRepository>();

            var hasMatchingStaticResourceColors = colourDefinitionRepo.HasColorDefinitionsForColor(colorInteger);
            if (hasMatchingStaticResourceColors) // Ignore static resource matches, leave that to another analyser
            {
                return Array.Empty<ICodeIssue>();
            }

            var colourUsageRepo = database.GetRepository<ColorUsageRepository>();

            var usages = colourUsageRepo.GetCountOfHexadecimalColorUsagesWithValue(colorInteger);

            if (usages <= 1)
            {
                return Array.Empty<ICodeIssue>();
            }

            return CreateIssue($"This color, '{syntax.Value.Value}', is used {usages} times throughout the app and could be consolidated into a static resource.", syntax, syntax.Value.Span).AsList();
        }
    }
}

