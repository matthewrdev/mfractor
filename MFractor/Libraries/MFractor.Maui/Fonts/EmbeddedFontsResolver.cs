using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Fonts;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IEmbeddedFontsResolver))]
    class EmbeddedFontsResolver : IEmbeddedFontsResolver
    {
        class EmbeddedFontDefinition
        {
            public EmbeddedFontDefinition(string fontName, string alias)
            {
                FontName = fontName;
                Alias = alias;
            }

            public string FontName { get; }

            public string Alias { get; }
        }

        readonly Lazy<IFontAssetResolver> fontAssetResolver;
        public IFontAssetResolver FontAssetResolver => fontAssetResolver.Value;

        [ImportingConstructor]
        public EmbeddedFontsResolver(Lazy<IFontAssetResolver> fontAssetResolver)
        {
            this.fontAssetResolver = fontAssetResolver;
        }

        public IEnumerable<IEmbeddedFont> GetEmbeddedFonts(Project project, IXamlPlatform platform)
        {
            var definitions = GetEmbeddedFontDefinitions(project, platform);

            return definitions.Select(d => new EmbeddedFont(d.FontName, d.Alias, project, new Lazy<IFont>(() =>
           {
               return FontAssetResolver.GetNamedFontAsset(project, d.FontName);
           })));
        }

        IEnumerable<EmbeddedFontDefinition> GetEmbeddedFontDefinitions(Project project, IXamlPlatform platform)
        {
            if (!platform.SupportsExportFontAttribute)
            {
                return Enumerable.Empty<EmbeddedFontDefinition>();
            }

            if (project == null)
            {
                return Enumerable.Empty<EmbeddedFontDefinition>();
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return Enumerable.Empty<EmbeddedFontDefinition>();
            }

            var assembly = compilation.Assembly;

            if (assembly == null)
            {
                return Enumerable.Empty<EmbeddedFontDefinition>();
            }

            var attributes = assembly.GetAttributes();

            if (!attributes.Any())
            {
                return Enumerable.Empty<EmbeddedFontDefinition>();
            }

            var exportedFonts = attributes.Where(a => SymbolHelper.DerivesFrom(a.AttributeClass, platform.ExportFontAttribute.MetaType));

            if (!exportedFonts.Any())
            {
                return Enumerable.Empty<EmbeddedFontDefinition>();
            }

            var result = new List<EmbeddedFontDefinition>();
            foreach (var exportedFont in exportedFonts)
            {
                var arguments = exportedFont.ConstructorArguments;
                var namedArguments = exportedFont.NamedArguments;

                if (arguments == null || arguments.Length < 1)
                {
                    continue;
                }

                var fontFileName = arguments[0].Value as string;

                var alias = string.Empty;


                if (namedArguments != null
                    && namedArguments.Any(kp => kp.Key == "Alias"))
                {
                    var aliasArgument = namedArguments.FirstOrDefault(na => na.Key == "Alias");

                    alias = aliasArgument.Value.Value as string;
                }

                if (!string.IsNullOrEmpty(fontFileName))
                {
                    result.Add(new EmbeddedFontDefinition(fontFileName, alias));
                }
            }

            return result;
        }

        public IEmbeddedFont GetEmbeddedFontByName(Project project, IXamlPlatform platform, string name)
        {
            if (project is null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var definitions = GetEmbeddedFonts(project, platform);

            if (definitions == null || !definitions.Any())
            {
                return null;
            }

            var definition = definitions.FirstOrDefault(d => d.LookupName == name);
            if (definition == null)
            {
                return null;
            }

            return new EmbeddedFont(definition.FontName,
                                    definition.Alias,
                                    project,
                                    new Lazy<IFont>(() => FontAssetResolver.GetNamedFontAsset(project, definition.FontFileName)));
        }
    }
}