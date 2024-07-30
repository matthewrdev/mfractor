using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.DeleteOutputFolders
{
    /// <summary>
    /// The <see cref="IDeleteOutputFoldersConfigurationService"/> manages the user-configured preferences for the delete output folders feature in each solution/project.
    /// </summary>
    public interface IDeleteOutputFoldersConfigurationService
    {
        IReadOnlyList<IDeleteOutputFoldersConfiguration> Configurations { get; }

        void Clear();

        bool HasConfiguration(string identifier);

        bool HasConfiguration(Solution solution);

        bool HasConfiguration(Project project);

        IDeleteOutputFoldersOptions GetOptionsOrDefault(string identifier);

        IDeleteOutputFoldersOptions GetOptionsOrDefault(Solution solution);

        IDeleteOutputFoldersOptions GetOptionsOrDefault(Project project);

        void SetOptions(string name, string identifier, IDeleteOutputFoldersOptions options);

        void SetOptions(IDeleteOutputFoldersConfiguration configuration);

        void SetOptions(Solution solution, IDeleteOutputFoldersOptions options);

        void SetOptions(Project project, IDeleteOutputFoldersOptions options);

        string GetIdentifier(Solution solution);

        string GetIdentifier(Project project);

        event EventHandler ConfigurationsChanged;
    }
}
