using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.LocaliserRefactorings
{
    [Export(typeof(ILocaliserRefactoringRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class LocaliserRefactoringRepository : PartRepository<ILocaliserRefactoring>, ILocaliserRefactoringRepository
    {
        [ImportingConstructor]
        public LocaliserRefactoringRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ILocaliserRefactoring> LocaliserRefactorings => Parts;

        public ILocaliserRefactoring GetSupportedLocaliserRefactoring(Project project, string filePath)
        {
            return LocaliserRefactorings.FirstOrDefault(lr => lr.IsAvailable(project, filePath));
        }
    }
}