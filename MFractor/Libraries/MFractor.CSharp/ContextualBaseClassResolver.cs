using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Utilities.SyntaxWalkers;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MFractor.CSharp
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IContextualBaseClassResolver))]
    class ContextualBaseClassResolver : IContextualBaseClassResolver
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public ContextualBaseClassResolver(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        public int MatchHeuristic { get; set; } = 3;

        public IReadOnlyDictionary<INamedTypeSymbol, int> GetSuggestedBaseClasses(Project project, IReadOnlyList<string> virtualFolderPath, TypeFilterDelegate typeFilterDelegate = null)
        {
            if (project is null || virtualFolderPath is null)
            {
                return new Dictionary<INamedTypeSymbol, int>();
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return new Dictionary<INamedTypeSymbol, int>();
            }

            typeFilterDelegate = typeFilterDelegate ?? DefaultTypeFilter;


            var projectFiles = ProjectService.GetProjectFiles(project, virtualFolderPath).ToList();

            if (!projectFiles.Any())
            {
                return new Dictionary<INamedTypeSymbol, int>();
            }

            var indexedProjectFiles = projectFiles.ToDictionary(pf => pf.FilePath, pf => pf);

            var matches = new Dictionary<INamedTypeSymbol, int>();

            var documents = project.Documents.Where(d => indexedProjectFiles.ContainsKey(d.FilePath)).ToList();

            foreach (var doc in documents)
            {
                if (!doc.TryGetSyntaxTree(out var ast))
                {
                    continue;
                }

                var model = compilation.GetSemanticModel(ast);

                var walker = new ClassDeclarationSyntaxWalker();
                walker.Visit(ast.GetRoot());

                var classes = walker.Classes;

                if (classes == null || !classes.Any())
                {
                    continue;
                }

                foreach (var c in classes)
                {
                    var classType = model.GetDeclaredSymbol(c);

                    if (classType != null)
                    {
                        var baseType = classType.BaseType;

                        if (baseType.SpecialType != SpecialType.None)
                        {
                            if (classType.Interfaces.Any())
                            {
                                baseType = classType.Interfaces.FirstOrDefault();
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (!typeFilterDelegate(baseType))
                        {
                            continue;
                        }

                        if (matches.ContainsKey(baseType))
                        {
                            matches[baseType]++;
                        }
                        else
                        {
                            matches[baseType] = 1;
                        }
                    }
                }
            }

            return matches;
        }

        static bool DefaultTypeFilter(INamedTypeSymbol namedType) => true;

        public INamedTypeSymbol GetSuggestedBaseClass(Project project, IReadOnlyList<string> virtualFolderPath, TypeFilterDelegate typeFilterDelegate = null, int matchHeuristic = 2)
        {
            var matches = GetSuggestedBaseClasses(project, virtualFolderPath, typeFilterDelegate);

            if (!matches.Any())
            {
                return default;
            }

            var max = 0;
            INamedTypeSymbol candidate = null;
            foreach (var match in matches)
            {
                if (match.Value > max && match.Value >= matchHeuristic)
                {
                    candidate = match.Key;
                    matchHeuristic = match.Value;
                }
            }

            return candidate;
        }
    }
}