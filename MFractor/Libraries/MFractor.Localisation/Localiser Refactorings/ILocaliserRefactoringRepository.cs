using System;
using System.Collections.Generic;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.LocaliserRefactorings
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> that provides the <see cref="ILocaliserRefactoring"/>'s within the app domain.
    /// </summary>
    public interface ILocaliserRefactoringRepository : IPartRepository<ILocaliserRefactoring>
    {
        IReadOnlyList<ILocaliserRefactoring> LocaliserRefactorings { get; }

        ILocaliserRefactoring GetSupportedLocaliserRefactoring(Project project, string filePath);
    }
}
