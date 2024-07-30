using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Configuration;
using MFractor.Maui.Configuration;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.XamlPlatforms;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDynamicResourceResolver))]
    class DynamicResourceResolver : IDynamicResourceResolver
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IAppXamlConfiguration> appXamlConfiguration;
        public IAppXamlConfiguration AppXamlConfiguration => appXamlConfiguration.Value;

        [ImportingConstructor]
        public DynamicResourceResolver(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                       Lazy<IAppXamlConfiguration> appXamlConfiguration)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.appXamlConfiguration = appXamlConfiguration;
        }

        public IReadOnlyList<DynamicResourceResult> GetAvailableDynamicResources(Project project,
                                                                                 IXamlPlatform platform,
                                                                                   string filePath)
        {
            try
            {
                var configId = ConfigurationId.Create(project.GetIdentifier());
                var result = new List<DynamicResourceResult>();

                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

                if (database == null || !database.IsValid)
                {
                    return result;
                }

                if (string.IsNullOrEmpty(filePath) || !Path.GetExtension(filePath).Equals(".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }

                var file = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(filePath);

                if (file == null || file.GCMarked)
                {
                    return result;
                }

                var classRepo = database.GetRepository<ClassDeclarationRepository>();

                var xClass = classRepo.GetClassForFile(file);

                if (xClass == null)
                {
                    return result;
                }

                if (!project.TryGetCompilation(out var compilation))
                {
                    return result;
                }

                var symbol = compilation.GetTypeByMetadataName(xClass.MetaDataName);

                if (symbol == null)
                {
                    return result;
                }

                return GetAvailableDynamicResources(project, platform, symbol);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return new List<DynamicResourceResult>();
        }

        public IReadOnlyList<DynamicResourceResult> GetAvailableDynamicResources(IXamlFeatureContext context)
        {
            if (context == null || context.XamlDocument == null)
            {
                return new List<DynamicResourceResult>();
            }

            return GetAvailableDynamicResources(context.Project, context.Platform, context.XamlDocument.CodeBehindSymbol);
        }

        public IReadOnlyList<DynamicResourceResult> GetAvailableDynamicResources(Project project,
                                                                                 IXamlPlatform platform,
                                                                                 INamedTypeSymbol namedType,
                                                                                 bool includeApplicationResources = true)
        {
            try
            {
                var result = new List<DynamicResourceResult>();

                if (project == null || namedType == null)
                {
                    return result;
                }

                if (!project.TryGetCompilation(out var compilation))
                {
                    return result;
                }

                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
                if (database == null || !database.IsValid)
                {
                    return result;
                }

                var configId = ConfigurationId.Create(project.GetIdentifier());
                AppXamlConfiguration.ApplyConfiguration(configId);
                var application = AppXamlConfiguration.ResolveAppXamlFile(project, platform);

                var classRepo = database.GetRepository<ClassDeclarationRepository>();
                var projectFileRepo = database.GetRepository<ProjectFileRepository>();
                var dynamicResourceRepository = database.GetRepository<DynamicResourceDefinitionRepository>();

                var applicationFile = projectFileRepo.GetProjectFileByFilePath(application.FilePath);

                if (applicationFile != null)
                {
                    var applicationClass = classRepo.GetClassForFile(applicationFile);

                    if (applicationClass != null)
                    {
                        var symbol = compilation.GetTypeByMetadataName(applicationClass.MetaDataName);

                        if (symbol != null && symbol.ToString() == namedType.ToString())
                        {
                            var appResources = GetAvailableDynamicResources(project, platform, symbol);

                            if (appResources.Any())
                            {
                                result.AddRange(appResources);
                            }
                        }
                    }
                }

                if (dynamicResourceRepository.HasDynamicResources(namedType))
                {
                    var dynamicResources = dynamicResourceRepository.GetDynamicResourcesDeclaredBySymbol(namedType);
                    if (dynamicResources != null && dynamicResources.Any())
                    {
                        result.AddRange(dynamicResources.Select(dr => new DynamicResourceResult(dr, project)));
                    }
                }

                var baseClass = namedType.BaseType;
                while (baseClass != null)
                {
                    var resources = dynamicResourceRepository.GetDynamicResourcesDeclaredBySymbol(baseClass);
                    if (resources != null && resources.Any())
                    {
                        result.AddRange(resources.Select(dr => new DynamicResourceResult(dr, project)));
                    }

                    baseClass = baseClass.BaseType;
                }

                return result;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return new List<DynamicResourceResult>();
        }

        public IReadOnlyList<DynamicResourceResult> GetAvailableDynamicResources(Project project)
        {
            try
            {
                var configId = ConfigurationId.Create(project.GetIdentifier());
                var result = new List<DynamicResourceResult>();

                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

                if (database == null || !database.IsValid)
                {
                    return result;
                }

                var repo = database.GetRepository<DynamicResourceDefinitionRepository>();

                var items = repo.GetAll().Where(d => !d.GCMarked);

                return items.Select(d => new DynamicResourceResult(d, project)).ToList();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return new List<DynamicResourceResult>();
        }

        public IReadOnlyList<DynamicResourceResult> FindNamedDynamicResources(Project project, string resourceName)
        {
            try
            {
                var configId = ConfigurationId.Create(project.GetIdentifier());
                var result = new List<DynamicResourceResult>();

                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

                if (database == null || !database.IsValid)
                {
                    return result;
                }

                var repo = database.GetRepository<DynamicResourceDefinitionRepository>();

                return repo.GetNamedDynamicResources(resourceName).Select(d => new DynamicResourceResult(d, project)).ToList();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return new List<DynamicResourceResult>();
        }

        public IReadOnlyList<DynamicResourceResult> FindAvailableNamedDynamicResources(Project project,
                                                                                       IXamlPlatform platform,
                                                                                       string filePath,
                                                                                       string resourceName)
        {
            try
            {
                var configId = ConfigurationId.Create(project.GetIdentifier());
                var result = new List<DynamicResourceResult>();

                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

                if (database == null || !database.IsValid)
                {
                    return result;
                }

                if (string.IsNullOrEmpty(filePath) || !Path.GetExtension(filePath).Equals(".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }

                var file = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(filePath);

                if (file == null || file.GCMarked)
                {
                    return result;
                }

                var classRepo = database.GetRepository<ClassDeclarationRepository>();

                var xClass = classRepo.GetClassForFile(file);

                if (xClass == null)
                {
                    return result;
                }

                if (!project.TryGetCompilation(out var compilation))
                {
                    return result;
                }

                var symbol = compilation.GetTypeByMetadataName(xClass.MetaDataName);

                if (symbol == null)
                {
                    return result;
                }

                return FindAvailableNamedDynamicResources(project, platform, symbol, resourceName);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return new List<DynamicResourceResult>();
        }

        public IReadOnlyList<DynamicResourceResult> FindAvailableNamedDynamicResources(IXamlFeatureContext context, string resourceName)
        {
            if (context == null || context.XamlDocument == null)
            {
                return null;
            }

            return FindAvailableNamedDynamicResources(context.Project, context.Platform, context.XamlDocument.CodeBehindSymbol,resourceName);
        }

        public IReadOnlyList<DynamicResourceResult> FindAvailableNamedDynamicResources(Project project,
                                                                                       IXamlPlatform platform,
                                                                                       INamedTypeSymbol namedType,
                                                                                       string resourceName)
        {
            try
            {
                var result = new List<DynamicResourceResult>();

                if (project == null || namedType == null)
                {
                    return result;
                }

                if (!project.TryGetCompilation(out var compilation))
                {
                    return result;
                }

                var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
                if (database == null || !database.IsValid)
                {
                    return result;
                }

                var configId = ConfigurationId.Create(project.GetIdentifier());
                AppXamlConfiguration.ApplyConfiguration(configId);

                var application = AppXamlConfiguration.ResolveAppXamlFile(project, platform);

                if (application == null)
                {
                    return result;
                }

                var classRepo = database.GetRepository<ClassDeclarationRepository>();
                var projectFileRepo = database.GetRepository<ProjectFileRepository>();
                var dynamicResourceRepository = database.GetRepository<DynamicResourceDefinitionRepository>();

                var applicationFile = projectFileRepo.GetProjectFileByFilePath(application.FilePath);

                if (applicationFile != null)
                {
                    var applicationClass = classRepo.GetClassForFile(applicationFile);

                    if (applicationClass != null)
                    {
                        var symbol = compilation.GetTypeByMetadataName(applicationClass.MetaDataName);

                        if (symbol != namedType)
                        {
                            var appResources = FindAvailableNamedDynamicResources(project, platform, symbol, resourceName);

                            if (appResources.Any())
                            {
                                result.AddRange(appResources);
                            }
                        }
                    }
                }

                if (dynamicResourceRepository.HasDynamicResources(namedType))
                {
                    var dynamicResources = dynamicResourceRepository.GetNamedDynamicResourcesDeclaredBySymbol(namedType, resourceName);
                    if (dynamicResources != null && dynamicResources.Any())
                    {
                        result.AddRange(dynamicResources.Select(dr => new DynamicResourceResult(dr, project)));
                    }
                }

                var baseClass = namedType.BaseType;
                while (baseClass != null)
                {
                    if (dynamicResourceRepository.HasDynamicResources(baseClass))
                    {
                        var resources = dynamicResourceRepository.GetNamedDynamicResourcesDeclaredBySymbol(baseClass, resourceName);
                        if (resources != null && resources.Any())
                        {
                            result.AddRange(resources.Select(dr => new DynamicResourceResult(dr, project)));
                        }
                    }

                    baseClass = baseClass.BaseType;
                }

                return result;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return new List<DynamicResourceResult>();
        }
    }
}
