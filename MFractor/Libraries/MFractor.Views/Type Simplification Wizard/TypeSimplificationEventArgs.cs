using System;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.TypeSimplificationWizard
{
    public class TypeSimplificationEventArgs : EventArgs
    {
        public TypeSimplificationEventArgs(IProjectFile projectFile, string simplifiedContent)
        {
            SimplifiedContent = simplifiedContent;
            ProjectFile = projectFile ?? throw new ArgumentNullException(nameof(projectFile));
        }

        public string SimplifiedContent { get; }

        public IProjectFile ProjectFile { get; }
    }
}
