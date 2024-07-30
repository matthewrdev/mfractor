using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.CodeGeneration.Styles;
using MFractor.Maui.Configuration;
using MFractor.Maui.Styles;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.WorkUnits;
using MFractor.Maui.XamlPlatforms;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.CodeActions.Refactor.Styles
{
    class ExtractStyle : RefactorXamlCodeAction
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<IStylePropertyFinder> stylePropertyFinder;
        public IStylePropertyFinder StylePropertyFinder => stylePropertyFinder.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        [Import]
        public IStyleGenerator StyleGenerator { get; set; }

        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        [ImportingConstructor]
        public ExtractStyle(Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                            Lazy<IStylePropertyFinder> stylePropertyFinder,
                            Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.stylePropertyFinder = stylePropertyFinder;
            this.textProviderService = textProviderService;
        }

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.extract_style";

        public override string Name => "Extract Style";

        public override string Documentation => "Extracts a new style from a given XAML node.";

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.VisualElement.MetaType))
            {
                return false;
            }

            var attrs = syntax.GetAttributes(a =>
            {
                if (!(SymbolHelper.FindMemberSymbolByName(symbol, a.Name.LocalName) is IPropertySymbol property))
                {
                    return false;
                }

                return true;
            });

            return attrs.Any();
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Extract XAML Style").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var targetTypeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            var properties = StylePropertyFinder.FindCandidateStyleProperties(syntax, context.XamlSemanticModel, context.Platform);

            var parentStyle = "";

            var style = syntax.GetAttribute(a =>
            {
                var property = SymbolHelper.FindMemberSymbolByName(targetTypeSymbol, a.Name.LocalName) as IPropertySymbol;

                if (property != null
                    && property.Name == "Style"
                    && SymbolHelper.DerivesFrom(property.Type, context.Platform.Style.MetaType))
                {
                    return true;
                }

                return false;
            });

            if (style != null && style.HasValue)
            {
                var expression = context.XamlSemanticModel.GetExpression(style) as StaticResourceExpression;

                if (expression != null
                    && expression.Value != null
                    && expression.Value.HasValue)
                {
                    parentStyle = expression.Value.Value;
                }
            }

            IReadOnlyList<IWorkUnit> applyStyle(IXamlPlatform platform,
                                              string styleName,
                                              string targetType,
                                              string targetTypePrefix,
                                              string parentStyleName,
                                              ParentStyleType parentStyleType,
                                              IReadOnlyDictionary<string, string> styleProperties,
                                              string targetFilePath)
            {
                var workUnits = new List<IWorkUnit>();

                var styleAttrCode = $"Style=\"{{StaticResource {styleName}}}\"";

                // Is this an implicit style?
                if (string.IsNullOrEmpty(styleName))
                {
                    styleAttrCode = string.Empty;
                }

                if (style != null)
                {
                    workUnits.Add(new ReplaceTextWorkUnit()
                    {
                        Text = styleAttrCode,
                        Span = style.Span,
                        FilePath = document.FilePath,
                    });
                }
                else
                {
                    workUnits.Add(new InsertTextWorkUnit(" " + styleAttrCode, syntax.NameSpan.End, document.FilePath));
                }

                workUnits.AddRange(RemoveProperties(syntax, document.FilePath, styleProperties.Keys, targetTypeSymbol));

                // Create the new style, locate the target file and insert it into it's resource dictionary.

                var styleSyntax = StyleGenerator.GenerateSyntax(platform, styleName, targetType, targetTypePrefix, parentStyleName, parentStyleType, styleProperties);

                XmlSyntaxTree xmlSyntaxTree = null;

                if (targetFilePath == document.FilePath)
                {
                    xmlSyntaxTree = document.GetSyntaxTree();

                    if (!File.Exists(targetFilePath))
                    {
                        log?.Info("The file " + targetFilePath + " does not exist, cannot extract the resource into it.. Using the current file instead.");
                    }
                    targetFilePath = document.FilePath;
                }
                else
                {
                    xmlSyntaxTree = XmlSyntaxParser.ParseText(TextProviderService.GetTextProvider(targetFilePath));
                }

                if (xmlSyntaxTree == null || xmlSyntaxTree.Root == null)
                {
                    log?.Warning("Cannot refactor/extract style as the target XML syntax tree is null");
                    return Array.Empty<IWorkUnit>();
                }

                var insertion = InsertResourceEntryGenerator.Generate(context.Project, targetFilePath, xmlSyntaxTree, styleSyntax);

                workUnits.InsertRange(0, insertion);

                return workUnits;
            }

            var file = ProjectService.GetProjectFileWithFilePath(context.Project, document.FilePath);
            var appxaml = AppXamlConfiguration.ResolveAppXamlFile(context.Project, context.Platform);

            var xmlns = context.Namespaces.ResolveNamespace(syntax);

            return new XamlStyleEditorWorkUnit()
            {
                FilePath = document.FilePath,
                TargetType = targetTypeSymbol,
                TargetTypePrefix = xmlns.Prefix,
                HelpUrl = "https://docs.mfractor.com/xamarin-forms/working-with-styles/extracting-styles/",
                ApplyButtonLabel = "Extract Style",
                ParentStyleName = parentStyle,
                ParentStyleType = ParentStyleType.BaseResourceKey,
                Properties = properties,
                Project = context.Project,
                Platform = context.Platform,
                ApplyStyleDelegate = applyStyle,
                TargetFiles = new List<IProjectFile>()
                {
                    file, appxaml
                },
            }.AsList();
        }

        IReadOnlyList<IWorkUnit> RemoveProperties(XmlNode syntax, string filePath, IEnumerable<string> properties, INamedTypeSymbol targetTypeSymbol)
        {
            var workUnits = new List<IWorkUnit>();

            if (syntax.HasAttributes)
            {
                foreach (var property in properties)
                {
                    var attr = syntax.GetAttribute(a =>
                    {
                        if (!(SymbolHelper.FindMemberSymbolByName(targetTypeSymbol, a.Name.LocalName) is IPropertySymbol propertySymbol))
                        {
                            return false;
                        }

                        return property == propertySymbol.Name;
                    });

                    if (attr != null)
                    {
                        var start = attr.Span.Start;
                        var end = attr.Span.End;

                        var index = syntax.Attributes.IndexOf(attr);
                        if (index - 1 >= 0)
                        {
                            start = syntax.Attributes[index - 1].Span.End;
                        }

                        workUnits.Add(new ReplaceTextWorkUnit()
                        {
                            Text = string.Empty,
                            Span = TextSpan.FromBounds(start, end),
                            FilePath = filePath,
                        });
                    }
                }
            }

            return workUnits;
        }
    }
}
