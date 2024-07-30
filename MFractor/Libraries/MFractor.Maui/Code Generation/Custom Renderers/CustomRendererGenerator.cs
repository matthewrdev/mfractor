using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.CSharp.CodeGeneration;
using MFractor.Logging;
using MFractor.Maui.Configuration;
using MFractor.Maui.CustomRenderers;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.CustomRenderers
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICustomRendererGenerator))]
    class CustomRendererGenerator : CodeGenerator, ICustomRendererGenerator
    {
        [ImportingConstructor]
        public CustomRendererGenerator(Lazy<ICustomRendererTypeMappingService> customRendererTypeMappingService,
                                       Lazy<IConfigurationEngine> configurationEngine)
        {
            this.customRendererTypeMappingService = customRendererTypeMappingService;
            this.configurationEngine = configurationEngine;
        }

        readonly ILogger log = Logging.Logger.Create();

        [Import]
        public IIOSCustomRendererConfiguration IOSCustomRendererConfiguration { get; set; }

        [Import]
        public IAndroidCustomRendererConfiguration AndroidCustomRendererConfiguration { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [ExportProperty("What is the folder path to place new renderers into?")]
        public string RenderersFolder { get; set; } = "Renderers";

        readonly Lazy<ICustomRendererTypeMappingService> customRendererTypeMappingService;
        protected ICustomRendererTypeMappingService CustomRendererTypeMappingService => customRendererTypeMappingService.Value;

        readonly Lazy<IConfigurationEngine> configurationEngine;
        protected IConfigurationEngine ConfigurationEngine => configurationEngine.Value;


        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.generate_control_native_renderer";

        public override string Name => "Generate Custom Renderers For Control";

        public override string Documentation => "For a given user control in C#, the **Generate Custom Renderer** code action enables you to generate a custom renderer for that control in each iOS and Android project in your solution.";

        public bool CanGenerateCustomRendererForType(INamedTypeSymbol type, IXamlPlatform platform)
        {
            var baseType = type.BaseType;

            if (SymbolHelper.DerivesFrom(baseType, platform.Page.MetaType)
                || SymbolHelper.DerivesFrom(baseType, platform.Layout.MetaType)
                || SymbolHelper.DerivesFrom(baseType, platform.View.MetaType)
                || SymbolHelper.DerivesFrom(baseType, platform.ViewCell.MetaType))
            {
                return true;
            }

            return false;
        }

        public IReadOnlyList<IWorkUnit> Generate(INamedTypeSymbol type,
                                               INamedTypeSymbol baseType,
                                               Project targetProject,
                                               IXamlPlatform platform, 
                                               string rendererName,
                                               string folderPath)
        {
            ICodeSnippet snippet = null;

            var platformFramework = targetProject.IsAndroidProject() ? PlatformFramework.Android : PlatformFramework.iOS;

            if (SymbolHelper.DerivesFrom(baseType, platform.Page.MetaType))
            {
                snippet = GetPageRendererSnippet(type, baseType, targetProject);
            }
            else if (SymbolHelper.DerivesFrom(baseType, platform.Layout.MetaType))
            {
                snippet = GetLayoutRendererSnippet(type, baseType, targetProject);

            }
            else if (SymbolHelper.DerivesFrom(baseType, platform.View.MetaType))
            {
                snippet = GetViewRendererSnippet(type, baseType, targetProject);

            }
            else if (SymbolHelper.DerivesFrom(baseType, platform.Cell.MetaType))
            {
                snippet = GetCellRendererSnippet(type, baseType, targetProject);
            }

            if (snippet == null)
            {
                log?.Warning("An error occurred while trying to get the correct code snippet for " + type.Name);
                return Array.Empty<IWorkUnit>();
            }

            var isBackwardsCompatible = false;
            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(targetProject, folderPath);

            var rendererBaseType = CustomRendererTypeMappingService.GetCustomRendererForType(baseType, platform, platformFramework, isBackwardsCompatible, out var rendererControlType);

            if (string.IsNullOrEmpty(rendererBaseType))
            {
                log?.Warning("The base class " + baseType + " could not resolve to a custom renderer");
                return Array.Empty<IWorkUnit>();
            }

            snippet.SetArgumentValue("control_type", type.ToString());
            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.BaseType, rendererBaseType);
            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, @namespace);
            snippet.SetArgumentValue(RendererCodeSnippetArguments.RendererName, rendererName);
            snippet.SetArgumentValue(RendererCodeSnippetArguments.RendererControl, rendererControlType);
            snippet.SetArgumentValue(RendererCodeSnippetArguments.ControlName, type.Name);

            var filePath = (string.IsNullOrEmpty(folderPath) ? rendererName : Path.Combine(folderPath, rendererName)) + ".cs";

            var workUnit = new CreateProjectFileWorkUnit()
            {
                FilePath = filePath,
                TargetProject = targetProject,
                FileContent = snippet.ToString(),
                AllowPostProcessing = true,
                InferWhenInSharedProject = true,
                ShouldOverWrite = true,

            };

            return workUnit.AsList();
        }

        bool ApplyTargetProjectConfiguration(Project targetProject)
        {
            var targetConfigId = ConfigurationId.Create(targetProject.GetIdentifier());

            if (targetProject.IsAndroidProject())
            {
                ApplyPropertyConfiguration(nameof(AndroidCustomRendererConfiguration), targetConfigId);
            }
            else if (targetProject.IsAppleUnifiedProject())
            {
                ApplyPropertyConfiguration(nameof(IOSCustomRendererConfiguration), targetConfigId);
            }
            else
            {
                log?.Warning("An unsupported project type was supplied to the custom renderer generator. Currently only iOS and Android custom renderers are supported.");
                return false;
            }

            return true;
        }

        public ICodeSnippet GetLayoutRendererSnippet(INamedTypeSymbol type, INamedTypeSymbol baseType, Project targetProject)
        {
            if (!ApplyTargetProjectConfiguration(targetProject))
            {
                return null;
            }

            if (targetProject.IsAndroidProject())
            {
                return AndroidCustomRendererConfiguration.LayoutRendererSnippet;
            }
            else if (targetProject.IsAppleUnifiedProject())
            {
                return IOSCustomRendererConfiguration.LayoutRendererSnippet;
            }

            return null;
        }

        public ICodeSnippet GetPageRendererSnippet(INamedTypeSymbol type, INamedTypeSymbol baseType, Project targetProject)
        {
            if (!ApplyTargetProjectConfiguration(targetProject))
            {
                return null;
            }

            if (targetProject.IsAndroidProject())
            {
                return AndroidCustomRendererConfiguration.PageRendererSnippet;
            }
            else if (targetProject.IsAppleUnifiedProject())
            {
                return IOSCustomRendererConfiguration.PageRendererSnippet;
            }

            return null;
        }

        public ICodeSnippet GetCellRendererSnippet(INamedTypeSymbol type, INamedTypeSymbol baseType, Project targetProject)
        {
            if (!ApplyTargetProjectConfiguration(targetProject))
            {
                return null;
            }

            if (targetProject.IsAndroidProject())
            {
                return AndroidCustomRendererConfiguration.CellRendererSnippet;
            }
            else if (targetProject.IsAppleUnifiedProject())
            {
                return IOSCustomRendererConfiguration.CellRendererSnippet;
            }

            return null;
        }

        public ICodeSnippet GetViewRendererSnippet(INamedTypeSymbol type, INamedTypeSymbol baseType, Project targetProject)
        {
            if (!ApplyTargetProjectConfiguration(targetProject))
            {
                return null;
            }

            if (targetProject.IsAndroidProject())
            {
                return AndroidCustomRendererConfiguration.ViewRendererSnippet;
            }
            else if (targetProject.IsAppleUnifiedProject())
            {
                return IOSCustomRendererConfiguration.ViewRendererSnippet;
            }

            return null;
        }
    }
}