using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Maui.Localisation;
using MFractor.Localisation.Configuration;
using MFractor.Localisation.WorkUnits;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Utilities;

namespace MFractor.Maui.CodeActions.Refactor
{
    class ReplaceWithLocalisationLookup : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_actions.xaml.replace_with_localisation_lookup";

        public override string Name => "Replace String With Localisation Lookup";

        public override string Documentation => "Replaces the given string literal with a resx lookup";

        [Import]
        public IXamlLocalisableStringsProvider XamlLocalisableStringsProvider { get; set; }

        [Import]
        public IDefaultResourceFile DefaultResourceFile { get; set; }

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return XamlLocalisableStringsProvider.CanLocalise(syntax, context.XamlSemanticModel, context.Platform);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Localise this string", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new LocaliseDocumentWorkUnit(context.Document,
                                                      context.Project,
                                                      GetTargetProjects(context),
                                                      context.SemanticModel,
                                                      syntax.Value.Span,
                                                      DefaultResourceFile.ProjectFilePath).AsList();
        }

        IReadOnlyList<Project> GetTargetProjects(IFeatureContext context)
        {
            var projects = new List<Project>();

            foreach (var projectReference in context.Project.ProjectReferences)
            {
                var target = context.Solution.Projects.FirstOrDefault(p => p.Id == projectReference.ProjectId);

                if (target != null)
                {
                    var resxFile = ProjectService.FindProjectFile(target, filePath => Path.GetExtension(filePath) == ".resx");

                    if (resxFile != null)
                    {
                        projects.Add(target);
                    }
                }
            }

            return projects;
        }
    }
}
