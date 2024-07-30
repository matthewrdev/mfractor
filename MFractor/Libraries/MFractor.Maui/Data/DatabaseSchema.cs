using System;
using MFractor.Data;
using MFractor.Maui.Data.Models;
using MFractor.Data.Schemas;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MFractor.Maui.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DatabaseSchema : Schema
    {
        public override string Domain => "Xaml";

        protected override IReadOnlyDictionary<Type, string> BuildTables()
        {
            return new Dictionary<Type, string>()
            .AddTable<StaticResourceDefinition>()
            .AddTable<ResourceDictionaryReference>()
            .AddTable<AutomationIdDeclaration>()
            .AddTable<DynamicResourceDefinition>()
            .AddTable<ClassDeclaration>()
            .AddTable<StyleSetter>()
            .AddTable<StyleDefinition>()
            .AddTable<ColorDefinition>()
            .AddTable<ThicknessDefinition>()
            .AddTable<OnPlatformDeclaration>()
            .AddTable<ExportFontDeclaration>()
            .AddTable<ThicknessUsage>()
            .AddTable<ColorUsage>()
            .AddTable<AppThemeColorDefinition>()
            .AddTable<StringResourceDefinition>()
            .AddTable<DesignTimeBindingContextDefinition>();
        }
    }
}
