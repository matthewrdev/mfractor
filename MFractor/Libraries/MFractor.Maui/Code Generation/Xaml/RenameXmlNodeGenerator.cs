using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Semantics;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.Xaml
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IRenameXmlNodeGenerator))]
    class RenameXmlNodeGenerator : XamlCodeGenerator, IRenameXmlNodeGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.xaml.change_element_type";

        public override string Name => "Rename XAML Node";

        public override string Documentation => "This code generator will create a list of workUnits that converts the opening and closing tags, property setters and triggers to the new type.";

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        [Import]
        public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver { get; set; }

        public IReadOnlyList<IWorkUnit> Rename(XmlNode node, INamedTypeSymbol newType, string oldName, IParsedXamlDocument xamlDocument, Project project, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            // Can we find this type in the existing XAML document?
            var @namespace = newType.ContainingNamespace;

            var xmlns = xamlDocument.Namespaces.FirstOrDefault(n =>
            {
                var namespaces = XmlnsNamespaceSymbolResolver.GetNamespaces(n, project, xamlDocument.XmlnsDefinitions);

                return namespaces.Any(ns => ns.ToString() == @namespace.ToString());
            });

            if (xmlns != null)
            {
                return Rename(node, newType, xmlns.Prefix, oldName, xamlDocument, semanticModel, platform);
            }

            return new TextInputWorkUnit("XAML Namespace Name",
                                         "What is the name of the xmlns namespace for " + @namespace.ToString() + "?",
                                         @namespace.Name.ToLower(),
                                         "Confirm",
                                         "Cancel",
                                         (name) =>
            {
                var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

                var workUnits = XamlNamespaceImportGenerator.CreateXmlnsImportStatementWorkUnit(xamlDocument, platform, name, newType, xamlDocument.ProjectFile.CompilationProject, xmlPolicy).ToList();

                workUnits.AddRange(Rename(node, newType, name, oldName, xamlDocument, semanticModel, platform));

                return workUnits;

            }).AsList();
        }

        public IReadOnlyList<IWorkUnit> Rename(XmlNode node, INamedTypeSymbol newType, string newTypeXamlNamespaceName, string oldName, IParsedXamlDocument xamlDocument, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            var newName = string.IsNullOrEmpty(newTypeXamlNamespaceName) ? newType.Name : (newTypeXamlNamespaceName + ":" + newType.Name);

            var workUnits = new List<IWorkUnit>();

            workUnits.AddRange(Rename(node, newName, xamlDocument));

            if (node.HasChildren)
            {
                foreach (var n in node.Children)
                {
                    if (XamlSyntaxHelper.IsPropertySetter(n.Name.FullName))
                    {
                        XamlSyntaxHelper.ExplodePropertySetter(n.Name.FullName, out var className, out var propertyName);

                        if (className != oldName)
                        {
                            continue;
                        }

                        workUnits.AddRange(Rename(n, newName + "." + propertyName, xamlDocument));

                        if (propertyName.Equals("Triggers", StringComparison.Ordinal))
                        {
                            // Process triggers that have a data type.
                            var triggers = n.GetChildren(c =>
                            {
                                var triggerType = semanticModel.GetSymbol(c) as INamedTypeSymbol;
                                return SymbolHelper.DerivesFrom(triggerType, platform.TriggerBase.MetaType);
                            });

                            foreach (var trigger in triggers)
                            {
                                var targetType = trigger.GetAttributeByName("TargetType");

                                if (targetType != null && targetType.HasValue)
                                {
                                    workUnits.Add(new ReplaceTextWorkUnit()
                                    {
                                        FilePath = xamlDocument.FilePath,
                                        Span = targetType.Value.Span,
                                        Text = newName,
                                    });
                                }
                            }
                        }
                    }
                }
            }

            if (node.IsRoot && node.HasAttribute("x:Class") && xamlDocument.CodeBehindSymbol != null)
            {
                var codeBehindSymbol = xamlDocument.CodeBehindSymbol;

                var codeBehind = codeBehindSymbol.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax() as ClassDeclarationSyntax;

                if (codeBehind != null)
                {
                    var stackLayoutBase = codeBehind.BaseList.Types.FirstOrDefault(t => t.ToString().EndsWith(node.Name.LocalName, StringComparison.Ordinal));

                    if (stackLayoutBase != null)
                    {
                        workUnits.Add(new ReplaceTextWorkUnit()
                        {
                            Text = newType.ToString(),
                            Span = stackLayoutBase.Span,
                            FilePath = codeBehind.SyntaxTree.FilePath,
                        });
                    }
                }
            }

            return workUnits;
        }

        public IReadOnlyList<IWorkUnit> Rename(XmlNode node, string newName, IParsedXamlDocument document)
        {
            var workUnits = new List<IWorkUnit>
            {
                new ReplaceTextWorkUnit()
                {
                    FilePath = document.FilePath,
                    Text = newName,
                    Span = node.NameSpan,
                }
            };

            if (node.HasClosingTag && node.ClosingTagNameSpanValid)
            {
                workUnits.Add(new ReplaceTextWorkUnit()
                {
                    FilePath = document.FilePath,
                    Text = newName,
                    Span = node.ClosingTagNameSpan,
                });
            }

            return workUnits;
        }
    }
}