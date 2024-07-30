using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Editor.Tooltips;
using MFractor.Editor.Utilities;
using MFractor.Fonts;
using MFractor.Fonts.Tooltips;
using MFractor.Fonts.Utilities;
using MFractor.Images;
using MFractor.Localisation;
using MFractor.Localisation.Tooltips;
using MFractor.Maui;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Fonts;
using MFractor.Maui.FontSizes;
using MFractor.Maui.Mvvm;
using MFractor.Maui.Semantics;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Tooltips;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Navigation;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace MFractor.Editor.XAML.Tooltips
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export]
    class XamlTooltipsQuickInfoSource : IAsyncQuickInfoSource
    {
        readonly Logging.ILogger log = Logging.Logger.Create();
        
        readonly Lazy<IXamlFeatureContextService> featureContextService;
        public IXamlFeatureContextService FeatureContextService => featureContextService.Value;

        readonly Lazy<IXamlSymbolResolver> symbolResolver;
        public IXamlSymbolResolver SymbolResolver => symbolResolver.Value;

        readonly Lazy<IBindingContextResolver> bindingContextResolver;
        public IBindingContextResolver BindingContextResolver => bindingContextResolver.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IMvvmResolver> mvvmResolver;
        public IMvvmResolver MvvmResolver => mvvmResolver.Value;

        readonly Lazy<IFontAssetResolver> fontAssetResolver;
        public IFontAssetResolver FontAssetResolver => fontAssetResolver.Value;

        readonly Lazy<IFontFamilyResolver> fontFamilyResolver;
        public IFontFamilyResolver FontFamilyResolver => fontFamilyResolver.Value;

        readonly Lazy<IFontSizeConfigurationService> fontSizeConfigurationService;
        public IFontSizeConfigurationService FontSizeConfigurationService => fontSizeConfigurationService.Value;

        readonly Lazy<IAnalysisResultStore> analysisResultStore;
        public IAnalysisResultStore AnalysisResultStore => analysisResultStore.Value;

        readonly Lazy<IDynamicResourceTooltipRenderer> dynamicResourceTooltipRenderer;
        public IDynamicResourceTooltipRenderer DynamicResourceTooltipRenderer => dynamicResourceTooltipRenderer.Value;

        readonly Lazy<IStaticResourceTooltipRenderer> staticResourceTooltipRenderer;
        public IStaticResourceTooltipRenderer StaticResourceTooltipRenderer => staticResourceTooltipRenderer.Value;

        readonly Lazy<IGridTooltipRenderer> gridTooltipRenderer;
        public IGridTooltipRenderer GridTooltipRenderer => gridTooltipRenderer.Value;

        readonly Lazy<IFontSummaryTooltipRenderer> fontSummaryTooltipRenderer;
        public IFontSummaryTooltipRenderer FontSummaryTooltipRenderer => fontSummaryTooltipRenderer.Value;

        readonly Lazy<INavigationService> navigationService;
        public INavigationService NavigationService => navigationService.Value;

        readonly Lazy<ILocalisationTooltipRenderer> localisationTooltipRenderer;        public ILocalisationTooltipRenderer LocalisationTooltipRenderer => localisationTooltipRenderer.Value;

        readonly Lazy<ICodeActionEngine> codeActionEngine;        public ICodeActionEngine CodeActionEngine => codeActionEngine.Value;

        ITextBuffer textBuffer;
        string filePath;

        [ImportingConstructor]
        public XamlTooltipsQuickInfoSource(Lazy<IXamlFeatureContextService> featureContextService,
                                           Lazy<IXamlSymbolResolver> symbolResolver,
                                           Lazy<IBindingContextResolver> bindingContextResolver,
                                           Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                           Lazy<IAnalyticsService> analyticsService,
                                           Lazy<IMvvmResolver> mvvmResolver,
                                           Lazy<IFontAssetResolver> fontAssetResolver,
                                           Lazy<IFontFamilyResolver> fontFamilyResolver,
                                           Lazy<IFontSizeConfigurationService> fontSizeConfigurationService,
                                           Lazy<IAnalysisResultStore> analysisResultStore,
                                           Lazy<IDynamicResourceTooltipRenderer> dynamicResourceTooltipRenderer,
                                           Lazy<IStaticResourceTooltipRenderer> staticResourceTooltipRenderer,
                                           Lazy<IGridTooltipRenderer> gridTooltipRenderer,
                                           Lazy<IFontSummaryTooltipRenderer> fontSummaryTooltipRenderer,
                                           Lazy<INavigationService> navigationService,
                                           Lazy<ILocalisationTooltipRenderer> localisationTooltipRenderer,
                                           Lazy<ICodeActionEngine> codeActionEngine)
        {
            this.featureContextService = featureContextService;
            this.symbolResolver = symbolResolver;
            this.bindingContextResolver = bindingContextResolver;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.analyticsService = analyticsService;
            this.mvvmResolver = mvvmResolver;
            this.fontAssetResolver = fontAssetResolver;
            this.fontFamilyResolver = fontFamilyResolver;
            this.fontSizeConfigurationService = fontSizeConfigurationService;
            this.analysisResultStore = analysisResultStore;
            this.dynamicResourceTooltipRenderer = dynamicResourceTooltipRenderer;
            this.staticResourceTooltipRenderer = staticResourceTooltipRenderer;
            this.gridTooltipRenderer = gridTooltipRenderer;
            this.fontSummaryTooltipRenderer = fontSummaryTooltipRenderer;
            this.navigationService = navigationService;
            this.localisationTooltipRenderer = localisationTooltipRenderer;
            this.codeActionEngine = codeActionEngine;
        }

        public void SetTextBuffer(ITextBuffer textBuffer, string filePath)
        {
            this.textBuffer = textBuffer;
            this.filePath = filePath;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session,
                                                         CancellationToken cancellationToken)
        {
            try
            {
                var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);

                var project = TextBufferHelper.GetCompilationProject(session.TextView.TextBuffer);

                if (project == null || !project.TryGetCompilation(out var compilation))
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var offset = triggerPoint.Value.Position;

                var context = (FeatureContextService.Retrieve(filePath)
                               ?? FeatureContextService.CreateFeatureContext(project, filePath, offset)) as IXamlFeatureContext;

                if (context == null || !triggerPoint.HasValue)
                {
                    Debug.WriteLine("Failed to retrieve the feature context for tooltips");
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var issues = AnalysisResultStore.Retrieve(filePath);

                if (issues.Any(i => i.Span.IntersectsWith(triggerPoint.Value.Position)))
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                context.Syntax = FeatureContextService.GetSyntaxAtLocation(context.SyntaxTree, offset);

                var symbol = SymbolResolver.Resolve(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, offset);

                if (symbol == null)
                {
                    Debug.WriteLine("Failed to resolve symbol at position");
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var span = textBuffer.CurrentSnapshot.CreateTrackingSpan(Span.FromBounds(symbol.Span.Start, symbol.Span.End), SpanTrackingMode.EdgeInclusive);

                if (symbol.SymbolKind == XamlSymbolKind.Color)
                {
                    var color = symbol.GetSymbol<Color>();

                    AnalyticsService.Track("Color Preview Tooltip");

                    return Task.FromResult(new QuickInfoItem(span, new ColorTooltipModel(color)));

                }
                else if (symbol.SymbolKind == XamlSymbolKind.Image)
                {
                    var image = symbol.GetSymbol<IImageAsset>();
                    AnalyticsService.Track("Image Preview Tooltip");
                    return Task.FromResult(new QuickInfoItem(span, new ImageTooltipModel(image)));
                }
                else if (symbol.SymbolKind == XamlSymbolKind.Svg)
                {
                    // TODO: Repair SVG tooltip
                }

                return CreateTooltipAsync(context, symbol, triggerPoint.Value.Position);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return Task.FromResult<QuickInfoItem>(null);
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }

        async Task<QuickInfoItem> CreateTooltipAsync(IXamlFeatureContext context, XamlSymbolInfo result, int position)
        {
            try
            {
                var span = textBuffer.CurrentSnapshot.CreateTrackingSpan(Span.FromBounds(result.Span.Start, result.Span.End), SpanTrackingMode.EdgeInclusive);

                var tooltip = string.Empty;
                var analyticsEvent = string.Empty;
                var analyticsTraits = new Dictionary<string, string>();

                var interactionLocation = new InteractionLocation(position);
                var navigationContext = new NavigationContext(filePath, context.Project, position, interactionLocation);

                var suggestion = await NavigationService.Suggest(navigationContext);

                switch (result.SymbolKind)
                {
                    case XamlSymbolKind.Symbol:
                        return await CreateSymbolTooltip(context, result, span, position);
                    case XamlSymbolKind.Expression:
                        break;
                    case XamlSymbolKind.Font:
                        {
                            if (result.Symbol is IFont font)
                            {
                                tooltip = FontSummaryTooltipRenderer.Render(font);
                                analyticsEvent = "Embedded Font Tooltip";
                            }
                        }
                        break;
                    case XamlSymbolKind.StaticResource:
                        {
                            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);

                            if (database != null)
                            {
                                var colorRepo = database.GetRepository<ColorDefinitionRepository>();
                                var stringRepo = database.GetRepository<StringResourceDefinitionRepository>();
                                var onPlatformRepo = database.GetRepository<OnPlatformDeclarationRepository>();
                                var thicknessRepo = database.GetRepository<ThicknessDefinitionRepository>();

                                if (result.AdditionalData is StaticResourceResult resource && resource.Definition != null)
                                {
                                    var colorDefinition = colorRepo.GetColorForStaticResourceDefinition(resource.Definition);
                                    var onPlatform = onPlatformRepo.GetOnPlatformDeclarationForStaticResourceDefinition(resource.Definition);
                                    var stringDefinition = stringRepo.GetStringForStaticResourceDefinition(resource.Definition);
                                    var thicknessDefinition = thicknessRepo.GetThicknessForStaticResourceDefinition(resource.Definition);

                                    tooltip = StaticResourceTooltipRenderer.CreateTooltip(resource.Definition, resource.Project, colorDefinition == null);

                                    if (onPlatform != null
                                        && onPlatform.Type == "System.String"
                                        && onPlatform.HasPlatforms
                                        && onPlatform.Platforms.TryGetValue("iOS", out var postscriptName))
                                    {
                                        var font = FontAssetResolver.GetFontAssetsWithPostscriptName(context.Project, postscriptName)?.FirstOrDefault();

                                        if (font != null)
                                        {
                                           // return new QuickInfoItem(span, new FontPreviewTooltipModel(font));
                                        }
                                    }
                                    else if (stringDefinition != null
                                             && stringDefinition.HasValue
                                             && result.Syntax is XmlAttribute attribute)
                                    {
                                        var content = FontGlyphCodeHelper.EscapedUnicodeCharacterToGlyphCharacter(stringDefinition.Value);

                                        if (!string.IsNullOrEmpty(content))
                                        {
                                            var fontFamily = FontFamilyResolver.ResolveFont(attribute.Parent, context.XamlSemanticModel, context.Platform, context.Document.ProjectFile);

                                            if (fontFamily != null)
                                            {
                                                return new QuickInfoItem(span, new FontGlyphTooltipModel(fontFamily, content));
                                            }
                                        }
                                    }
                                    else if (thicknessDefinition != null)
                                    {
                                        return new QuickInfoItem(span, new ThicknessTooltipModel(thicknessDefinition.Left, thicknessDefinition.Right, thicknessDefinition.Top, thicknessDefinition.Bottom)
                                        {
                                            Content = tooltip,
                                            NavigationContext = navigationContext,
                                            NavigationSuggestion = suggestion,
                                        });
                                    }

                                    if (colorDefinition != null)
                                    {
                                        AnalyticsService.Track("Color Preview Tooltip (StaticResource)");

                                        return new QuickInfoItem(span, new ColorTooltipModel(colorDefinition.Color, tooltip)
                                        {
                                            NavigationContext = navigationContext,
                                            NavigationSuggestion = suggestion,
                                        });
                                    }
                                    else
                                    {
                                        analyticsEvent = "Static Resource Tooltip";
                                        analyticsTraits = new Dictionary<string, string>()
                                        {
                                            { "MetaType", resource.Definition.SymbolName}
                                        };
                                    }
                                }
                            }
                        }
                        break;
                    case XamlSymbolKind.DynamicResource:
                        {
                            tooltip = CreateDynamicResourceTooltip(context, result);
                            analyticsEvent = "Dynamic Resource Tooltip";
                        }
                        break;
                    case XamlSymbolKind.Syntax:
                        break;
                    case XamlSymbolKind.Svg:
                        break;
                    case XamlSymbolKind.Localisation:
                        {
                            if (result.Symbol is ILocalisationDeclarationCollection localisations)
                            {
                                tooltip = LocalisationTooltipRenderer.CreateLocalisationTooltip(localisations);
                            }
                        }
                        break;
                }

                var codeActions = CodeActionEngine.RetrieveCommonCodeActions(context, interactionLocation);
                if (string.IsNullOrEmpty(tooltip))
                {
                    if (codeActions is null || !codeActions.Any())
                    {
                        return default;
                    }
                }

                var quickInfoItem = new QuickInfoItem(span, new TextContentTooltipModel()
                {
                    Content = tooltip,
                    NavigationContext = navigationContext,
                    NavigationSuggestion = suggestion,
                    InteractionLocation = interactionLocation,
                    CodeActions = codeActions.ToList(),
                    FeatureContext = context,
                }.AsTextElement());

                if (!string.IsNullOrEmpty(analyticsEvent))
                {
                    AnalyticsService.Track(analyticsEvent, analyticsTraits);
                }

                return quickInfoItem;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return default;
        }

        string CreateDynamicResourceTooltip(IXamlFeatureContext context, XamlSymbolInfo result)
        {
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);

            if (database != null)
            {
                if (result.AdditionalData is DynamicResourceResult resource && resource.Definition != null)
                {
                    return DynamicResourceTooltipRenderer.CreateTooltip(resource.Definition.Name, context.Document.FilePath, resource.Project, context.Platform);
                }
            }

            return string.Empty;
        }

        async Task<QuickInfoItem> CreateSymbolTooltip(IXamlFeatureContext context, XamlSymbolInfo result, ITrackingSpan span, int position)
        {
            var tooltip = string.Empty;
            var analyticsEvent = string.Empty;
            var platform = context.Platform;

            var symbol = result.Symbol as ISymbol;

            var attribute = result.GetSyntax<XmlAttribute>();
            var attributeSYmbol = context.XamlSemanticModel.GetSymbol(attribute);
            var memberType = SymbolHelper.ResolveMemberReturnType(attributeSYmbol);

            var allowNavigation = false;
            if (result.Expression != null
                && result.Expression is BindingExpression binding)
            {
                var bindingContext = BindingContextResolver.ResolveBindingContext(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, binding, binding.ParentAttribute.Parent);

                if (bindingContext != null)
                {
                    allowNavigation = true;
                    tooltip += $"{platform.BindingContextProperty.SeparateUpperLettersBySpace()}\n{bindingContext.ToString()}";
                }
            }

            if (attribute != null && XamlSyntaxHelper.IsColorSymbol(attribute, context.XamlSemanticModel, context.Platform))
            {
                var formats = "Name" + " - A named color.";
                formats += "\n" + "#RRGGBB" + " - A color with 32 bit (00-FF) Red, Green and Blue channels.";
                formats += "\n" + "#AARRGGBB" + " - A color with 32 bit (00-FF) Alpha, Red, Green and Blue channels.";
                formats += "\n" + "#RGB" + " - A color with 16 bit (0-F) Red, Green and Blue channels.";
                formats += "\n" + "#ARGB" + " - A color with 16 bit (0-F) Alpha, Red, Green and Blue channels.";
                formats += "\n\n" + "When setting the alpha channel, 0 is fully transparent, F is fully opaque.";

                tooltip += "Color Formats\n" + formats;

                analyticsEvent = "Color Formats Tooltip";
            }

            var propertySymbol = attributeSYmbol as IPropertySymbol;

            var typeSymbol = symbol as INamedTypeSymbol;
            if (SymbolHelper.DerivesFrom(typeSymbol, platform.ValueConverter.MetaType))
            {
                if (FormsSymbolHelper.ResolveValueConverterConstraints(typeSymbol, out var converterInput, out var converterOutput, out var converterParameter))
                {
                    var conversion = "Input: " + converterInput.ToString();
                    conversion += "\n" + "Output: " + converterOutput.ToString();
                    if (converterParameter != null)
                    {
                        conversion += "\n" + "converterParameter: " + converterOutput.ToString();
                    }

                    tooltip += "Value Conversion\n" + conversion;
                }

                analyticsEvent = "Value Converter Tooltip";
            }
            else if (propertySymbol != null
                     && propertySymbol.Name == "FontSize"
                     && propertySymbol.Type.SpecialType == SpecialType.System_Double
                     && attribute.HasValue
                     && attribute.Value.Span.Contains(position)
                     && FontSizeConfigurationService.TryGetNamedFontSize(attribute.Value.Value, out var fontSize))
            {
                tooltip += $"Platform Values:\niOS={fontSize.IOS}\nAndroid={fontSize.Android}\nUWP={fontSize.UWP}";

                tooltip += $"\n\nOn iOS and Android, named font sizes will autoscale based on operating system accessibility options.";

                analyticsEvent = "Named FontSize Tooltip";
            }
            else if (propertySymbol != null
                     && SymbolHelper.DerivesFrom(propertySymbol.Type, platform.Thickness.MetaType)
                     && ThicknessHelper.ProcessThickness(attribute.Value?.Value, out var left, out var right, out var top, out var bottom))
            {
                return CreateThicknessTooltip(span, left, right, top, bottom);
            }
            else if (symbol is IFieldSymbol fieldSymbol
                     && fieldSymbol.IsConst
                     && fieldSymbol.HasConstantValue
                     && fieldSymbol.ConstantValue is string fieldValue
                     && !string.IsNullOrEmpty(fieldValue)
                     && fieldValue.Length == 1
                     && IsUnicode(fieldValue))
            {
                var fontFamily = FontFamilyResolver.ResolveFont(attribute.Parent, context.XamlSemanticModel, context.Platform, context.Document.ProjectFile);

                if (fontFamily is null)
                {

                }

                if (fontFamily != null)
                {
                    return new QuickInfoItem(span, new FontGlyphTooltipModel(fontFamily, fieldValue));
                }
            }
            else if (memberType != null && memberType.SpecialType == SpecialType.System_String)
            {
                if (attribute.HasValue)
                {
                    var fontFamily = FontFamilyResolver.ResolveFont(attribute.Parent, context.XamlSemanticModel, context.Platform, context.Document.ProjectFile);

                    if (fontFamily != null)
                    {
                        var value = context.GetSyntax<XmlAttribute>()?.Value?.Value;

                        var content = FontGlyphCodeHelper.EscapedUnicodeCharacterToGlyphCharacter(value);

                        if (!string.IsNullOrEmpty(content)
                                 && attribute.Value.Span.IntersectsWith(position))
                        {
                            return new QuickInfoItem(span, new FontGlyphTooltipModel(fontFamily, content));
                        }
                        else if (!string.IsNullOrEmpty(value)
                                 && !ExpressionParserHelper.IsExpression(value)
                                 && attribute.Value.Span.IntersectsWith(position))
                        {
                            return new QuickInfoItem(span, new FontPreviewTooltipModel(fontFamily, value));
                        }
                    }
                }
            }
            else if (SymbolHelper.DerivesFrom(typeSymbol, platform.Page.MetaType))
            {
                var pageSyntax = typeSymbol.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax();

                if (pageSyntax != null && pageSyntax.SyntaxTree != null)
                {
                    var bindingContext = MvvmResolver.ResolveViewModelSymbol(context.Project, pageSyntax.SyntaxTree.FilePath);
                    if (bindingContext != null)
                    {
                        tooltip += "Detected ViewModel\n" + bindingContext.ToString();

                        analyticsEvent = "Binding Context Tooltip";
                    }
                }
            }
            else if (SymbolHelper.DerivesFrom(SymbolHelper.ResolveMemberReturnType(symbol), platform.Thickness.MetaType))
            {
                var formats = BuildThicknesFormatsText();

                tooltip += formats;

                analyticsEvent = "Thickness Tooltip";
            }
            else if (symbol is IFieldSymbol layoutFlagsField
                     && layoutFlagsField.Name == "LayoutFlagsProperty"
                     && SymbolHelper.DerivesFrom(layoutFlagsField.ContainingType, platform.AbsoluteLayout.MetaType))
            {
                var format = "Views within an AbsoluteLayout are positioned using four values, structured as 'X,Y,Width,Height':\n\nX – the x (horizontal) position of the view's anchor\nY – the y (vertical) position of the view's anchor\nWidth – the width of the view\nHeight – the height of the view.\n\nWhen using proportional layout flags, these values must used as 0 to 1 values.";

                tooltip += "Format\n" + format;

                analyticsEvent = "AbsoluteLayout Flags Tooltip";
            }
            else if (attribute != null
                    && attribute.HasValue
                    && symbol is IFieldSymbol gridField
                    && SymbolHelper.DerivesFrom(gridField.ContainingType, platform.Grid.MetaType)
                    && SymbolHelper.DerivesFrom(context.XamlSemanticModel.GetSymbol(attribute.Parent?.Parent) as INamedTypeSymbol, platform.Grid.MetaType))
            {
                var grid = attribute.Parent.Parent;

                if (attribute.Name.FullName == $"{context.Platform.Grid.Name}.{context.Platform.RowProperty}")
                {
                    tooltip += $"{context.Platform.Grid.Name} {context.Platform.RowProperty}\n" + GridTooltipRenderer.CreateRowTooltip(attribute, grid, platform);
                    analyticsEvent = "Grid Row Tooltip";
                }
                else if (attribute.Name.FullName == $"{context.Platform.Grid.Name}.{context.Platform.RowProperty}Span")
                {
                    tooltip += $"{context.Platform.Grid.Name} {context.Platform.RowProperty} Span\n" + GridTooltipRenderer.CreateRowSpanTooltip(attribute, grid, platform);
                        analyticsEvent = "Grid RowSpan Tooltip";
                }
                else if (attribute.Name.FullName == $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}")
                {
                    tooltip += $"{context.Platform.Grid.Name} {context.Platform.ColumnProperty}\n" + GridTooltipRenderer.CreateColumnTooltip(attribute, grid, platform);
                        analyticsEvent = "Grid Column Tooltip";
                }
                else if (attribute.Name.FullName == $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}Span")
                { 
                        tooltip += $"{context.Platform.Grid.Name} {context.Platform.ColumnProperty} Span\n" + GridTooltipRenderer.CreateColumnSpanTooltip(attribute, grid, platform);
                        analyticsEvent = "Grid Column Span Tooltip";
                }
            }

            var interactionLocation = new InteractionLocation(position);

            var codeActions = CodeActionEngine.RetrieveCommonCodeActions(context, interactionLocation);
            if (string.IsNullOrEmpty(tooltip))
            {
                if (codeActions is null || !codeActions.Any())
                {
                    return default;
                }
            }

            if (!string.IsNullOrEmpty(analyticsEvent))
            {
                AnalyticsService.Track(analyticsEvent);
            }

            NavigationContext navigationContext = null;
            INavigationSuggestion suggestion = null;
            if (allowNavigation)
            {
                navigationContext = new NavigationContext(filePath, context.Project, position, new InteractionLocation(position));

                suggestion = await NavigationService.Suggest(navigationContext);
            }

            var node = result.GetSyntax<XmlNode>();

            if (node != null)
            {
                tooltip = AddGridRowColumnInformation(tooltip, node, context.XamlSemanticModel, context.Platform);
            }

            // TODO: TextContentTooltipModel needs to be converted into a ClassifiedTextElement
            var quickInfoItem = new QuickInfoItem(span, new TextContentTooltipModel()
            {
                Content = tooltip,
                NavigationContext = navigationContext,
                NavigationSuggestion = suggestion,
                InteractionLocation = interactionLocation,
                FeatureContext = context,
                CodeActions = codeActions.ToList(),
            }.AsTextElement());

            return quickInfoItem;
        }

        string BuildThicknesFormatsText()
        {
            var formats = "size" + " - A thickness where all dimensions are the same size.";
            formats += "\n" + "horizontal,vertical" + " - A thickness with a horizontal (left/right) and vertical (top/bottom) size.";
            formats += "\n" + "left,top,right,bottom" + " - A thickness defined by left, top, right, and bottom.";

            formats = "Formats:\n" + formats;
            return formats;
        }

        QuickInfoItem CreateThicknessTooltip(ITrackingSpan span, double left, double right, double top, double bottom)
        {
            AnalyticsService.Track("Thickness Preview Tooltip");

            var model = new ThicknessTooltipModel(left, right, top, bottom)
            {
                Content = BuildThicknesFormatsText()
            };
            var quickInfoItem = new QuickInfoItem(span, model);

            return quickInfoItem;
        }

        public static bool IsUnicode(string input)
        {
            var asciiBytesCount = Encoding.ASCII.GetByteCount(input);
            var unicodBytesCount = Encoding.UTF8.GetByteCount(input);
            return asciiBytesCount != unicodBytesCount;
        }

        string AddGridRowColumnInformation(string tooltip, XmlNode node, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            var nodeType = semanticModel.GetSymbol(node) as INamedTypeSymbol;
            if (SymbolHelper.DerivesFrom(nodeType, platform.RowDefinition.MetaType))
            {
                var rows = node.Parent.Children.Where(c =>
                {
                    var type = semanticModel.GetSymbol(c) as INamedTypeSymbol;
                    return SymbolHelper.DerivesFrom(type, platform.RowDefinition.MetaType);
                }).ToList();

                var index = rows.IndexOf(node);

                tooltip += $"{platform.Grid.Name} {platform.RowProperty}: " + index;

            }
            else if (SymbolHelper.DerivesFrom(nodeType, platform.ColumnDefinition.MetaType))
            {
                var rows = node.Parent.Children.Where(c =>
                {
                    var type = semanticModel.GetSymbol(c) as INamedTypeSymbol;
                    return SymbolHelper.DerivesFrom(type, platform.ColumnDefinition.MetaType);
                }).ToList();

                var index = rows.IndexOf(node);

                tooltip += $"{platform.Grid.Name} {platform.ColumnProperty}: " + index;
            }

            return tooltip;
        }
    }
}