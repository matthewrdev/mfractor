using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Concurrency;
using MFractor.Data.GarbageCollection;
using MFractor.Data.Repositories;
using MFractor.Data.Schemas;
using MFractor.Workspace.Data.Synchronisation;
using MFractor.Text;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using MFractor.Workspace.Data.GarbageCollection;
using MFractor.Workspace.Data.Repositories;

namespace MFractor.Workspace.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IResourcesDatabaseEngine))]
    class ResourcesDatabaseEngine : IResourcesDatabaseEngine, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDialogsService> dialogsService;
        IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IDatabaseGarbageCollector> databaseGarbageCollector;
        IDatabaseGarbageCollector DatabaseGarbageCollector => databaseGarbageCollector.Value;

        readonly Lazy<IProjectFileDatabaseGarbageCollectionService> projectFileDatabaseGarbageCollectionService;
        public IProjectFileDatabaseGarbageCollectionService ProjectFileDatabaseGarbageCollectionService => projectFileDatabaseGarbageCollectionService.Value;

        readonly Lazy<IWorkspaceService> workspaceService;
        IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IProjectService> projectService;
        IProjectService ProjectService => projectService.Value;

        readonly Lazy<IAssetResourceSynchroniserRepository> assetResourceSynchroniserRepository;
        IAssetResourceSynchroniserRepository AssetResourceSynchroniserRepository => assetResourceSynchroniserRepository.Value;

        readonly Lazy<ITextResourceSynchroniserRepository> textResourceSynchroniserRepository;
        ITextResourceSynchroniserRepository TextResourceSynchroniserRepository => textResourceSynchroniserRepository.Value;

        readonly Lazy<IGarbageCollectionEventsRepository> garbageCollectionEventsRepository;
        IGarbageCollectionEventsRepository GarbageCollectionEventsRepository => garbageCollectionEventsRepository.Value;

        readonly Lazy<IRepositoryCollectionRepository> repositoryCollectionRepository;
        IRepositoryCollectionRepository RepositoryCollectionRepository => repositoryCollectionRepository.Value;

        readonly Lazy<ISchemaRepository> schemaRepository;
        ISchemaRepository SchemaRepository => schemaRepository.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        ITextProviderService TextProviderService => textProviderService.Value;

        [ImportingConstructor]
        public ResourcesDatabaseEngine(Lazy<IWorkspaceService> workspaceService,
                                       Lazy<IProjectService> projectService,
                                       Lazy<IDialogsService> dialogsService,
                                       Lazy<IDatabaseGarbageCollector> databaseGarbageCollector,
                                       Lazy<IProjectFileDatabaseGarbageCollectionService> projectFileDatabaseGarbageCollectionService,
                                       Lazy<IAssetResourceSynchroniserRepository> assetResourceSynchroniserRepository,
                                       Lazy<ITextResourceSynchroniserRepository> textResourceSynchroniserRepository,
                                       Lazy<ISchemaRepository> schemaRepository,
                                       Lazy<IGarbageCollectionEventsRepository> garbageCollectionEventsRepository,
                                       Lazy<IRepositoryCollectionRepository> repositoryCollectionRepository,
                                       Lazy<ITextProviderService> textProviderService)
        {
            this.workspaceService = workspaceService;
            this.projectService = projectService;
            this.dialogsService = dialogsService;
            this.databaseGarbageCollector = databaseGarbageCollector;
            this.projectFileDatabaseGarbageCollectionService = projectFileDatabaseGarbageCollectionService;
            this.assetResourceSynchroniserRepository = assetResourceSynchroniserRepository;
            this.textResourceSynchroniserRepository = textResourceSynchroniserRepository;
            this.schemaRepository = schemaRepository;
            this.garbageCollectionEventsRepository = garbageCollectionEventsRepository;
            this.repositoryCollectionRepository = repositoryCollectionRepository;
            this.textProviderService = textProviderService;
        }

        readonly Dictionary<string, List<IAssetResourceSynchroniser>> extensionGroupedAssetSynchronisers = new Dictionary<string, List<IAssetResourceSynchroniser>>();

        readonly Dictionary<string, List<ITextResourceSynchroniser>> extensionGroupedTextSynchronisers = new Dictionary<string, List<ITextResourceSynchroniser>>();

        readonly TaskedBackgroundWorkerQueue workerQueue = new TaskedBackgroundWorkerQueue();

        void SetupSynchronisers()
        {
            SetupAssetResourceSynchronisers();

            SetupTextResourceSynchronisers();
        }

        void SetupAssetResourceSynchronisers()
        {
            var assetSynchronisers = AssetResourceSynchroniserRepository.Synchronisers;

            foreach (var synchroniser in assetSynchronisers)
            {
                var extensions = synchroniser.SupportedFileExtensions;

                if (extensions == null || !extensions.Any())
                {
                    log?.Warning("The asset synchroniser " + synchroniser.GetType() + " returned no targeted file extensions. This synchroniser will not run during resource sync passes");
                    continue;
                }

                log?.Info(" - Detected asset synchroniser " + synchroniser.GetType() + " targeting: " + string.Join(", ", extensions));
                try
                {
                    foreach (var supportedExtension in extensions)
                    {
                        var extension = supportedExtension?.ToLower();
                        if (string.IsNullOrEmpty(extension))
                        {
                            log?.Info(synchroniser.GetType() + " is trying to register a null or empty file extension. Ignoring.");
                            continue;
                        }

                        if (!extensionGroupedAssetSynchronisers.ContainsKey(extension))
                        {
                            extensionGroupedAssetSynchronisers.Add(extension, new List<IAssetResourceSynchroniser>());
                        }

                        extensionGroupedAssetSynchronisers[extension].Add(synchroniser);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        void SetupTextResourceSynchronisers()
        {
            var textResourceSynchronisers = TextResourceSynchroniserRepository.Synchronisers;

            foreach (var synchroniser in textResourceSynchronisers)
            {
                log?.Info(" - Detected text resource synchroniser " + synchroniser.GetType() + " targeting: " + string.Join(", ", synchroniser.SupportedFileExtensions));
                try
                {
                    foreach (var supportedExtension in synchroniser.SupportedFileExtensions)
                    {
                        var extension = supportedExtension?.ToLower();
                        if (string.IsNullOrEmpty(extension))
                        {
                            log?.Info(synchroniser.GetType() + " is trying to register a null or empty file extension. Ignoring.");
                            continue;
                        }

                        if (!extensionGroupedTextSynchronisers.ContainsKey(extension))
                        {
                            extensionGroupedTextSynchronisers.Add(extension, new List<ITextResourceSynchroniser>());
                        }

                        if (extensionGroupedTextSynchronisers[extension].Contains(synchroniser))
                        {
                            log?.Warning($"The synchroniser {synchroniser.GetType().Name} targets the extension {supportedExtension} multiple times. The synchroniser will only be added once per file extension.");
                            continue;
                        }

                        extensionGroupedTextSynchronisers[extension].Add(synchroniser);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        readonly object databaseMapLock = new object();
        readonly Dictionary<string, ProjectResourcesDatabase> databases = new Dictionary<string, ProjectResourcesDatabase>();

        public event EventHandler<SolutionSynchronisationStatusEventArgs> SolutionSyncStarted;
        public event EventHandler<SolutionSynchronisationStatusEventArgs> SolutionSyncEnded;
        public event EventHandler<ProjectSynchronisationPassCompletedEventArgs> ProjectSynchronisationPassCompleted;

        readonly Lazy<EventInfo> projectSynchronisationPassCompletedEventInfo = new Lazy<EventInfo>(() => typeof(ResourcesDatabaseEngine).GetEvent(nameof(ProjectSynchronisationPassCompleted)));
        EventInfo ProjectSynchronisationPassCompletedEventInfo => projectSynchronisationPassCompletedEventInfo.Value;

        void WorkspaceService_FilesAddedToProject(object sender, FilesEventArgs args)
        {
            workerQueue.QueueTask(async () =>
            {
                foreach (var guid in args.ProjectGuids)
                {
                    var project = ProjectService.GetProject(guid);

                    if (project == null)
                    {
                        return;
                    }


                    if (GetProjectResourcesDatabase(guid) is ProjectResourcesDatabase database)
                    {
                        try
                        {
                            database.IsValid = false;
                            foreach (var filePath in args.GetProjectFiles(guid))
                            {
                                database.GetRepository<ProjectFileRepository>().CreateFile(filePath);

                                var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);
                                try
                                {
                                    await SynchroniseProjectFile(project, projectFile, database);
                                }
                                catch (Exception ex)
                                {
                                    log?.Exception(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log?.Exception(ex);
                        }
                        finally
                        {
                            if (database != null)
                            {
                                database.IsValid = true;
                            }
                        }

                        GarbageCollect(database);
                        this.NotifyProjectSyncronisationPassCompleted(guid);
                    }
                }
            });
        }


        readonly object fileChangeCancellationLock = new object();
        readonly Dictionary<string, CancellationTokenSource> fileChangeCancellationContexts = new Dictionary<string, CancellationTokenSource>();

        void WorkspaceService_FilesChanged(object sender, FilesEventArgs args)
        {
            workerQueue.QueueTask(async () =>
            {
                foreach (var guid in args.ProjectGuids)
                {
                    var token = CreateCancellationContext(guid);

                    try
                    {
                        var project = ProjectService.GetProject(guid);

                        if (project == null)
                        {
                            return;
                        }

                        if (GetProjectResourcesDatabase(guid) is ProjectResourcesDatabase database)
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }

                            try
                            {
                                database.IsValid = false;
                                foreach (var filePath in args.GetProjectFiles(guid))
                                {
                                    var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);

                                    if (projectFile == null)
                                    {
                                        continue;
                                    }

                                    var fileModel = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(projectFile.FilePath);
                                    try
                                    {
                                        ProjectFileDatabaseGarbageCollectionService.Mark(database, fileModel, MarkOperation.ChildrenOnly);

                                        await SynchroniseProjectFile(project, projectFile, database);
                                    }
                                    catch (Exception ex)
                                    {
                                        log?.Exception(ex);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log?.Exception(ex);
                            }
                            finally
                            {
                                database.IsValid = true;
                            }

                            GarbageCollect(database);
                            this.NotifyProjectSyncronisationPassCompleted(guid);
                        }
                    }
                    finally
                    {
                        RemoveCancellationContext(guid);
                    }
                }
            });
        }

        void NotifyProjectSyncronisationPassCompleted(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return;
            }

            try
            {
                var invocations = ProjectSynchronisationPassCompleted?.GetInvocationList();

                if (invocations == null)
                {
                    return;
                }

                var eventArgs = new ProjectSynchronisationPassCompletedEventArgs(projectGuid);
                foreach (var handler in invocations)
                {
                    if (handler == null)
                    {
                        continue;
                    }

                    try
                    {
                        handler.DynamicInvoke(this, eventArgs);
                    }
                    catch (Exception ex)
                    {
                        log?.Warning("The callback delegate for " + handler.Method.Name + " from " + handler.Target.ToString() + " threw " + ex);
                        log?.Warning("MFractor has zero tolerance for synchronisation pass delegates that throw exceptions, please ensure your delegate handles all exceptions. Automatically removing this delegate.");

                        try
                        {
                            ProjectSynchronisationPassCompletedEventInfo.RemoveEventHandler(this, handler);
                        }
                        catch (Exception ex2)
                        {
                            log?.Exception(ex2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        CancellationToken CreateCancellationContext(string projectGuid)
        {
            lock (fileChangeCancellationLock)
            {
                if (fileChangeCancellationContexts.TryGetValue(projectGuid, out var source))
                {
                    source.Cancel();
                }

                fileChangeCancellationContexts[projectGuid] = new CancellationTokenSource();

                return fileChangeCancellationContexts[projectGuid].Token;
            }
        }

        void RemoveCancellationContext(string projectGuid)
        {
            lock (fileChangeCancellationLock)
            {
                if (fileChangeCancellationContexts.TryGetValue(projectGuid, out var source))
                {
                    source.Cancel();
                    fileChangeCancellationContexts.Remove(projectGuid);
                }
            }
        }

        void GarbageCollect(IProjectResourcesDatabase database)
        {
            if (database is null)
            {
                return;
            }

            foreach (var garbageCollectionEvent in GarbageCollectionEventsRepository.GarbageCollectionEvents)
            {
                try
                {
                    garbageCollectionEvent.OnGarbageCollectionStarted(database);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            DatabaseGarbageCollector.GarbageCollect(database);

            foreach (var garbageCollectionEvent in GarbageCollectionEventsRepository.GarbageCollectionEvents)
            {
                try
                {
                    garbageCollectionEvent.OnGarbageCollectionEnded(database);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        void WorkspaceService_FilesRemovedFromProject(object sender, FilesEventArgs args)
        {
            workerQueue.QueueTask(() =>
            {
                foreach (var guid in args.ProjectGuids)
                {
                    var project = ProjectService.GetProject(guid);

                    if (project == null)
                    {
                        return;
                    }

                    var database = GetProjectResourcesDatabase(guid) as ProjectResourcesDatabase;

                    if (database != null)
                    {
                        try
                        {
                            database.IsValid = false;
                            foreach (var filePath in args.GetProjectFiles(guid))
                            {
                                var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);

                                if (projectFile is null)
                                {
                                    continue;
                                }

                                var fileModel = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(projectFile.FilePath);
                                try
                                {
                                    ProjectFileDatabaseGarbageCollectionService.Mark(database, fileModel, MarkOperation.FileAndChildren);
                                }
                                catch (Exception ex)
                                {
                                    log?.Exception(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log?.Exception(ex);
                        }
                        finally
                        {
                            database.IsValid = true;
                        }

                        GarbageCollect(database);
                        NotifyProjectSyncronisationPassCompleted(guid);
                    }
                }
            });
        }

        void WorkspaceService_FilesRenamed(object sender, FilesRenamedEventArgs args)
        {
            workerQueue.QueueTask(() =>
            {
                foreach (var guid in args.ChangeSet.ProjectGuids)
                {
                    var database = GetProjectResourcesDatabase(guid);

                    if (database != null)
                    {
                        var projectFileRepo = database.GetRepository<ProjectFileRepository>();
                        foreach (var change in args.GetProjectFiles(guid))
                        {
                            var file = projectFileRepo.GetProjectFileByFilePath(change.OldFilePath);

                            if (file != null)
                            {
                                file.FilePath = change.NewFilePath;
                                file.FileName = Path.GetFileName(change.NewFilePath);
                                projectFileRepo.Update(file);
                            }
                        }
                    }

                    NotifyProjectSyncronisationPassCompleted(guid);
                }
            });
        }

        void WorkspaceService_ProjectAdded(object sender, ProjectAddedEventArgs e)
        {
            workerQueue.QueueTask(async () =>
            {
                try
                {
                    var solution = WorkspaceService.GetSolution(e.SolutionName);
                    var project = solution?.Projects?.FirstOrDefault(p => ProjectService.GetProjectGuid(p) == e.ProjectGuid);

                    if (solution == null || project == null)
                    {
                        return;
                    }

                    await SynchroniseNewProject(project);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            });
        }

        void WorkspaceService_ProjectRemoved(object sender, ProjectRemovedEventArgs e)
        {
            workerQueue.QueueTask(() =>
            {
                lock (databaseMapLock)
                {
                    var removableKeys = databases.Where(kp => kp.Value.SolutionName == e.SolutionName).Select(kp => kp.Key).ToList();

                    foreach (var key in removableKeys)
                    {
                        databases.Remove(key);
                    }
                }
            });
        }

        void WorkspaceService_SolutionClosed(object sender, SolutionClosedEventArgs e)
        {
            workerQueue.QueueTask(() =>
            {
                lock (databaseMapLock)
                {
                    var removableKeys = databases.Where(kp => kp.Value.SolutionName == e.SolutionName).Select(kp => kp.Key).ToList();

                    foreach (var key in removableKeys)
                    {
                        databases.Remove(key);
                    }
                }
            });
        }

        void WorkspaceService_SolutionOpened(object sender, SolutionOpenedEventArgs e)
        {
            var solution = WorkspaceService.GetSolution(e.SolutionName);

            SynchroniseSolutionResources(solution);
        }

        public void SynchroniseSolutionResources(Solution solution)
        {
            workerQueue.QueueTask(async () =>
            {
                if (solution == null)
                {
                    return;
                }

               var solutionName = Path.GetFileName(solution.FilePath);
               try
               {
                   SolutionSyncStarted?.Invoke(this, new SolutionSynchronisationStatusEventArgs(solutionName));
               }
               catch
               {
               }

               DialogsService.StatusBarMessage("MFractor - Solution resources sync for " + solutionName + " has started");

               try
               {
                   foreach (var project in solution.Projects.Where(p => p.SupportsCompilation))
                   {
                       try
                       {
                           await SynchroniseNewProjectAsync(project);
                       }
                       catch (Exception ex)
                       {
                           log?.Exception(ex);
                       }
                   }
               }
               catch (Exception ex)
               {
                   log?.Exception(ex);
               }

               try
               {
                   SolutionSyncEnded?.Invoke(this, new SolutionSynchronisationStatusEventArgs(solutionName));
               }
               catch
               {
               }

               DialogsService.StatusBarMessage("MFractor - Solution resources sync for " + solutionName + " is complete");
           });
        }

        async Task SynchroniseNewProject(Project project)
        {
            var database = GetOrCreateDatabase(project);

            database.IsValid = false;

            try
            {
                var files = database.GetRepository<ProjectFileRepository>().GetAll();
                foreach (var file in files)
                {
                    ProjectFileDatabaseGarbageCollectionService.Mark(database, file, MarkOperation.FileAndChildren);
                }

                var projectFiles = ProjectService.GetProjectFiles(project);

                foreach (var projectFile in projectFiles)
                {
                    var file = database.GetRepository<ProjectFileRepository>().CreateFile(projectFile.FilePath);

                    if (file != null)
                    {
                        ProjectFileDatabaseGarbageCollectionService.Mark(database, file, MarkOperation.ChildrenOnly);
                    }

                    var textProvider = new CachedFileSystemTextProvider(projectFile.FilePath);
                    try
                    {
                        await SynchroniseProjectFile(project, projectFile, textProvider, database);
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }

                GarbageCollect(database);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                database.IsValid = true;
            }
        }

        async Task SynchroniseNewProjectAsync(Project project)
        {
            var database = GetOrCreateDatabase(project);

            database.IsValid = false;

            try
            {
                var files = database.GetRepository<ProjectFileRepository>().GetAll();
                foreach (var file in files)
                {
                    ProjectFileDatabaseGarbageCollectionService.Mark(database, file, MarkOperation.FileAndChildren);
                }

                var projectFiles = ProjectService.GetProjectFiles(project);

                foreach (var projectFile in projectFiles)
                {
                    var file = database.GetRepository<ProjectFileRepository>().CreateFile(projectFile.FilePath);

                    if (file != null)
                    {
                        ProjectFileDatabaseGarbageCollectionService.Mark(database, file, MarkOperation.ChildrenOnly);
                    }

                    var textProvider = new CachedFileSystemTextProvider(projectFile.FilePath);
                    try
                    {
                        await SynchroniseProjectFile(project, projectFile, textProvider, database);
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }

                GarbageCollect(database);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                database.IsValid = true;
            }
        }

        async Task SynchroniseProjectFile(Project project, IProjectFile projectFile, IProjectResourcesDatabase database)
        {
            var textProvider = TextProviderService.GetTextProvider(projectFile.FilePath);

            await SynchroniseProjectFile(project, projectFile, textProvider, database);
        }


        async Task SynchroniseProjectFile(Project project, IProjectFile projectFile, ITextProvider textProvider, IProjectResourcesDatabase database)
        {
            var extension = Path.GetExtension(projectFile.FilePath);

            await TrySynchroniseAssetResource(project, projectFile, database, extension);

            await TrySynchroniseTextResource(project, projectFile, textProvider, database, extension);
        }

        async Task TrySynchroniseTextResource(Project project, IProjectFile projectFile, ITextProvider textProvider, IProjectResourcesDatabase database, string extension)
        {
            var availableSynchronisers = GetTextResourceSynchronisers(extension.ToLower());

            var synchronisers = new List<ITextResourceSynchroniser>();

            foreach (var synchroniser in availableSynchronisers)
            {
                try
                {
                    var canSync = await synchroniser.CanSynchronise(project.Solution, project, projectFile);

                    if (canSync)
                    {
                        synchronisers.Add(synchroniser);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            if (!synchronisers.Any())
            {
                return;
            }

            foreach (var synchroniser in synchronisers)
            {
                try
                {
                    await synchroniser.Synchronise(project.Solution, project, projectFile, textProvider, database);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        async Task TrySynchroniseAssetResource(Project project, IProjectFile projectFile, IProjectResourcesDatabase database, string extension)
        {
            var availableSynchronisers = GetAssetResourceSynchronisers(extension.ToLower());

            var synchronisers = new List<IAssetResourceSynchroniser>();

            foreach (var synchroniser in availableSynchronisers)
            {
                try
                {
                    var canSync = await synchroniser.CanSynchronise(project.Solution, project, projectFile);

                    if (canSync)
                    {
                        synchronisers.Add(synchroniser);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }


            if (!synchronisers.Any())
            {
                return;
            }

            foreach (var synchroniser in synchronisers)
            {
                try
                {
                    await synchroniser.Synchronise(project.Solution, project, projectFile, database);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        IEnumerable<ITextResourceSynchroniser> GetTextResourceSynchronisers(string extension)
        {
            if (!extensionGroupedTextSynchronisers.ContainsKey(extension))
            {
                return Enumerable.Empty<ITextResourceSynchroniser>();
            }

            return extensionGroupedTextSynchronisers[extension] ?? Enumerable.Empty<ITextResourceSynchroniser>();
        }

        IEnumerable<IAssetResourceSynchroniser> GetAssetResourceSynchronisers(string extension)
        {
            if (!extensionGroupedAssetSynchronisers.ContainsKey(extension))
            {
                return Enumerable.Empty<IAssetResourceSynchroniser>();
            }

            return extensionGroupedAssetSynchronisers[extension] ?? Enumerable.Empty<IAssetResourceSynchroniser>();
        }

        internal ProjectResourcesDatabase GetOrCreateDatabase(Project project)
        {
            if (project == null)
            {
                return null;
            }

            var guid = ProjectService.GetProjectGuid(project);

            ProjectResourcesDatabase db = null;
            lock (databaseMapLock)
            {
                try
                {
                    if (!databases.ContainsKey(guid))
                    {
                        var solutionName = Path.GetFileName(project.Solution.FilePath);

                        var databasePath = WorkingPathHelper.GetMFractorWorkingPath(project.Solution);

                        db = new ProjectResourcesDatabase(solutionName,
                                                          guid,
                                                          SchemaRepository.Schemas,
                                                          RepositoryCollectionRepository.RepositoryCollections);

                        databases[guid] = db;
                    }
                    db = databases[guid];
                }
                catch (Exception ex)
                {
                    log.Exception(ex);
                }
            }

            return db;
        }

        public IProjectResourcesDatabase GetProjectResourcesDatabase(Project project)
        {
            if (project is null)
            {
                return null;
            }

            var guid = ProjectService.GetProjectGuid(project);

            return GetProjectResourcesDatabase(guid);
        }

        public IProjectResourcesDatabase GetProjectResourcesDatabase(ProjectIdentifier projectIdentifier)
        {
            return GetProjectResourcesDatabase(projectIdentifier?.Guid);
        }

        public IProjectResourcesDatabase GetProjectResourcesDatabase(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            ProjectResourcesDatabase db = null;

            if (Monitor.TryEnter(databaseMapLock, 1))
            {
                if (databases.ContainsKey(guid))
                {
                    db = databases[guid];
                }
                Monitor.Exit(databaseMapLock);
            }

            return db;
        }

        public void Startup()
        {
            WorkspaceService.FilesAddedToProject += WorkspaceService_FilesAddedToProject;
            WorkspaceService.FilesChanged += WorkspaceService_FilesChanged;
            WorkspaceService.FilesRemovedFromProject += WorkspaceService_FilesRemovedFromProject;
            WorkspaceService.FilesRenamed += WorkspaceService_FilesRenamed;
            WorkspaceService.ProjectAdded += WorkspaceService_ProjectAdded;
            WorkspaceService.ProjectRemoved += WorkspaceService_ProjectRemoved;
            WorkspaceService.SolutionClosed += WorkspaceService_SolutionClosed;
            WorkspaceService.SolutionOpened += WorkspaceService_SolutionOpened;

            SetupSynchronisers();
        }

        public void Shutdown()
        {
            WorkspaceService.FilesAddedToProject -= WorkspaceService_FilesAddedToProject;
            WorkspaceService.FilesChanged -= WorkspaceService_FilesChanged;
            WorkspaceService.FilesRemovedFromProject -= WorkspaceService_FilesRemovedFromProject;
            WorkspaceService.FilesRenamed -= WorkspaceService_FilesRenamed;
            WorkspaceService.ProjectAdded -= WorkspaceService_ProjectAdded;
            WorkspaceService.ProjectRemoved -= WorkspaceService_ProjectRemoved;
            WorkspaceService.SolutionClosed -= WorkspaceService_SolutionClosed;
            WorkspaceService.SolutionOpened -= WorkspaceService_SolutionOpened;
        }
    }
}