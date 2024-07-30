using System;
using System.IO;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Configuration
{
    /// <summary>
    /// A <see cref="ConfigurationId"/> maps a project to the configuration settings stored in the <see cref="ConfigurationRepository"/>.
    /// </summary>
    public class ConfigurationId
    {
        /// <summary>
        /// An empty configuration id.
        /// </summary>
        public static readonly ConfigurationId Empty = new ConfigurationId(string.Empty, string.Empty);

        /// <summary>
        /// The name of this configuration id.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; }

        internal ConfigurationId(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="MFractor.Configuration.ConfigurationId"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="MFractor.Configuration.ConfigurationId"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="MFractor.Configuration.ConfigurationId"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var id = obj as ConfigurationId;
            if (id == null)
            {
                return false;
            }

            return id.Id == this.Id;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="MFractor.Configuration.ConfigurationId"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="MFractor.Configuration.ConfigurationId"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="MFractor.Configuration.ConfigurationId"/>.</returns>
        public override string ToString()
        {
            return Id + (string.IsNullOrWhiteSpace(Name) ? "" : $" - ({Name})");
        }

        /// <summary>
        /// Creates a <see cref="ConfigurationId"/> from the provided <paramref name="projectGuid"/> and <paramref name="projectName"/>.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="projectName">Project name.</param>
        public static ConfigurationId Create(string projectGuid, string projectName)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return Empty;
            }

            return new ConfigurationId(projectGuid, projectName);
        }

        /// <summary>
        /// Creates a <see cref="ConfigurationId"/> from the provided <paramref name="projectGuid"/> and <paramref name="projectName"/>.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="projectName">Project name.</param>
        public static ConfigurationId Create(ProjectIdentifier projectIdentifier)
        {
            if (projectIdentifier is null)
            {
                return Empty;
            }

            return new ConfigurationId(projectIdentifier.Guid, projectIdentifier.Name);
        }

        /// <summary>
        /// Creates a <see cref="ConfigurationId"/> from the provided <paramref name="projectGuid"/>, <paramref name="projectName"/> and <paramref name="filePath"/>.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="projectName">Project name.</param>
        /// <param name="filePath">File path.</param>
        public static ConfigurationId Create(string projectGuid, string projectName, string filePath)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return Empty;
            }

            var id = new ConfigurationId(projectGuid, projectName);

            return Create(id, filePath);
        }

        /// <summary>
        /// Creates a <see cref="ConfigurationId"/> from the provided <paramref name="projectId"/> and <paramref name="filePath"/>.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="projectId">Project identifier.</param>
        /// <param name="filePath">File path.</param>
        public static ConfigurationId Create(ConfigurationId projectId, string filePath)
        {
            if (projectId == null || string.IsNullOrEmpty(projectId.Id))
            {
                return ConfigurationId.Empty;
            }

            var id = projectId.Id + "-" + filePath;

            return new ConfigurationId(id, Path.GetFileName(filePath));
        }
    }
}
