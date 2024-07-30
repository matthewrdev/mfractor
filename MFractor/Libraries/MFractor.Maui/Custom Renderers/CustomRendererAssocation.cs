using System.Collections.Generic;
using System.Linq;
using MFractor.Utilities;

namespace MFractor.Maui.CustomRenderers
{
    class PlatformRendererMapping
    {
        public string Renderer { get; }

        public bool IsBackwardsCompatible { get; }

        public PlatformRendererMapping(string renderer, bool isCompatible)
        {
            Renderer = renderer;
            IsBackwardsCompatible = isCompatible;
        }
    }

    class CustomRendererAssocation
    {
        public string Type { get; }

        readonly string defaultRenderer;

        readonly Dictionary<PlatformFramework, List<PlatformRendererMapping>> platformRendererMap = new Dictionary<PlatformFramework, List<PlatformRendererMapping>>();

        public CustomRendererAssocation(string type,
                                        string defaultRenderer)
        {
            Type = type;
            this.defaultRenderer = defaultRenderer;
        }

        public void AddPlatformRenderer(string renderer,  PlatformFramework platform)
        {
            if (!platformRendererMap.ContainsKey(platform))
            {
                platformRendererMap[platform] = new List<PlatformRendererMapping>();
            }

            platformRendererMap[platform].Insert(0, new PlatformRendererMapping(renderer, false));
        }

        public void AddBackwardCompatiblePlatformRenderer(string renderer,  PlatformFramework platform)
        {
            if (!platformRendererMap.ContainsKey(platform))
            {
                platformRendererMap[platform] = new List<PlatformRendererMapping>();
            }

            platformRendererMap[platform].Insert(0, new PlatformRendererMapping(renderer, true));
        }

        public string GetRenderer(PlatformFramework platform)
        {
            var renderer = defaultRenderer;

            if (platformRendererMap.ContainsKey(platform))
            {
                renderer = platformRendererMap[platform]?.FirstOrDefault(r => r.IsBackwardsCompatible == false)?.Renderer;

                if (string.IsNullOrEmpty(renderer))
                {
                    renderer = defaultRenderer;
                }
            }

            var display = EnumHelper.GetEnumDescription(platform);

            return renderer.Replace("$platform$", display).Replace("$type$", Type);
        }

        public string GetCompatibilityRenderer(PlatformFramework platform)
        {
            var renderer = "";

            if (platformRendererMap.ContainsKey(platform))
            {
                renderer =  platformRendererMap[platform]?.FirstOrDefault(r => r.IsBackwardsCompatible)?.Renderer;

                if (string.IsNullOrEmpty(renderer))
                {
                    renderer = defaultRenderer;
                }
            }

            var display = EnumHelper.GetEnumDescription(platform);

            return defaultRenderer.Replace("$platform$", display).Replace("$type$", Type);
        }
    }
}
