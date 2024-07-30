using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Thickness
{
    class ThicknessCanBeConsolidated : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects thickness values and verifies if there are other thickness in the projects that also declare ";

        public override IssueClassification Classification => IssueClassification.Improvement;

        public override string Identifier => "com.mfractor.code.analysis.xaml.thickness_value_can_be_consolidated";

        public override string Name => "Thickness Value Can Be Consolidated";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1092";

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.ThicknessExecutionFilter;

        [ImportingConstructor]
        public ThicknessCanBeConsolidated(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
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

            var thicknessType = context.Compilation.GetTypeByMetadataName(context.Platform.Thickness.MetaType);
            if (!property.Type.Equals(thicknessType))
            {
                return Array.Empty<ICodeIssue>();
            }

            if (syntax.HasValue == false
                || !ThicknessHelper.ProcessThickness(syntax.Value.Value, out var left, out var right, out var top, out var bottom))
            {
                return Array.Empty<ICodeIssue>();
            }

            var formattedValue = ThicknessHelper.ToFormattedValueString(left, right, top, bottom);

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);

            if (!database.IsValid)
            {
                return Array.Empty<ICodeIssue>();
            }

            var repo = database.GetRepository<ThicknessUsageRepository>();

            var usages = repo.GetCountOfThicknessUsagesWithValue(formattedValue);

            if (usages  <= 1)
            {
                return Array.Empty<ICodeIssue>();
            }

            return CreateIssue($"This thickness value, '{syntax.Value.Value}', is used {usages} times throughout the app and could be consolidated into a static resource.", syntax, syntax.Value.Span).AsList();
        }
    }
}

