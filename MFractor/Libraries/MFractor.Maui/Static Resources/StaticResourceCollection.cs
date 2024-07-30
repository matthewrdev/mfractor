using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Data.Repositories;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.StaticResources
{
    class StaticResourceCollection : IStaticResourceCollection
    {
        public StaticResourceCollection(IResourcesDatabaseEngine resourcesDatabaseEngine, string filePath, Project project)
        {
            ResourcesDatabaseEngine = resourcesDatabaseEngine;
            FilePath = filePath;
            Project = project;
        }

        public IResourcesDatabaseEngine ResourcesDatabaseEngine { get; }
        public string FilePath { get; }
        public Project Project { get; }

        class ProjectResourceCollection
        {
            public ProjectResourceCollection(Project project)
            {
                Project = project ?? throw new ArgumentNullException(nameof(project));
            }

            public Project Project { get; }

            public Dictionary<string, int> SourceFiles { get; } = new Dictionary<string, int>();

            public Dictionary<int, HashSet<int>> SourceFileIndexedStaticResourceDefinitions { get; } = new Dictionary<int, HashSet<int>>();

            public Dictionary<int, StaticResourceDefinition> StaticResourceDefinitions { get; } = new Dictionary<int, StaticResourceDefinition>();

            internal void Add(ProjectFile projectFile, StaticResourceDefinition staticResourceDefinition)
            {
                if (projectFile is null
                    || staticResourceDefinition is null)
                {
                    return;
                }

                SourceFiles[projectFile.FilePath] = projectFile.PrimaryKey;

                Add(projectFile.PrimaryKey, staticResourceDefinition);
            }

            void Add(int projectFileKey, StaticResourceDefinition staticResourceDefinition)
            {
                if (staticResourceDefinition is null)
                {
                    return;
                }

                if (!SourceFileIndexedStaticResourceDefinitions.TryGetValue(projectFileKey, out var fileResourceGroup))
                {
                    fileResourceGroup = new HashSet<int>();
                    SourceFileIndexedStaticResourceDefinitions[projectFileKey] = fileResourceGroup;
                }

                fileResourceGroup.Add(staticResourceDefinition.PrimaryKey);
                StaticResourceDefinitions[staticResourceDefinition.PrimaryKey] = staticResourceDefinition;
            }

            internal void AddRange(ProjectFile projectFile, IEnumerable<StaticResourceDefinition> staticResourceDefinitions)
            {
                if (projectFile is null
                    || staticResourceDefinitions is null
                    || !staticResourceDefinitions.Any())
                {
                    return;
                }

                SourceFiles[projectFile.FilePath] = projectFile.PrimaryKey;

                foreach (var definition in staticResourceDefinitions)
                {
                    Add(projectFile.PrimaryKey, definition);
                }
            }
        }

        readonly Dictionary<Project, ProjectResourceCollection> projectResourceCollections = new Dictionary<Project, ProjectResourceCollection>();
        public IReadOnlyList<Project> Projects => projectResourceCollections.Keys.ToList();

        public IReadOnlyList<string> SourceFiles
        {
            get
            {
                var result = new List<string>();

                foreach (var collection in projectResourceCollections)
                {
                    result.AddRange(collection.Value.SourceFiles.Keys);
                }

                return result;
            }
        }


        public void Add(Project project, ProjectFile projectFile, StaticResourceDefinition staticResourceDefinition)
        {
            if (project is null
                || projectFile is null
                || staticResourceDefinition is null)
            {
                return;
            }

            var collection = GetResourceCollectionForProject(project);

            collection.Add(projectFile, staticResourceDefinition);
        }

        public void AddRange(Project project, ProjectFile projectFile, IEnumerable<StaticResourceDefinition> staticResourceDefinitions)
        {
            if (project is null
                || projectFile is null
                || staticResourceDefinitions is null
                || !staticResourceDefinitions.Any())
            {
                return;
            }

            var collection = GetResourceCollectionForProject(project);

            collection.AddRange(projectFile, staticResourceDefinitions);
        }

        ProjectResourceCollection GetResourceCollectionForProject(Project project)
        {
            if (!projectResourceCollections.TryGetValue(project, out var collection))
            {
                collection = new ProjectResourceCollection(project);
                projectResourceCollections[project] = collection;
            }

            return collection;
        }

        public IReadOnlyList<StaticResourceDefinition> GetStyleStaticResourceDefinitions()
        {
            var result = new List<StaticResourceDefinition>();

            foreach (var project in Projects)
            {
                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
                if (database is null || !database.IsValid)
                {
                    continue;
                }

                var repo = database.GetRepository<StaticResourceDefinitionRepository>();

                result.AddRange(repo.GetDefinedStyleResources());
            }

            return result;
        }

        public IReadOnlyList<StaticResourceDefinition> Find(Func<Project, StaticResourceDefinition, bool> predicate)
        {
            if (predicate is null)
            {
                return new List<StaticResourceDefinition>();
            }

            var result = new List<StaticResourceDefinition>();

            foreach (var colleciton in projectResourceCollections.Values)
            {
                result.AddRange(colleciton.StaticResourceDefinitions.Values.Where(d => predicate(colleciton.Project, d)));
            }

            return result;
        }

        public IEnumerator<StaticResourceDefinition> GetEnumerator()
        {
            if (!projectResourceCollections.Any())
            {
                yield break;
            }

            foreach (var collection in projectResourceCollections)
            {
                foreach (var definition in collection.Value.StaticResourceDefinitions)
                {
                    yield return definition.Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Project GetProjectFor(StaticResourceDefinition definition)
        {
            if (definition is null)
            {
                return null;
            }

            foreach (var collection in projectResourceCollections)
            {
                if (collection.Value.StaticResourceDefinitions.TryGetValue(definition.PrimaryKey, out _))
                {
                    return collection.Key;
                }
            }

            return null;
        }
    }
}