using System;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Data.Repositories;

namespace MFractor.Maui.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class RepositoryCollection : IRepositoryCollection
    {
        public void RegisterRepositories(IDatabase database)
        {
            database.RegisterRepository<ResourceDictionaryReferenceRepository, ResourceDictionaryReference>(new ResourceDictionaryReferenceRepository());
            database.RegisterRepository<StaticResourceDefinitionRepository, StaticResourceDefinition>(new StaticResourceDefinitionRepository());
            database.RegisterRepository<AutomationIdDeclarationRepository, AutomationIdDeclaration>(new AutomationIdDeclarationRepository());
            database.RegisterRepository<ClassDeclarationRepository, ClassDeclaration>(new ClassDeclarationRepository());
            database.RegisterRepository<DynamicResourceDefinitionRepository, DynamicResourceDefinition>(new DynamicResourceDefinitionRepository());

            database.RegisterRepository<DesignTimeBindingContextDefinitionRepository, DesignTimeBindingContextDefinition>(new DesignTimeBindingContextDefinitionRepository());

            database.RegisterRepository<StyleDefinitionRepository, StyleDefinition>(new StyleDefinitionRepository());
            database.RegisterRepository<StyleSetterRepository, StyleSetter>(new StyleSetterRepository());

            database.RegisterRepository<ColorDefinitionRepository, ColorDefinition>(new ColorDefinitionRepository());
            database.RegisterRepository<ColorUsageRepository, ColorUsage>(new ColorUsageRepository());
            database.RegisterRepository<AppThemeColorDefinitionRepository, AppThemeColorDefinition>(new AppThemeColorDefinitionRepository());

            database.RegisterRepository<OnPlatformDeclarationRepository, OnPlatformDeclaration>(new OnPlatformDeclarationRepository());

            database.RegisterRepository<ThicknessDefinitionRepository, ThicknessDefinition>(new ThicknessDefinitionRepository());
            database.RegisterRepository<ThicknessUsageRepository, ThicknessUsage>(new ThicknessUsageRepository());

            database.RegisterRepository<StringResourceDefinitionRepository, StringResourceDefinition>(new StringResourceDefinitionRepository());
        }
    }
}
