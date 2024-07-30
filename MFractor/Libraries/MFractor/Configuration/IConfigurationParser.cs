using System.IO;

namespace MFractor.Configuration
{
    /// <summary>
    /// Parses a configuration file.
    /// </summary>
    public interface IConfigurationParser
    {
        /// <summary>
        /// Parse the specified id, parentId, filePath and scope.
        /// </summary>
        /// <returns>The parse.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="parentId">Parent identifier.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="scope">Scope.</param>
        IConfigurationSource Parse(ConfigurationId id, ConfigurationId parentId, string filePath, ConfigurationScope scope);

        /// <summary>
        /// Parse the specified id, parentId, filePath, content and scope.
        /// </summary>
        /// <returns>The parse.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="parentId">Parent identifier.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="content">Content.</param>
        /// <param name="scope">Scope.</param>
        IConfigurationSource Parse(ConfigurationId id, ConfigurationId parentId, string filePath, string content, ConfigurationScope scope);

        /// <summary>
        /// Parse the specified id, parentId, filePath, content and scope.
        /// </summary>
        /// <returns>The parse.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="parentId">Parent identifier.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="content">Content.</param>
        /// <param name="scope">Scope.</param>
        IConfigurationSource Parse(ConfigurationId id, ConfigurationId parentId, string filePath, Stream content, ConfigurationScope scope);
    }
}
