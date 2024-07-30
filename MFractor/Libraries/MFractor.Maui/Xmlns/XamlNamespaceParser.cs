using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Xml;

namespace MFractor.Maui.Xmlns
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlNamespaceParser))]
    class XamlNamespaceParser : IXamlNamespaceParser
    {
        public IXamlNamespaceCollection ParseNamespaces(XmlSyntaxTree syntaxTree)
        {
            var namespaces = new List<IXamlNamespace>();

            if (syntaxTree == null
                || syntaxTree.Root == null
                || syntaxTree.Root.Attributes == null)
            {
                return new XamlNamespaceCollection(namespaces);
            }

            var namespaceAttributes = syntaxTree.Root.GetAttributes(attr => attr.Name.FullName.StartsWith("xmlns", StringComparison.Ordinal));

            foreach (var attr in namespaceAttributes)
            {
                var ns = Parse(attr.Name.FullName, attr.Value?.Value);
                if (ns != null)
                {
                    namespaces.Add(ns);
                }
            }

            return new XamlNamespaceCollection(namespaces);
        }

        /// <summary>
        /// Parse the specified name and value into a new <see cref="XamlNamespace"/>.
        /// </summary>
        /// <returns>The parse.</returns>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public IXamlNamespace Parse(string name, string value)
        {
            var xmlns = new XamlNamespace();

            try
            {
                xmlns.Prefix = name;
                xmlns.Value = value;

                if (name.StartsWith("xmlns:", StringComparison.OrdinalIgnoreCase))
                {
                    var components = name.Split(':');
                    xmlns.Prefix = components[1];
                }
                else
                {
                    if (!value.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    xmlns.Prefix = "";
                }

                var namespaceName = "";
                var namespaceKeyword = "";

                var assemblyName = "";
                var assemblyKeyword = "";

                var platformName = "";
                var platformKeyword = "";

                XamlSchema schema = null;

                if (value.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
                {
                    schema = new XamlSchema(value);
                }
                else
                {
                    var content = xmlns.Value.Split(';');

                    var clr = content.FirstOrDefault(c => c.StartsWith(Keywords.Xmlns.Namespace, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(clr))
                    {
                        namespaceName = clr.Remove(0, Keywords.Xmlns.Namespace.Length + 1);
                        namespaceKeyword = clr.Substring(0, Keywords.Xmlns.Namespace.Length);
                    }
                    else
                    {
                        clr = content.FirstOrDefault(c => c.StartsWith(Keywords.Xmlns.Using, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(clr))
                        {
                            namespaceName = clr.Remove(0, Keywords.Xmlns.Using.Length + 1);
                            namespaceKeyword = clr.Substring(0, Keywords.Xmlns.Using.Length);
                        }
                    }

                    var targetPlatform = content.FirstOrDefault(c => c.StartsWith(Keywords.Xmlns.TargetPlatform, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(targetPlatform))
                    {
                        platformName = targetPlatform.Remove(0, Keywords.Xmlns.TargetPlatform.Length + 1);
                        platformKeyword = targetPlatform.Substring(0, Keywords.Xmlns.TargetPlatform.Length);
                    }

                    var assembly = content.FirstOrDefault(c => c.StartsWith(Keywords.Xmlns.Assembly, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(assembly))
                    {
                        assemblyName = assembly.Remove(0, Keywords.Xmlns.Assembly.Length + 1);
                        assemblyKeyword = assembly.Substring(0, Keywords.Xmlns.Assembly.Length);
                    }
                }

                xmlns.ClrNamespaceComponent = new ClrNamespaceDeclaration(namespaceKeyword, namespaceName);
                xmlns.AssemblyComponent = new AssemblyDeclaration(assemblyKeyword, assemblyName);
                xmlns.TargetPlatformComponent = new TargetPlatformDeclaration(platformKeyword, platformName);
                xmlns.Schema = schema;
            }
            catch
            {
                xmlns = null;
            }

            return xmlns;
        }
    }
}
