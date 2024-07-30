using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Licensing;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.CodeActions
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeActionSuggestionProvider))]
    class CodeActionSuggestionProvider : ICodeActionSuggestionProvider
    {
        protected readonly IReadOnlyList<CodeActionCategory> Categories = new List<CodeActionCategory>()
        {
            CodeActionCategory.Fix,
            CodeActionCategory.Generate,
            CodeActionCategory.Organise,
            CodeActionCategory.Refactor,
            CodeActionCategory.Navigate,
            CodeActionCategory.Misc,
        };

        readonly Lazy<ICodeActionEngine> codeActionEngine;
        public ICodeActionEngine CodeActionEngine => codeActionEngine.Value;

        readonly Lazy<IFeatureContextFactoryRepository> featureContextFactoryRepository;
        public IFeatureContextFactoryRepository FeatureContextFactoryRepository => featureContextFactoryRepository.Value;

        IReadOnlyList<IFeatureContextFactory> FeatureContextFactories => FeatureContextFactoryRepository.FeatureContextFactories;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        [ImportingConstructor]
        public CodeActionSuggestionProvider(Lazy<ICodeActionEngine> codeActionEngine,
                                            Lazy<IFeatureContextFactoryRepository> featureContextFactoryRepository,
                                            Lazy<ILicensingService> licensingService)
        {
            this.codeActionEngine = codeActionEngine;
            this.featureContextFactoryRepository = featureContextFactoryRepository;
            this.licensingService = licensingService;
        }

        public IReadOnlyList<ICodeActionSuggestion> CodeActionSuggestionsAtContext(Project project, string filePath, int offset, params CodeActionCategory[] filterByCategories)
        {
            if (project == null || string.IsNullOrEmpty(filePath))
            {
                return new List<ICodeActionSuggestion>();
            }

            var suggestions = new List<ICodeActionSuggestion>();
            var providers = FeatureContextFactories.Where(e => e.IsInterestedInDocument(project, filePath));

            var interaction = new InteractionLocation(offset);

            foreach (var provider in providers)
            {
                var context = provider.CreateFeatureContext(project, filePath, offset);

                if (context == null)
                {
                    continue;
                }

                var categories = Categories;

                if (filterByCategories != null)
                {
                    categories = filterByCategories.ToList();
                }

                foreach (var category in categories)
                {
                    var availableActions = CodeActionEngine.RetrieveCodeActions(context, interaction, category);

                    if (availableActions.Any())
                    {
                        foreach (var action in availableActions)
                        {
                            var actionSuggestions = action.Suggest(context, interaction);
                            if (actionSuggestions != null && actionSuggestions.Any())
                            {
                                suggestions.AddRange(actionSuggestions);
                            }
                        }
                    }
                }
            }

            return suggestions;
        }
    }
}
