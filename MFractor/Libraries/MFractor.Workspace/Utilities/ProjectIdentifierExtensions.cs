using System;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace.Utilities
{
    public static class ProjectIdentifierExtensions
    {
        /// <summary>
        /// For this given <paramref name="project"/>, get a <see cref="ProjectIdentifier"/>.
        /// </summary>
        /// <returns>The identifier.</returns>
        /// <param name="project">Project.</param>
        public static ProjectIdentifier GetIdentifier(this Project project)
        {
            if (project == null)
            {
                return null;
            }

            var guid = Resolver.Resolve<IProjectService>().GetProjectGuid(project);

            return new ProjectIdentifier(guid, project.Name);
        }
    }
}
