using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Repositories;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.AutomationId
{
    class DuplicateAutomationIdsAnalysis : XamlCodeAnalyser
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        public override string Documentation => "Inspects a Xaml document for occurances of duplicate `AutomationId` declarations.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.duplicate_automation_ids";

        public override string Name => "Duplicate AutomationIds";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1000";

        [ImportingConstructor]
        public DuplicateAutomationIdsAnalysis(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != "AutomationId"
                || !syntax.HasValue)
            {
                return null;
            }

            var name = syntax.Value.Value;

            var db = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            if (db == null)
            {
                return null;
            }

            var file = db.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(document.FilePath);

            if (file == null)
            {
                return null;
            }

            var ids = db.GetRepository<AutomationIdDeclarationRepository>().GetAllNamedAutomationIdInFile(name, file);

            if (ids == null || ids.Count <= 1)
            {
                return null;
            }

            return CreateIssue($"The automation id '{syntax.Value}' is declared multiple times", syntax, syntax.Value.Span).AsList();
        }
    }
}

