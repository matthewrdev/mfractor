using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace MFractor.Maui.CodeActions
{
    class GenerateMissingClassAsCSharpViewFix : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(Analysis.XamlNodeDoesNotResolveAnalysis);

        public override string Documentation => "When a Xaml node cannot be resolved, this fix will create a new view declared in C#.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.generate_missing_class_as_csharp_view";

        public override string Name => "Generate .NET View From XAML Node";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator { get; set; }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

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

            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);

            return CreateSuggestion($"Generate a C# view named '{syntax.Name.LocalName}' in '{xamlNamespace.AssemblyComponent.AssemblyName}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var typeSymbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            var options = FormattingPolicyService.GetFormattingPolicy(context);

            var className = syntax.Name.LocalName;
            if (XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                XamlSyntaxHelper.ExplodePropertySetter(syntax, out className, out var propertyName);
            }

            var classSyntax = ClassDeclarationGenerator.GenerateSyntax(className, context.Platform.View.MetaType, Enumerable.Empty<CSharp.MemberDeclaration>());

            var usingSyntax = UsingDirectiveGenerator.GenerateSyntax("System");

            var namespaceSyntax = NamespaceDeclarationGenerator.GenerateSyntax(typeSymbol.ContainingNamespace.ToString());
            namespaceSyntax = namespaceSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classSyntax));

            var unit = SyntaxFactory.CompilationUnit().AddUsings(usingSyntax).AddMembers(namespaceSyntax);

            var sourceCode = Formatter.Format(unit, context.Workspace, options.OptionSet).ToString();

            var fileName = syntax.Name.LocalName + ".cs";

            return new CreateProjectFileWorkUnit(sourceCode, fileName, context.Project.GetIdentifier()).AsList();
        }
    }
}
