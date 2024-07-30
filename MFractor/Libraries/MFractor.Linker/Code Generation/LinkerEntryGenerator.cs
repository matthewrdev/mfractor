using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;

using MFractor.Linker.Helpers;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Linker.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILinkerEntryGenerator))]
    class LinkerEntryGenerator : CodeGenerator, ILinkerEntryGenerator
    {
        public override string[] Languages { get; } = new string[] { "XML" };

        public override string Identifier => "com.mfractor.code_gen.linker.linker_entry";

        public override string Name => "Linker XML Generator";

        public override string Documentation => "The Linker XML generator can create linker XML entries";

        public bool CanCreateLinkerEntry(ISymbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            if (symbol is IAssemblySymbol
                || symbol is INamedTypeSymbol
                || symbol is IFieldSymbol
                || symbol is IMethodSymbol
                || symbol is IPropertySymbol)
            {
                return true;
            }

            return false;
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(ISymbol symbol)
        {
            if (symbol is IAssemblySymbol assembly)
            {
                return CreateLinkerEntry(assembly);
            }
            else if (symbol is INamedTypeSymbol namedType)
            {
                return CreateLinkerEntry(namedType, LinkerTypeMembersPreserveMode.All);
            }
            else if (symbol is INamespaceSymbol @namespace)
            {
                return CreateLinkerEntry(@namespace);
            }
            else if (symbol is IFieldSymbol field)
            {
                return CreateLinkerEntry(field, LinkerMemberEntryMode.Name);
            }
            else if (symbol is IMethodSymbol method)
            {
                return CreateLinkerEntry(method, LinkerMemberEntryMode.Name);
            }
            else if (symbol is IPropertySymbol property)
            {
                return CreateLinkerEntry(property);
            }
            else if (symbol is IEventSymbol @event)
            {
                return CreateLinkerEntry(@event);
            }
            else
            {
                throw new ArgumentException("The symbol " + symbol.ToString() + " of type " + symbol.GetType() + " is not supported for linking.");
            }
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(IAssemblySymbol assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var node = new XmlNode()
            {
                Name = new XmlName(LinkerKeywords.Elements.Assembly)
            };
            node.IsSelfClosing = true;

            node.AddAttribute(LinkerKeywords.Attributes.FullName, assembly.Name);

            return node.AsList();
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(INamedTypeSymbol namedType, LinkerTypeMembersPreserveMode preserveMode)
        {
            if (namedType == null)
            {
                throw new ArgumentNullException(nameof(namedType));
            }

            var node = new XmlNode()
            {
                Name = new XmlName(LinkerKeywords.Elements.Type)
            };
            node.IsSelfClosing = true;

            node.AddAttribute(LinkerKeywords.Attributes.FullName, namedType.ToString());

            switch (preserveMode)
            {
                case LinkerTypeMembersPreserveMode.Methods:
                    node.AddAttribute(LinkerKeywords.Attributes.Preserve, "methods");
                    break;
                case LinkerTypeMembersPreserveMode.Fields:
                    node.AddAttribute(LinkerKeywords.Attributes.Preserve, "fields");
                    break;
                default:
                    break;
            }

            return node.AsList();
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(INamespaceSymbol @namespace)
        {
            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            var node = new XmlNode()
            {
                Name = new XmlName(LinkerKeywords.Elements.Type)
            };
            node.IsSelfClosing = true;

            var namespaceName = @namespace.Name;

            var parent = @namespace.ContainingNamespace;
            while (parent != null)
            {
                namespaceName = parent.Name + "." + namespaceName;
                parent = parent.ContainingNamespace;
            }

            node.AddAttribute(LinkerKeywords.Attributes.FullName, namespaceName);

            return node.AsList();
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(IFieldSymbol field, LinkerMemberEntryMode linkerMethodEntryType)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var node = new XmlNode()
            {
                Name = new XmlName(LinkerKeywords.Elements.Field)
            };
            node.IsSelfClosing = true;

            switch (linkerMethodEntryType)
            {
                case LinkerMemberEntryMode.Name:
                    node.AddAttribute(LinkerKeywords.Attributes.Name, field.Name);
                    break;
                case LinkerMemberEntryMode.Signature:
                    node.AddAttribute(LinkerKeywords.Attributes.Signature, LinkerSignatureHelper.GetSignature(field));
                    break;
            }

            return node.AsList();
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(IMethodSymbol method, LinkerMemberEntryMode linkerMethodEntryType)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var node = new XmlNode()
            {
                Name = new XmlName(LinkerKeywords.Elements.Method),
            };
            node.IsSelfClosing = true;

            switch (linkerMethodEntryType)
            {
                case LinkerMemberEntryMode.Name:
                    node.AddAttribute(LinkerKeywords.Attributes.Name, method.Name);
                    break;
                case LinkerMemberEntryMode.Signature:
                    node.AddAttribute(LinkerKeywords.Attributes.Signature, LinkerSignatureHelper.GetSignature(method));
                    break;
            }

            return node.AsList();
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(IPropertySymbol property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var nodes = new List<XmlNode>();

            if (property.GetMethod != null)
            {
                nodes.AddRange(CreateLinkerEntry(property.GetMethod, LinkerMemberEntryMode.Name));
            }

            if (property.SetMethod != null)
            {
                nodes.AddRange(CreateLinkerEntry(property.SetMethod, LinkerMemberEntryMode.Name));
            }

            return nodes;
        }

        public IReadOnlyList<XmlNode> CreateLinkerEntry(IEventSymbol @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var node = new XmlNode()
            {
                Name = new XmlName(LinkerKeywords.Elements.Field),
            };
            node.IsSelfClosing = true;

            node.AddAttribute(LinkerKeywords.Attributes.Name, @event.Name);

            return node.AsList();
        }
    }
}