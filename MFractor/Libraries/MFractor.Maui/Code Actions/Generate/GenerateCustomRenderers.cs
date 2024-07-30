using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Maui.CodeGeneration.CustomRenderers;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions.Generate
{
    class GenerateCustomRenderers : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.generate_custom_renderers";

        public override string Name => "Generate Custom Renderers";

        public override string Documentation => "Generates a platform specific custom renderer for a control. This code action will resolve the correct control baseclass on each platform and then create a custom render in each iOS or Android project.";

        [Import]
        public ICustomRendererGenerator CustomRendererGenerator { get; set; }

        [Import]
        public IXamlPlatformRepository XamlPlatforms { get; set; }

        protected override bool IsAvailableInDocument(IParsedCSharpDocument document, IFeatureContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var classSyntax = syntax as ClassDeclarationSyntax;
            if (classSyntax == null)
            {
                return false;
            }

            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            var model = compilation.GetSemanticModel(document.GetSyntaxTree());
            if (model == null)
            {
                return false;
            }

            var platform = XamlPlatforms.ResolvePlatform(context.Project, compilation);

            var symbol = model.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

            return CustomRendererGenerator.CanGenerateCustomRendererForType(symbol, platform);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var classSyntax = syntax as ClassDeclarationSyntax;
            context.Project.TryGetCompilation(out var compilation);

            var model = compilation.GetSemanticModel(document.GetSyntaxTree());

            var symbol = model.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

            return CreateSuggestion("Generate custom renderers for " + symbol.Name).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax,
                                                       IParsedCSharpDocument document,
                                                       IFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            var platform = XamlPlatforms.ResolvePlatform(context.Project);
            var classSyntax = syntax as ClassDeclarationSyntax;
            context.Project.TryGetCompilation(out var compilation);

            var model = compilation.GetSemanticModel(document.GetSyntaxTree());

            var symbol = model.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

            IReadOnlyList<IWorkUnit> generateRenderers(GenerateCodeFilesResult result)
            {
                var workUnits = new List<IWorkUnit>();

                foreach (var p in result.SelectedProjects)
                {
                    workUnits.AddRange(CustomRendererGenerator.Generate(symbol, symbol.BaseType, p, platform,  result.FolderPath, result.Name));
                }

                return workUnits;
            }

            var solution = context.Project.Solution;

            var iosProjects = solution.Projects.Where(p => p.IsAppleUnifiedProject());
            var androidProjects = solution.Projects.Where(p => p.IsAndroidProject());

            var choices = new List<Project>();
            choices.AddRange(iosProjects);
            choices.AddRange(androidProjects);

            var rendererName = symbol.Name + "Renderer";

            return new GenerateCodeFilesWorkUnit(rendererName,
                                                       null,
                                                       choices,
                                                       CustomRendererGenerator.RenderersFolder,
                                                       "Generate Custom Renderers",
                                                       "Choose projects to generate this controls custom renderers into.",
                                                       "https://docs.mfractor.com/xamarin-forms/custom-renderers/generate-custom-renderers/",
                                                       ProjectSelectorMode.Multiple,
                                                       generateRenderers).AsList();
        }
    }
}
