using System;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Xmlns
{
    public static class XamlSchemaHelper
    {
        public static bool IsSchema(IXamlNamespace xamlNamespace)
        {
            if (xamlNamespace is null)
            {
                return false;
            }

            if (xamlNamespace.Schema is null)
            {
                return false;
            }

            return true;
        }

        public static bool IsSchema(IXamlNamespace xamlNamespace, string schemaUrl)
        {
            if (xamlNamespace is null)
            {
                return false;
            }

            if (xamlNamespace.Schema is null)
            {
                return false;
            }

            return xamlNamespace.Schema.Uri == schemaUrl;
        }

        public static bool IsMicrosoftSchema(IXamlNamespace xamlNamespace)
        {
            return IsSchema(xamlNamespace, XamlSchemas.MicrosoftSchemaUrl);
        }

        public static bool IsMarkupCompatibilitySchema(IXamlNamespace xamlNamespace)
        {
            return IsSchema(xamlNamespace, XamlSchemas.MarkupCompatibilitySchemaUrl);
        }

        public static bool IsPlatformSchema(IXamlNamespace xamlNamespace, IXamlPlatform platform)
        {
            return IsSchema(xamlNamespace, platform.SchemaUrl);
        }

        public static bool IsDesign(IXamlNamespace xamlNamespace)
        {
            return IsSchema(xamlNamespace, XamlSchemas.DesignSchemaUrl);
        }
    }
}