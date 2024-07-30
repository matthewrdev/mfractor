using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Utilities
{
    public static class XamlSyntaxHelper
    {
        public static bool IsPropertySetter(XmlNode syntax)
        {
            if (string.IsNullOrEmpty(syntax?.Name?.LocalName))
            {
                return false;
            }

            return IsPropertySetter(syntax.Name.LocalName);
        }

        public static bool IsPropertySetter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.Contains(".");
        }

        public static bool IsMatchingPropertySetter(XmlNode syntax, string name)
        {
            if (!ExplodePropertySetter(syntax, out _, out var propertyName))
            {
                return false;
            }

            return name == propertyName;
        }

        public static bool ExplodePropertySetter(XmlNode syntax, out string className, out string propertyName)
        {
            return ExplodePropertySetter(syntax?.Name?.LocalName, out className, out propertyName);
        }

        public static bool ExplodePropertySetter(string name, out string className, out string propertyName)
        {
            className = "";
            propertyName = "";

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var content = name.Split('.');

            className = content[0];

            if (content.Length != 2)
            {
                return false;
            }

            propertyName = content[1];
            return true;
        }

        public static bool ExplodeTypeReference(string value,
                                                out string namespaceName,
                                                out string className)
        {
            namespaceName = "";
            className = "";

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (!value.Contains(":"))
            {
                className = value;
                return true;
            }

            var components = value.Split(':');

            if (components.Length < 2)
            {
                if (components.Length == 1)
                {
                    namespaceName = components[0];
                }

                return false;
            }

            namespaceName = components[0];
            className = components[1];

            return true;
        }

        public static bool IsAttachedProperty(XmlAttribute syntax)
        {
            return IsAttachedProperty(syntax?.Name);
        }

        public static bool IsAttachedProperty(XmlName propertyName)
        {
            return IsAttachedProperty(propertyName?.LocalName);
        }

        public static bool IsAttachedProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            return propertyName.Contains(".");
        }

        public static bool ExplodeAttachedProperty(XmlAttribute syntax, out string className, out string propertyName)
        {
            return ExplodeAttachedProperty(syntax?.Name, out className, out propertyName);
        }

        public static bool ExplodeAttachedProperty(XmlName attachedPropertyName, out string className, out string propertyName)
        {
            return ExplodeAttachedProperty(attachedPropertyName?.LocalName, out className, out propertyName);
        }

        public static bool ExplodeAttachedProperty(string attachedPropertyName,
                                                   out string className,
                                                   out string propertyName)
        {
            className = "";
            propertyName = "";

            if (string.IsNullOrEmpty(attachedPropertyName))
            {
                return false;
            }

            var content = attachedPropertyName.Split('.');

            if (content.Length != 2)
            {
                return false;
            }

            className = content[0];
            propertyName = content[1];
            return true;
        }

        public static bool IsColorSymbol(XmlAttribute syntax, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            if (syntax == null)
            {
                return false;
            }

            var attrSymbol = semanticModel.GetSymbol(syntax);
            if (attrSymbol == null)
            {
                return false;
            }

            var returnType = SymbolHelper.ResolveMemberReturnType(attrSymbol);

            return FormsSymbolHelper.IsColor(returnType, platform);
        }

        public static bool IsConstructor(XmlNode syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsConstructor(syntax, xamlNamespace);
        }

        public static bool IsConstructor(XmlAttribute syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsConstructor(syntax, xamlNamespace);
        }

        public static bool IsConstructor(XmlNode syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.Arguments;
        }

        public static bool IsConstructor(XmlAttribute syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.Arguments;
        }

        public static bool IsTypeArguments(XmlNode syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsTypeArguments(syntax, xamlNamespace);
        }

        public static bool IsTypeArguments(XmlAttribute syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsTypeArguments(syntax, xamlNamespace);
        }

        public static bool IsTypeArguments(XmlAttribute syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.TypeArguments;
        }

        public static bool IsTypeArguments(XmlNode syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.TypeArguments;
        }

        public static bool IsDictionaryKey(XmlNode syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsDictionaryKey(syntax, xamlNamespace);
        }

        public static bool IsDictionaryKey(XmlAttribute syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsDictionaryKey(syntax, xamlNamespace);
        }

        public static bool IsDictionaryKey(XmlAttribute syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.DictionaryKey;
        }

        public static bool IsDictionaryKey(XmlNode syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.DictionaryKey;
        }

        public static bool IsCodeBehindFieldName(XmlNode syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsCodeBehindFieldName(syntax, xamlNamespace);
        }

        public static bool IsCodeBehindFieldName(XmlAttribute syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsCodeBehindFieldName(syntax, xamlNamespace);
        }

        public static bool IsCodeBehindFieldName(XmlAttribute syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.CodeBehindName;
        }

        public static bool IsCodeBehindFieldName(XmlNode syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.CodeBehindName;
        }

        public static bool IsCodeBehindClassName(XmlNode syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsCodeBehindClassName(syntax, xamlNamespace);
        }

        public static bool IsCodeBehindClassName(XmlAttribute syntax, IXamlNamespaceCollection namespaces)
        {
            var xamlNamespace = namespaces.ResolveNamespace(syntax);

            if (xamlNamespace == null)
            {
                return false;
            }

            return IsCodeBehindClassName(syntax, xamlNamespace);
        }

        public static bool IsCodeBehindClassName(XmlAttribute syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.CodeBehindClass;
        }

        public static bool IsCodeBehindClassName(XmlNode syntax, IXamlNamespace xamlNamespace)
        {
            if (!XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return false;
            }

            return syntax.Name.LocalName == Keywords.MicrosoftSchema.CodeBehindClass;
        }
    }
}
