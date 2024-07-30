using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.StaticResources
{
    class AmbiguousStaticResourceReference : ExpressionTypeAnalysisRoutine<StaticResourceExpression>
    {
        readonly Lazy<IProjectService> projectService;
        IProjectService ProjectService => projectService.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.ambiguous_static_resource";

        public override string Name => "Ambiguous Static Resource Reference";

        public override string Documentation => "Inspects StaticResource expressions and checks if one or more static resources will be returned from the resource expression.";

        public override string DiagnosticId => "MF1054";


        [ImportingConstructor]
        public AmbiguousStaticResourceReference(Lazy<IProjectService> projectService,
                                                Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.projectService = projectService;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(StaticResourceExpression expression,
                                                                   IParsedXamlDocument document,
                                                                   IXamlFeatureContext context)
        {
            var resourceName = expression.Value?.Value;
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            if (!TryGetPreprocessor<StaticResourceAnalysisPreprocessor>(context, out var preprocessor))
            {
                return null;
            }

            var resources = preprocessor.FindNamedStaticResources(resourceName);

            if (resources == null || resources.Count() <= 1)
            {
                return null;
            }

            var message = $"This static resource expression is ambiguous as there are multiple resources available to this file with the key '{resourceName}'";

            message += Environment.NewLine;
            message += Environment.NewLine;

            message += "The resource '" + resourceName + "' is defined in the following files:";
            message += Environment.NewLine;

            var filePaths = new HashSet<string>();

            foreach (var resource in resources)
            {
                var definition = resource;
                var project = preprocessor.StaticResourceCollection.GetProjectFor(resource);
                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
                var file = database.GetRepository<ProjectFileRepository>().GetProjectFileById(definition.ProjectFileKey);

                var projectFile = ProjectService.GetProjectFileWithFilePath(project, file.FilePath);

                if (projectFile != null)
                {
                    var path = projectFile.ProjectFolders.Any() ? string.Join("/", projectFile.ProjectFolders) : "";

                    if (string.IsNullOrEmpty(path) == false)
                    {
                        path = Path.Combine(path, file.FileName);
                    }
                    else
                    {
                        path = file.FileName;
                    }

                    filePaths.Add(path);

                    message += " * " + path;
                    message += Environment.NewLine;
                }
            }

            if (filePaths.Count <= 1)
            {
                return null;
            }

            return CreateIssue(message, expression.ParentAttribute, expression.Span).AsList();
        }

    }
}