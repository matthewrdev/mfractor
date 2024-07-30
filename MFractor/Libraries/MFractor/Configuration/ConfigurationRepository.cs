using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Configuration
{
    [Export(typeof(IConfigurationRepository))]
    sealed class ConfigurationRepository : IConfigurationRepository
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        class ProjectConfigurationCollection
        {
            public ConfigurationId Id { get; }

            public Dictionary<ConfigurationId, IConfigurationSource> Configurations { get; } = new Dictionary<ConfigurationId, IConfigurationSource>();

            public ProjectConfigurationCollection(ConfigurationId id)
            {
                Id = id;
            }

            public void Save(IConfigurationSource config)
            {
                if (config != null)
                {
                    Configurations[config.Id] = config;
                }
            }

            public void Remove(ConfigurationId id)
            {
                if (Configurations.ContainsKey(id))
                {
                    Configurations.Remove(id);
                }
            }

            internal IEnumerable<IPropertySetting> GetSettings(ConfigurationId id, string identifier)
            {
                if (!Configurations.ContainsKey(id))
                {
                    return Enumerable.Empty<IPropertySetting>();
                }

                var source = Configurations[id];

                if (!source.Settings.ContainsKey(identifier))
                {
                    return Enumerable.Empty<IPropertySetting>();
                }

                return source.Settings[identifier];
            }
        }

        readonly Dictionary<ConfigurationId, ProjectConfigurationCollection> configurations = new Dictionary<ConfigurationId, ProjectConfigurationCollection>();

        readonly IConfigurationParser configurationParser;

        [ImportingConstructor]
        public ConfigurationRepository(IConfigurationParser configurationParser)
        {
            this.configurationParser = configurationParser;
        }

        public void Clear()
        {
            configurations.Clear();
        }

        public void Clear(string projectGuid, string projectName)
        {
            var id = ConfigurationId.Create(projectGuid, projectName);
            if (configurations.ContainsKey(id))
            {
                configurations.Remove(id);
            }
        }

        public void ClearProjectPackages(string projectGuid, string projectName)
        {
            var id = ConfigurationId.Create(projectGuid, projectName);
            if (configurations.ContainsKey(id))
            {
                var packageIds = configurations[id].Configurations.Where(c => c.Value.Scope == ConfigurationScope.Package).Select(c => c.Key);
                foreach (var packageId in packageIds)
                {
                    configurations[id].Remove(packageId);
                }
            }
        }

        public bool SaveConfiguration(string projectGuid, string projectName, string filePath, out string failureMessage)
        {
            var id = ConfigurationId.Create(projectGuid, projectName);

            return SaveConfiguration(id, filePath, out failureMessage);
        }

        public bool SaveConfiguration(ConfigurationId configId, string filePath, out string failureMessage)
        {
            var id = ConfigurationId.Create(configId, filePath);

            return SaveConfiguration(filePath, id, configId, ConfigurationScope.Folder, out failureMessage);
        }

        public bool SaveReferenceConfiguration(string projectGuid, string projectName, string filePath, string configName, out string failureMessage)
        {
            var id = ConfigurationId.Create(projectGuid, projectName);

            return SaveReferenceConfiguration(id, filePath, configName, out failureMessage);
        }

        public bool SaveReferenceConfiguration(ConfigurationId configId, string filePath, string configName, out string failureMessage)
        {
            var id = ConfigurationId.Create(configId, configName + "-reference");

            return SaveConfiguration(filePath, id, configId, ConfigurationScope.Package, out failureMessage);
        }

        public bool SaveConfiguration(string filePath,
                                      ConfigurationId id,
                                      ConfigurationId parentId,
                                      ConfigurationScope scope,
                                      out string failureMessage)
        {
            failureMessage = "";
            if (!File.Exists(filePath))
            {
                failureMessage = "The configuration file " + filePath + " does not exist.";
                return false;
            }

            var config = configurationParser.Parse(id, parentId, filePath, scope);

            if (config != null)
            {
                if (!configurations.ContainsKey(parentId))
                {
                    configurations[parentId] = new ProjectConfigurationCollection(parentId);
                }

                configurations[parentId].Save(config);
            }
            else
            {
                failureMessage = "MFractor could not parse " + Path.GetFileName(filePath) + "; the configuration wasn't applied to " + id.Name;
                return false;
            }

            return true;
        }

        public IEnumerable<IPropertySetting> GetConfiguration(string projectGuid, string projectName, string identifier)
        {
            var id = ConfigurationId.Create(projectGuid, projectName);

            return GetConfiguration(id, identifier);
        }

        public IEnumerable<IPropertySetting> GetConfiguration(ConfigurationId configId, string identifier)
        {
            if (configId == null || string.IsNullOrEmpty(configId.Id))
            {
                return Enumerable.Empty<IPropertySetting>();
            }

            var settings = new List<IPropertySetting>();

            if (!configurations.ContainsKey(configId))
            {
                return Enumerable.Empty<IPropertySetting>();
            }

            var projectConfiguration = configurations[configId];

            foreach (var configuration in projectConfiguration.Configurations.Values)
            {
                if (configuration.Settings != null
                    && configuration.Settings.Any()
                    && configuration.Settings.ContainsKey(identifier))
                {
                    foreach (var s in configuration.Settings[identifier])
                    {
                        var conflict = settings.FirstOrDefault(setting => setting.Name == s.Name);

                        if (conflict != null)
                        {
                            settings.Remove(conflict);
                        }

                        settings.Add(s);
                    }
                }
            }

            return settings;
        }

        public bool RemoveConfiguration(string projectGuid, string projectName, string filePath)
        {
            var projectId = ConfigurationId.Create(projectGuid, projectName);

            return RemoveConfiguration(projectId, filePath);
        }

        public bool RemoveConfiguration(ConfigurationId configId, string filePath)
        {
            var fileId = ConfigurationId.Create(configId, filePath);

            if (configurations.ContainsKey(configId))
            {
                configurations[configId].Remove(fileId);
            }

            return true;
        }

        public bool RemoveReferenceConfiguration(string projectGuid, string projectName, string configName)
        {
            var projectId = ConfigurationId.Create(projectGuid, projectName);

            return RemoveReferenceConfiguration(projectId, configName);
        }

        public bool RemoveReferenceConfiguration(ConfigurationId configId, string configName)
        {
            var fileId = ConfigurationId.Create(configId, configName + "-reference");

            if (configurations.ContainsKey(configId))
            {
                configurations[configId].Remove(fileId);
            }

            return true;
        }

        public bool RemoveConfiguration(string projectGuid)
        {
            var projectId = ConfigurationId.Create(projectGuid, string.Empty);

            return RemoveConfiguration(projectId);
        }

        public bool RemoveConfiguration(string projectGuid, string projectName)
        {
            var projectId = ConfigurationId.Create(projectGuid, projectName);

            return RemoveConfiguration(projectId);
        }

        public bool RemoveConfiguration(ConfigurationId projectId)
        {
            if (configurations.ContainsKey(projectId))
            {
                configurations.Remove(projectId);
            }

            return true;
        }

        public IPropertySetting GetPropertyConfiguration(string propertyName, string projectGuid, string projectName, string identifier)
        {
            var projectId = ConfigurationId.Create(projectGuid, projectName);

            return GetPropertyConfiguration(projectName, projectId, identifier);
        }

        public IPropertySetting GetPropertyConfiguration(string propertyName, ConfigurationId projectId, string identifier)
        {
            var props = GetConfiguration(projectId, identifier);

            if (!props.Any())
            {
                return null;
            }

            return props.FirstOrDefault(p => p.Name == propertyName);
        }
    }
}
