using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.Grid;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Grids
{
    public class SimplifyGridAxisDefinition : FixCodeAction
    {
        public override string Documentation => "When a XAML element specifies grid rows, columns or spans, however, it is not within a grid element, this code fix removes all redundant grid properties applied to the element.";

        public override Type TargetCodeAnalyser => typeof(GridAxisDefinitionsCanBeSimplified);

        public override string Identifier => "com.mfractor.code_fixes.xaml.simplify_grid_axis_definition";

        public override string Name => "Simplify Grid Axis Definition";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Simplify {syntax.Name.FullName}").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,
                                                          XmlNode syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context,
                                                          ICodeActionSuggestion suggestion,
                                                          InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<GridAxisDefinitionsCanBeSimplifiedBundle>();
            XamlSyntaxHelper.ExplodePropertySetter(syntax, out _, out var propertyName);

            var result = new List<IWorkUnit>()
            {
                new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntax = syntax,
                }
            };

            result.Add(new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax.Parent,
                Syntax = new XmlAttribute(propertyName, bundle.Simplification)
            });

            return result;
        }
    }
}
