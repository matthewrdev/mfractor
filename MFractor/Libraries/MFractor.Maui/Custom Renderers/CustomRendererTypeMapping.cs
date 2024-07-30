using System;
using System.Collections.Generic;

namespace MFractor.Maui.CustomRenderers
{
    class CustomRendererTypeMapping
    {


        Dictionary<string, CustomRendererAssocation> Map { get; } = new Dictionary<string, CustomRendererAssocation>();

        public string GetCustomRendererForType(string type,  PlatformFramework platform, bool backwardsCompatible)
        {
            if (Map.ContainsKey(type))
            {
                var mapping = Map[type];

                return backwardsCompatible ? mapping.GetCompatibilityRenderer(platform) : mapping.GetRenderer(platform);
            }

            return string.Empty;
        }

        internal void AddDefaultMapping(string type, string renderer)
        {
            if (!Map.ContainsKey(type))
            {
                Map[type] = new CustomRendererAssocation(type, renderer);
            }
        }

        internal void AddPlatformMapping(string type, string renderer,  PlatformFramework platform)
        {
            if (Map.ContainsKey(type))
            {
                Map[type].AddPlatformRenderer(renderer, platform);
            }
        }

        internal void AddBackwardCompatiblePlatformMapping(string type, string renderer,  PlatformFramework platform)
        {
            if (Map.ContainsKey(type))
            {
                Map[type].AddBackwardCompatiblePlatformRenderer(renderer, platform);
            }
        }

    }
}