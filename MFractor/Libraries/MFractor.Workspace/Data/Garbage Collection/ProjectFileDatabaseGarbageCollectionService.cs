using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Data;
using MFractor.Data.GarbageCollection;
using MFractor.Data.Schemas;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data.GarbageCollection
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProjectFileDatabaseGarbageCollectionService))]
    class ProjectFileDatabaseGarbageCollectionService : IProjectFileDatabaseGarbageCollectionService
    {
        readonly Lazy<IReadOnlyList<Type>> fileOwnedEntities;
        IReadOnlyList<Type> FileOwnedEntities => fileOwnedEntities.Value;

        readonly Lazy<ISchemaRepository> schemaRepository;
        public ISchemaRepository SchemaRepository => schemaRepository.Value;

        [ImportingConstructor]
        public ProjectFileDatabaseGarbageCollectionService(Lazy<ISchemaRepository> schemaRepository)
        {
            this.schemaRepository = schemaRepository;

            fileOwnedEntities = new Lazy<IReadOnlyList<Type>>(() =>
            {
                var types = SchemaRepository.Schemas.SelectMany(s => s.Entities).Distinct();

                var projectFileOwnedEntityType = typeof(ProjectFileOwnedEntity);

                var projectFileEntityTypes = types.Where(projectFileOwnedEntityType.IsAssignableFrom).ToList();

                return projectFileEntityTypes;
            });
        }

        public void Mark(IDatabase database, ProjectFile file, MarkOperation markOperation)
        {
            if (file == null)
            {
                return;
            }

            if (markOperation == MarkOperation.FileOnly || markOperation == MarkOperation.FileAndChildren)
            {
                file.GCMarked = true;
                var repo = database.GetRepositoryForEntity<ProjectFile>();
                if (repo != null)
                {
                    repo.Update(file);
                }
            }

            if (markOperation == MarkOperation.ChildrenOnly || markOperation == MarkOperation.FileAndChildren)
            {
                UpdateProjectFileChildren(database, file, gcMarked: true);
            }
        }

        public void UnMark(IDatabase database, ProjectFile file, MarkOperation markOperation)
        {
            if (file == null)
            {
                return;
            }

            if (markOperation == MarkOperation.FileOnly || markOperation == MarkOperation.FileAndChildren)
            {
                file.GCMarked = false;
                var repo = database.GetRepositoryForEntity<ProjectFile>();
                if (repo != null)
                {
                    repo.Update(file);
                }
            }

            if (markOperation == MarkOperation.ChildrenOnly || markOperation == MarkOperation.FileAndChildren)
            {
                UpdateProjectFileChildren(database, file, gcMarked:false);
            }
        }

        void UpdateProjectFileChildren(IDatabase database, ProjectFile file, bool gcMarked)
        {
            foreach (var entityType in FileOwnedEntities)
            {
                var repo = database.GetRepositoryForEntityType(entityType);
                if (repo != null)
                {
                    foreach (var item in repo.GetAllEntities())
                    {
                        if (item is ProjectFileOwnedEntity fileOwnedEntity && fileOwnedEntity.ProjectFileKey == file.PrimaryKey)
                        {
                            fileOwnedEntity.GCMarked = gcMarked;
                        }
                    }
                }
            }
        }
    }
}