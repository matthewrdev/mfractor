using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using MFractor.Analytics;
using MFractor.Configuration;
using MFractor.Editor.Utilities;
using MFractor.Code;
using MFractor.Maui;
using MFractor.Maui.CodeGeneration.Commands;
using MFractor.Maui.CodeGeneration.CSharp;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using MFractor.Work;
using MFractor.Code.Formatting;
using MFractor.Workspace;
using MFractor.Code.WorkUnits;
using MFractor.Workspace.Utilities;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDataBindingCompletionService))]
    class DataBindingCompletionService : IXamlCompletionService, IDataBindingCompletionService
    {
        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        public IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IConfigurationEngine> configurationEngine;
        public IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        [ImportingConstructor]
        public DataBindingCompletionService(Lazy<IBindingContextResolver> bindingContextResolver,
                                            Lazy<IAnalyticsService> analyticsService,
                                            Lazy<IWorkspaceService> workspaceService,
                                            Lazy<IConfigurationEngine> configurationEngine,
                                            Lazy<ICodeFormattingPolicyService> formattingPolicyService)
        {
            this.bindingContextResolver = bindingContextResolver;
            this.analyticsService = analyticsService;
            this.workspaceService = workspaceService;
            this.configurationEngine = configurationEngine;
            this.formattingPolicyService = formattingPolicyService;
        }

        public string AnalyticsEvent => "DataBinding Shorthand Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var attribute = context.GetSyntax<XmlAttribute>();

            if (attribute == null)
            {
                return false;
            }

            if (!CompletionHelper.IsWithinAttributeValue(context, textView, triggerLocation))
            {
                return false;
            }

            var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;
            if (property == null)
            {
                return false;
            }

            if (attribute.HasValue)
            {
                if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
                {
                    return false;
                }

                if (attribute.Value.Value.Contains(","))
                {
                    return false;
                }
            }

            // Ignore AutomationId's as we can't data bind to them.
            if (property.Name == "AutomationId" && SymbolHelper.DerivesFrom(property.ContainingType, context.Platform.Element.MetaType))
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var completions = ProvideBindingCompletions(textView, context, triggerLocation, applicableToSpan, token);

            return completions;
        }

        IReadOnlyList<ICompletionSuggestion> ProvideBindingCompletions(ITextView textView, IXamlFeatureContext context, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();

            var attribute = context.GetSyntax<XmlAttribute>();
            var node = attribute.Parent;

            var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;

            if (BindingContextResolver.ResolveBindingContext(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, node) is INamedTypeSymbol bindingContext)
            {
                items.AddRange(ProvideBindingCodeActionCompletions(context, textView.TextBuffer, property, bindingContext));
                items.AddRange(ProvideBindingContextCompletions(context, bindingContext, true));
            }

            return items;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideBindingContextCompletions(IXamlFeatureContext context, INamedTypeSymbol bindingContext, bool isShorthandMode)
        {
            var items = new List<ICompletionSuggestion>();

            var publicMembers = SymbolHelper.GetAllMemberSymbols<IPropertySymbol>(bindingContext).Where(p => p.DeclaredAccessibility.HasFlag(Microsoft.CodeAnalysis.Accessibility.Public)).ToList();

            foreach (var pm in publicMembers)
            {
                var insertion = isShorthandMode ? "{" + context.Platform.BindingExtension.MarkupExpressionName + " " + pm.Name + "}" : pm.Name;

                var completion = new CompletionSuggestion($"{pm.Name} ({pm.Type.Name} from {bindingContext.Name})", insertion);

                var tooltip = pm.Name + " (" + pm.Type + ") from the binding context '" + bindingContext.ToString() + "'\n\nInserts:\n" + insertion;
                completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip);

                items.Add(completion);
            }

            return items;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideBindingCodeActionCompletions(IXamlFeatureContext context,
                                                                                      ITextBuffer textBuffer,
                                                                                      IPropertySymbol property,
                                                                                      INamedTypeSymbol bindingContext)
        {
            var items = new List<ICompletionSuggestion>();

            var bindablePropertyName = property.Name + "Property";
            var bindableProperty = SymbolHelper.FindMemberSymbolByName(property.ContainingType, bindablePropertyName);

            if (bindableProperty != null
                && bindingContext != null
                && property.Type is INamedTypeSymbol propertyType
                && bindingContext.DeclaringSyntaxReferences.Any())
            {
                if (bindingContext.DeclaringSyntaxReferences.Any())
                {
                    var generateProperty = new CompletionSuggestion("Generate New Property", string.Empty);
                    generateProperty.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Generate a new property in " + bindingContext);
                    generateProperty.AddProperty(XamlCompletionItemPropertyKeys.CompletionAction, new CompletionAction((tv, t, ci) =>
                    {
                        return new TextInputWorkUnit("Create Property", "Enter Property Name", "", "Generate", "Cancel", (input) =>
                        {
                            var span = TextEditorHelper.GetAttributeSpanAtOffset(textBuffer, tv.GetCaretOffset());

                            var textWorkUnit = new ReplaceTextWorkUnit()
                            {
                                Span = span,
                                Text = "{" + context.Platform.BindingExtension.MarkupExpressionName + " " + input + "}",
                                FilePath = context.Document.FilePath,
                            };

                            var configId = ConfigurationId.Create(context.Project.GetIdentifier());
                            var workspace = WorkspaceService.CurrentWorkspace;
                            var formatting = FormattingPolicyService.GetFormattingPolicy(context.Project);

                            var insertion = ConfigurationEngine.Resolve<IViewModelPropertyGenerator>(configId).GenerateSyntax(property.Type.ToString(), Microsoft.CodeAnalysis.Accessibility.Public, input, null);

                            var syntax = bindingContext.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax();

                            var targetProject = context.Solution.Projects.FirstOrDefault(p => p.AssemblyName == bindingContext.ContainingAssembly.Name);

                            var workUnit = new InsertSyntaxNodesWorkUnit
                            {
                                HostNode = syntax,
                                SyntaxNodes = insertion.ToList(),
                                Workspace = workspace,
                                Project = targetProject
                            };

                            var fileName = Path.GetFileName(syntax.SyntaxTree.FilePath);

                            AnalyticsService.Track("Generate Property Binding Completion");

                            return new List<IWorkUnit>()
                            {
                                textWorkUnit,
                                workUnit,
                            };
                        }).AsList();
                    }));

                    items.Add(generateProperty);

                    if (SymbolHelper.DerivesFrom(propertyType, "System.Windows.Input.ICommand"))
                    {
                        var generateCommand = new CompletionSuggestion("Generate New Command", string.Empty);
                        generateCommand.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Generate a new command implementation in " + bindingContext);
                        generateCommand.AddProperty(XamlCompletionItemPropertyKeys.CompletionAction, new CompletionAction((tv, t, ci) =>
                        {
                            return new TextInputWorkUnit("Create Command", "Enter Command Name", "", "Generate", "Cancel", (input) =>
                            {
                                var span = TextEditorHelper.GetAttributeSpanAtOffset(textBuffer, tv.GetCaretOffset());

                                var textWorkUnit = new ReplaceTextWorkUnit()
                                {
                                    Span = span,
                                    Text = "{" + context.Platform.BindingExtension.MarkupExpressionName + " " + input + "}",
                                    FilePath = context.Document.FilePath,
                                };


                                var configId = ConfigurationId.Create(context.Project.GetIdentifier());
                                var workspace = WorkspaceService.CurrentWorkspace;
                                var formatting = FormattingPolicyService.GetFormattingPolicy(context.Project);

                                var insertion = ConfigurationEngine.Resolve<ICommandImplementationGenerator>(configId).GenerateSyntax(input, context.Platform.Command.MetaType);

                                var syntax = bindingContext.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax();

                                var targetProject = context.Solution.Projects.FirstOrDefault(p => p.AssemblyName == bindingContext.ContainingAssembly.Name);

                                var workUnit = new InsertSyntaxNodesWorkUnit
                                {
                                    HostNode = syntax,
                                    SyntaxNodes = insertion.ToList(),
                                    Workspace = workspace,
                                    Project = targetProject
                                };

                                AnalyticsService.Track("Generate Command Binding Completion");

                                return new List<IWorkUnit>()
                                {
                                    textWorkUnit,
                                    workUnit,
                                };
                            }).AsList();
                        }));
                        items.Add(generateCommand);
                    }
                }
            }

            return items;
        }
    }
}
