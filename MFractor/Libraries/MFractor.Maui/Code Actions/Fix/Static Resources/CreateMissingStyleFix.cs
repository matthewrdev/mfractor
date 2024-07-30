using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.StaticResources;
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

namespace MFractor.Maui.CodeActions.Fix.StaticResources
{
    class CreateMissingStyleFix : FixCodeAction
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        [Import]
        public IStyleGenerator StyleGenerator { get; set; }

        [Import]
        public IAppXamlConfiguration AppXamlConfiguration { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        readonly Lazy<IStylePropertyFinder> stylePropertyFinder;
        public IStylePropertyFinder StylePropertyFinder => stylePropertyFinder.Value;

        [ImportingConstructor]
        public CreateMissingStyleFix(Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                     Lazy<IStylePropertyFinder> stylePropertyFinder,
                                     Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.stylePropertyFinder = stylePropertyFinder;
            this.textProviderService = textProviderService;
        }

        public override string Documentation => "When a static resource does not exists and the target type is a Style, this code fix launches the XAML style editor ";

        public override Type TargetCodeAnalyser => typeof(UndefinedStaticResourceAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.create_missing_style";

        public override string Name => "Create Missing Style";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax) as StaticResourceExpression;

            if (expression == null || expression.Value == null || !expression.Value.HasValue)
            {
                return false;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax);

            var returnType = SymbolHelper.ResolveMemberReturnType(symbol);

            if (!SymbolHelper.DerivesFrom(returnType, context.Platform.Style.MetaType))
            {
                return false;
            }

            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax) as StaticResourceExpression;

            return CreateSuggestion("Create a new style named " + expression.Value.Value).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var expression = context.XamlSemanticModel.GetExpression(syntax) as StaticResourceExpression;
            var targetTypeSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;

            var properties = StylePropertyFinder.FindCandidateStyleProperties(syntax.Parent, context.XamlSemanticModel, context.Platform);

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

                if (styleName != expression.Value.Value)
                {
                    workUnits.Add(new ReplaceTextWorkUnit()
                    {
                        Text = styleName,
                        Span = expression.Value.Span,
                        FilePath = document.FilePath,
                    });
                }

                if (syntax.Parent.HasAttributes)
                {
                    foreach (var styleProperty in styleProperties)
                    {
                        var attr = syntax.Parent.GetAttribute(a =>
                        {
                            if (!(SymbolHelper.FindMemberSymbolByName(targetTypeSymbol, a.Name.LocalName) is IPropertySymbol property))
                            {
                                return false;
                            }

                            return property.Name == styleProperty.Key;
                        });

                        if (attr != null)
                        {
                            var start = attr.Span.Start;
                            var end = attr.Span.End;

                            var index = syntax.Parent.Attributes.IndexOf(attr);
                            if (index - 1 >= 0)
                            {
                                start = syntax.Parent.Attributes[index - 1].Span.End;
                            }

                            workUnits.Add(new ReplaceTextWorkUnit()
                            {
                                Text = string.Empty,
                                Span = TextSpan.FromBounds(start, end),
                                FilePath = document.FilePath,
                            });
                        }
                    }
                }

                // Create the new style, locate the target file and insert it into it's resource dictionary.

                var styleSyntax = StyleGenerator.GenerateSyntax(platform, styleName, targetType, targetTypePrefix, parentStyleName, parentStyleType, styleProperties);

                XmlSyntaxTree xmlSyntaxTree = null;

                if (targetFilePath == document.FilePath || !File.Exists(targetFilePath))
                {
                    xmlSyntaxTree = document.GetSyntaxTree();

                    if (!File.Exists(targetFilePath))
                    {
                        log?.Info("The file " + targetFilePath + " does not exist, cannot extract the resource into it. Using the current file instead.");
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

            var xmlns = context.Namespaces.ResolveNamespace(syntax.Parent);

            return new XamlStyleEditorWorkUnit()
            {
                StyleName = expression.Value.Value,
                FilePath = document.FilePath,
                TargetType = targetTypeSymbol,
                TargetTypePrefix = xmlns.Prefix,
                ParentStyleType = ParentStyleType.BaseResourceKey,
                Project = context.Project,
                ApplyStyleDelegate = applyStyle,
                Platform = context.Platform,
                ShowAllProperties = false,
                Properties = properties,
                ApplyButtonLabel = "Create Style",
                TargetFiles = new List<IProjectFile>()
                {
                    file, appxaml
                },
            }.AsList();
        }
    }
}

