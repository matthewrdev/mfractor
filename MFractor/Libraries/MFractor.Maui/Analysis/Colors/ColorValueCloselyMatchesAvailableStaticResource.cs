using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Colors
{
    class ColorValueCloselyMatchesAvailableStaticResource : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1086";

        public override string Identifier => "com.mfractor.code.analysis.xaml.color_value_closely_matches_available_static_resources";

        public override string Name => "Color Value Closely Matches Available Static Resource";

        public override string Documentation => "Inspects color values and checks if they closely match the color value defined by a static resource.";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ColorExecutionFilter;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public ColorValueCloselyMatchesAvailableStaticResource(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (property == null
                || !syntax.HasValue
                || !FormsSymbolHelper.IsColor(property.Type, context.Platform))
            {
                return Array.Empty<ICodeIssue>();
            }

            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
            {
                return Array.Empty<ICodeIssue>();
            }

            if (!ColorHelper.TryParseHexColor(syntax.Value.Value, out var color, out var hasAlpha))
            {
                return Array.Empty<ICodeIssue>();
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            if (!database.IsValid)
            {
                return default;
            }

            var colourDefinitionRepo = database.GetRepository<ColorDefinitionRepository>();

            var matches = colourDefinitionRepo.GetCloselyMatchingColors(color);
            if (!matches.Any())
            {
                return default;
            }

            var message = $"This color, {syntax.Value.Value}, closely matches multiple available color resources.";
            if (matches.Count == 1)
            {
                var firstMatch = matches.First();
                message = $"This color, {syntax.Value.Value}, closely matches the color defined by the resource '{firstMatch.Name}'.";
            }

            message += "\n\nWould you like to replace this color value with a static resource lookup instead?";

            return CreateIssue(message, syntax, syntax.Value.Span, new ColorValueCanBeReplacedWithStaticResourceBundle(matches)).AsList();
        }
    }
}
