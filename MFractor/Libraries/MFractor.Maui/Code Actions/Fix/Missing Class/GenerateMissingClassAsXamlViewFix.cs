using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Maui.Configuration;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions
{
    class GenerateMissingClassAsXamlViewFix : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(Analysis.XamlNodeDoesNotResolveAnalysis);

        public override string Documentation => "When a Xaml node cannot be resolved, this fix will create a new view that uses XAML and a code-behind class.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.generate_missing_class_as_xaml_view";

        public override string Name => "Generate XAML View From XAML Node";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IXamlViewWithCodeBehindGenerator XamlViewWithCodeBehindGenerator { get; set; }

        [Import]
        public ICustomControlsConfiguration CustomControlsConfiguration { get; set; }

        protected override bool CanExecute(ICodeIssue issue,
                                           XmlNode syntax,
                                           IParsedXamlDocument document,
                                           IXamlFeatureContext context,
                                           InteractionLocation location)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (typeSymbol is null)
            {
                return false;
            }

            var assemblyIsWriteable = typeSymbol.ContainingAssembly.Locations.Any((arg) => arg.Kind == Microsoft.CodeAnalysis.LocationKind.SourceFile);

            if (!assemblyIsWriteable)
            {
                return false;
            }

            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue,
                                                                     XmlNode syntax,
                                                                     IParsedXamlDocument document,
                                                                     IXamlFeatureContext context,
                                                                     InteractionLocation location)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            return CreateSuggestion($"Generate a XAML view named '{syntax.Name.LocalName}' in '{typeSymbol.ContainingAssembly.Name}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,
                                                          XmlNode syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context,
                                                          ICodeActionSuggestion suggestion,
                                                          InteractionLocation location)
        {
            var type = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            IReadOnlyList<IWorkUnit> callback(GenerateCodeFilesResult result)
            {
                var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);

                return XamlViewWithCodeBehindGenerator.Generate(result.Name,
                                                                type.ContainingNamespace.ToString(),
                                                                xamlNamespace.Prefix,
                                                                result.SelectedProject,
                                                                context.Platform,
                                                                result.FolderPath,
                                                                context.Platform.View.MetaType);
            }

            var className = syntax.Name.LocalName;
            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                XamlSyntaxHelper.ExplodePropertySetter(syntax, out className, out var propertyName);
            }

            var workUnit = new GenerateCodeFilesWorkUnit(className,
                                                         context.Project,
                                                         new List<Project>() { context.Project },
                                                         CustomControlsConfiguration.ControlsFolder,
                                                         "Generate XAML Control",
                                                         "Enter the name of the new control",
                                                         string.Empty,
                                                         ProjectSelectorMode.Single,
                                                         callback);

            return workUnit.AsList();
        }
    }
}
