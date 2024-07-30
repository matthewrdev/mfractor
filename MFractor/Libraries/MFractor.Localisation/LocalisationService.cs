using System;
using System.ComponentModel.Composition;
using MFractor.Localisation.LocaliserRefactorings;
using MFractor.Localisation.StringsProviders;
using MFractor.Localisation.ValueProviders;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILocalisationService))]
    class LocalisationService : ILocalisationService
    {
        readonly Lazy<ILocaliserRefactoringRepository> localiserRefactoringRepository;
        public ILocaliserRefactoringRepository LocaliserRefactoringRepository => localiserRefactoringRepository.Value;

        readonly Lazy<ILocalisableStringsProviderRepository> localisableStringsProviderRepository;
        public ILocalisableStringsProviderRepository LocalisableStringsProviderRepository => localisableStringsProviderRepository.Value;

        readonly Lazy<ILocalisationValuesProviderRepository> localisationValuesProviderRepository;
        public ILocalisationValuesProviderRepository LocalisationValuesProviderRepository => localisationValuesProviderRepository.Value;

        [ImportingConstructor]
        public LocalisationService(Lazy<ILocalisableStringsProviderRepository> localisableStringsProviderRepository,
                                   Lazy<ILocaliserRefactoringRepository> localiserRefactoringRepository,
                                   Lazy<ILocalisationValuesProviderRepository> localisationValuesProviderRepository)
        {
            this.localisableStringsProviderRepository = localisableStringsProviderRepository;
            this.localiserRefactoringRepository = localiserRefactoringRepository;
            this.localisationValuesProviderRepository = localisationValuesProviderRepository;
        }

        public bool CanLocalise(Project project, string filePath)
        {
            var valuesProvider = LocalisationValuesProviderRepository.GetSupportedValuesProvider(project);
            var localiserRefactoring = LocaliserRefactoringRepository.GetSupportedLocaliserRefactoring(project, filePath);
            var stringsProvider = LocalisableStringsProviderRepository.GetSupportedStringsProvider(project, filePath);

            return valuesProvider != null
                && stringsProvider != null
                && localiserRefactoring != null;
        }
    }
}