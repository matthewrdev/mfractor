using System;
using System.ComponentModel.Composition;
using System.Xml;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CustomRenderers
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICustomRendererTypeMappingService))]
    class CustomRendererTypeMappingService : ICustomRendererTypeMappingService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        // TODO: Update custom renderer type mapping to support MAUI.
        const string mappingXml = "Resources.Configuration.CustomRendererTypeMapping.xml";

        readonly CustomRendererTypeMapping mapping = new CustomRendererTypeMapping();

        void LoadConfiguration()
        {
            var resource = ResourcesHelper.LocateMatchingResourceId(GetType().Assembly, mappingXml);

            var content = ResourcesHelper.ReadResourceContent(this, resource);

            try
            {
                var document = new XmlDocument();
                document.LoadXml(content);

                var root = document.SelectSingleNode(@".//Renderers");

                var nodes = root.SelectNodes(@".//Map");

                foreach (var node in nodes)
                {
                    var map = node as XmlNode;

                    if (map != null)
                    {
                        var typeAttr = map.Attributes.GetNamedItem("Type");
                        var defaultRendererAttr = map.Attributes.GetNamedItem("Renderer");

                        if (typeAttr == null)
                        {
                            continue;
                        }

                        var type = typeAttr.Value;

                        mapping.AddDefaultMapping(type, defaultRendererAttr?.Value ?? string.Empty);

                        var platforms = map.SelectNodes(".//OnPlatform");

                        foreach (var p in platforms)
                        {
                            var onplatform = p as XmlNode;

                            if (onplatform != null)
                            {
                                var platform = onplatform.Attributes.GetNamedItem("Platform");
                                var renderer = onplatform.Attributes.GetNamedItem("Renderer");
                                var backwardCompatible = onplatform.Attributes.GetNamedItem("BackwardsCompatible");

                                if (platform == null || string.IsNullOrEmpty(platform.Value))
                                {
                                    continue;
                                }

                                if (renderer == null || string.IsNullOrEmpty(renderer.Value))
                                {
                                    continue;
                                }

                                if (!Enum.TryParse<PlatformFramework>(platform.Value, true, out var targetPlatform))
                                {
                                    continue;
                                }

                                if (backwardCompatible != null && string.Equals(backwardCompatible.Value, "true", StringComparison.OrdinalIgnoreCase))
                                {
                                    mapping.AddBackwardCompatiblePlatformMapping(type, renderer.Value, targetPlatform);
                                }
                                else
                                {
                                    mapping.AddPlatformMapping(type, renderer.Value, targetPlatform);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public string GetCustomRendererForType(INamedTypeSymbol type, IXamlPlatform platform, PlatformFramework platformFramework, bool backwardsCompatible, out string rendererControlType)
        {
            rendererControlType = string.Empty;

            while (SymbolHelper.DerivesFrom(type, platform.Element.MetaType))
            {
                var rendererType = mapping.GetCustomRendererForType(type.ToString(), platformFramework, backwardsCompatible);
                if (!string.IsNullOrEmpty(rendererType))
                {
                    rendererControlType = type.ToString();
                    return rendererType;
                }

                type = type.BaseType;
            }

            return string.Empty;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            LoadConfiguration();
        }
    }
}