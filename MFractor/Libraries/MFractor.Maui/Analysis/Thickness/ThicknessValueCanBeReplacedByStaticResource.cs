using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Thickness
{
    class ThicknessValueCanBeReplacedByStaticResource : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Improvement;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1080";

        public override string Identifier => "com.mfractor.code.analysis.xaml.thickness_value_can_be_replaced_with_static_resource";

        public override string Name => "Thickness Value Can Be Replaced By Static Resource";

        public override string Documentation => "Inspects assignments of `Thickness` values in XAML and matches them to declared static resources.";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ThicknessExecutionFilter;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public ThicknessValueCanBeReplacedByStaticResource(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }


        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return null;
            }

            var property = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (property == null)
            {
                return null;
            }

            var thicknessType = context.Compilation.GetTypeByMetadataName(context.Platform.Thickness.MetaType);
            if (property.Type != thicknessType)
            {
                return null;
            }

            if (syntax.HasValue == false)
            {
                return null;
            }

            if (!ThicknessHelper.ProcessThickness(syntax.Value.Value, out var left, out var right, out var top, out var bottom))
            {
                return null;
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            if (!database.IsValid)
            {
                return default;
            }

            var thicknessDefinitionRepo = database.GetRepository<ThicknessDefinitionRepository>();

            var matches = thicknessDefinitionRepo.FindMatchingThicknessDefinitions(left, right, top, bottom);
            if (!matches.Any())
            {
                return null;
            }

            var message = $"This thickness value, {syntax.Value.Value}, matches multiple available thickness resources.";
            if (matches.Count == 1)
            {
                message = $"This thickness value, {syntax.Value.Value}, matches the value provided by the available resource '{matches.First().Name}'.";
            }

            message += "\n\nWould you like to replace this thickness with a static resource lookup instead?";

            return CreateIssue(message, syntax, syntax.Value.Span, new ThicknessValueCanBeReplacedByStaticResourceBundle(matches)).AsList();
        }
    }
}
