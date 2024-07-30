using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.CodeGeneration.ValueConversion;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions
{
    class GenerateMissingClassAsValueConverterFix : FixCodeAction
	{
        public override Type TargetCodeAnalyser => typeof(Analysis.XamlNodeDoesNotResolveAnalysis);

        public override string Documentation => "When a Xaml node cannot be resolved and it ends with '[cC]onverter', this fix will create a new implemenation of IValueConverter.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.generate_missing_class_as_value_converter";

        public override string Name => "Generate Value Converter From XAML Node";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IValueConverterGenerator ValueConverterGenerator { get; set; }

        [Import]
        public IValueConversionSettings ValueConversionSettings { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
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

            var className = syntax.Name.LocalName;
            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                XamlSyntaxHelper.ExplodePropertySetter(syntax, out className, out var propertyName);
            }

            if (!className.EndsWith("converter", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);
            var className = syntax.Name.LocalName;
            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                XamlSyntaxHelper.ExplodePropertySetter(syntax, out className, out _);
            }

			return CreateSuggestion($"Generate a value converter named '{className}' in '{xamlNamespace.AssemblyComponent.AssemblyName}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
			var className = syntax.Name.LocalName;
			if (XamlSyntaxHelper.IsPropertySetter(syntax))
			{
                XamlSyntaxHelper.ExplodePropertySetter(syntax, out className, out _);
            }

            var @namespace = ValueConversionSettings.CreateConvertersClrNamespace(context.Project.GetIdentifier(), null);

            return ValueConverterGenerator.Generate(className, context.Project.GetIdentifier(), context.Platform, ValueConversionSettings.Folder, @namespace, default);
		}
	}
}
