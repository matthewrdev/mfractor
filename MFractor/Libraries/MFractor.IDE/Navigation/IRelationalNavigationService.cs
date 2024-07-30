using System;
using System.Threading.Tasks;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Navigation
{
    public interface IRelationalNavigationService
    {
        IRelationalNavigationHandler ResolveRelationalNavigationHandler(IProjectFile projectFile);
        IRelationalNavigationHandler ResolveRelationalNavigationHandler(Project project, string filePath);
    }
}