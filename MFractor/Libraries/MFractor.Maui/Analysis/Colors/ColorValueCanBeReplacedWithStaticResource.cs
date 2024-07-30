using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
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
    class ColorValueCanBeReplacedWithStaticResource : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1079";

        public override string Identifier => "com.mfractor.code.analysis.xaml.color_value_can_be_replaced_with_static_resource";

        public override string Name => "Color Value Matches Static Resource";

        public override string Documentation => "When assigning a color a hexadecimal or named constant, this code inspection detects if that color value matches an available color resource.";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ColorExecutionFilter;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public ColorValueCanBeReplacedWithStaticResource(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
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
                return default;
            }

            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
            {
                return default;
            }

            Color color;
            try
            {
                color = ColorTranslator.FromHtml(syntax.Value.Value);
            }
            catch
            {
                return default;
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            if (!database.IsValid)
            {
                return default;
            }

            var colorInteger = color.ToArgb();
            var colourDefinitionRepo = database.GetRepository<ColorDefinitionRepository>();

            var matchingColorResources = colourDefinitionRepo.GetColorDefinitionsForColorInteger(colorInteger);
            if (!matchingColorResources.Any())
            {
                return default;
            }

            var message = $"This color value, {syntax.Value.Value}, matches multiple available color resources.";
            if (matchingColorResources.Count == 1)
            {
                message = $"This color value, {syntax.Value.Value}, matches the value provided by the available resource '{matchingColorResources.First().Name}'.";
            }

            message += "\n\nWould you like to replace this color with a static resource lookup instead?";

            return CreateIssue(message, syntax, syntax.Value.Span, new ColorValueCanBeReplacedWithStaticResourceBundle(matchingColorResources)).AsList();
        }
    }
}
