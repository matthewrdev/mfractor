using System.Collections.Generic;

namespace MFractor.Configuration
{
    public interface IConfigurationRepository
    {
        bool SaveConfiguration(string projectGuid, string projectName, string filePath, out string failureMessage);
        bool SaveConfiguration(ConfigurationId projectId, string filePath, out string failureMessage);

        bool SaveReferenceConfiguration(string projectGuid, string projectName, string filePath, string configName, out string failureMessage);
        bool SaveReferenceConfiguration(ConfigurationId projectId, string filePath, string configName, out string failureMessage);

        IEnumerable<IPropertySetting> GetConfiguration(string projectGuid, string projectName, string identifier);
        IEnumerable<IPropertySetting> GetConfiguration(ConfigurationId projectId, string identifier);

        IPropertySetting GetPropertyConfiguration(string propertyName, string projectGuid, string projectName, string identifier);
        IPropertySetting GetPropertyConfiguration(string propertyName, ConfigurationId projectId, string identifier);

        bool RemoveConfiguration(string projectGuid);
        bool RemoveConfiguration(string projectGuid, string projectName);
        bool RemoveConfiguration(ConfigurationId projectId);
        bool RemoveConfiguration(string projectGuid, string projectName, string configname);
        bool RemoveConfiguration(ConfigurationId projectId, string filePath);

        bool RemoveReferenceConfiguration(string projectGuid, string projectName, string configName);
        bool RemoveReferenceConfiguration(ConfigurationId projectId, string configName);

        void Clear();
        void Clear(string projectGuid, string projectName);
        void ClearProjectPackages(string projectGuid, string projectName);
    }
}
