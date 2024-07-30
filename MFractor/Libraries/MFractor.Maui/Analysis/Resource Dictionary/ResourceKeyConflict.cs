using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.ResourceDictionary
{
    class ResourceKeyConflict : XamlCodeAnalyser
    {

        public override string Documentation => "Inspects resource key declarations and validates that the resource key is not already defined in other files that are used by this file.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.resource_key_conflict";

        public override string Name => "Resource Key Conflict";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1044";

        readonly Lazy<IStaticResourceResolver> staticResourceResolver;
        public IStaticResourceResolver StaticResourceResolver => staticResourceResolver.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public ResourceKeyConflict(Lazy<IStaticResourceResolver> staticResourceResolver,
                                   Lazy<IProjectService> projectService,
                                   Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.staticResourceResolver = staticResourceResolver;
            this.projectService = projectService;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax,
                                                           IParsedXamlDocument document,
                                                           IXamlFeatureContext context)
        {
            if (!syntax.HasValue)
            {
                return null;
            }

            if (!XamlSyntaxHelper.IsDictionaryKey(syntax, context.Namespaces))
            {
                return null;
            }

            var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(parentSymbol,context.Platform.ResourceDictionary.MetaType))
            {
                return null;
            }

            var resourceName = syntax.Value.Value;

            var resources = StaticResourceResolver.FindNamedStaticResources(context.Project, context.Platform, document.FilePath, resourceName);

            if (resources == null || resources.Count() <= 1)
            {
                return null;
            }

            var message = "This resource key conflicts with other resources defined by this XAML files dependencies.";

            message += Environment.NewLine;
            message += Environment.NewLine;

            message += "This resource key is also defined in:";
            message += Environment.NewLine;

            var conflicts = false;

            foreach (var resource in resources)
            {
                var definition = resource;
                var project = resources.GetProjectFor(definition);
                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
                var file = database.GetRepository<ProjectFileRepository>().GetProjectFileById(definition.ProjectFileKey);

                var projectFile = ProjectService.GetProjectFileWithFilePath(project, file.FilePath);

                if (projectFile != null
                    && !string.Equals(projectFile.FilePath, document.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    conflicts = true;
                    var path = projectFile.ProjectFolders.Any() ? string.Join("/", projectFile.ProjectFolders) : "";

                    if (string.IsNullOrEmpty(path) == false)
                    {
                        path = Path.Combine(path, file.FileName);
                    }
                    else
                    {
                        path = file.FileName;
                    }

                    message += " * " + path;
                    message += Environment.NewLine;
                }
            }

            if (!conflicts)
            {
                return null;
            }

            return CreateIssue(message, syntax, syntax.Span).AsList();
        }

    }
}
