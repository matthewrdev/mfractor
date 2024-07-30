using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.CodeGeneration;
using MFractor.Code.Documents;
using MFractor.Code.TypeInferment;
using MFractor.Code.WorkUnits;
using MFractor.CodeSnippets;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.BindableProperties;
using MFractor.Maui.Configuration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions.Generate
{
    class InsertBindableProperty : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.insert_bindable_property";

        public override string Name => "Create Bindable Property";

        public override string Documentation => "When inside a Visual Element, this code action generates a new bindable property declaration.";

        [Import]
        public IBindablePropertyGenerator BindablePropertyGenerator { get; set; }

        [Import]
        public IExceptionHandlerGenerator ExceptionHandlerGenerator { get; set; }

        [Import]
        public IValueConverterTypeInfermentConfiguration TypeInfermentConfiguration { get; set; }

        [Import]
        public ITypeInfermentService TypeInfermentService { get; set; }

        [Import]
        public IXamlPlatformRepository XamlPlatforms { get; set; }

        protected override bool IsAvailableInDocument(IParsedCSharpDocument document, IFeatureContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var classDeclarationSyntax = (syntax is ClassDeclarationSyntax ? syntax as ClassDeclarationSyntax : syntax.Parent as ClassDeclarationSyntax);
            if (classDeclarationSyntax == null)
            {
                return false;
            }

            if (classDeclarationSyntax != syntax && syntax.Span.Contains(location.Position))
            {
                return false;
            }

            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            var model = compilation.GetSemanticModel(syntax.SyntaxTree);

            if (model == null)
            {
                return false;
            }

            var platform = XamlPlatforms.ResolvePlatform(context.Project);
            var classType = model.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;

            return SymbolHelper.DerivesFrom(classType, platform.BindableObject.MetaType);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return new List<ICodeActionSuggestion>
            {
                CreateSuggestion("Create a bindable property", BindablePropertyKind.Default),
                CreateSuggestion("Create a bindable property with property changed callback", BindablePropertyKind.IncludePropertyChanged)
            };
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var classDeclarationSyntax = (syntax is ClassDeclarationSyntax ? syntax as ClassDeclarationSyntax : syntax.Parent as ClassDeclarationSyntax);
            context.Project.TryGetCompilation(out var compilation);

            var model = compilation.GetSemanticModel(syntax.SyntaxTree);
            var platform = XamlPlatforms.ResolvePlatform(context.Project);

            var classType = model.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;

            var snippet =  BindablePropertyGenerator.GetSnippet(suggestion.GetAction<BindablePropertyKind>());

            snippet.SetArgumentValue("control_type", classType.Name);
            snippet.SetArgumentValue("bindable_property", platform.BindableProperty.MetaType);
            snippet.SetArgumentValue("bindable_object", platform.BindableObject.MetaType);

            bool onArgumentValueEdited(string argumentName, ICodeSnippet editedSnippet)
            {
                var name = EnumHelper.GetEnumDescription(ReservedCodeSnippetArgumentName.Name);
                var nameArgument = editedSnippet.GetNamedArgument(name);

                if (argumentName != name || nameArgument == null)
                {
                    return false;
                }

                var type = EnumHelper.GetEnumDescription(ReservedCodeSnippetArgumentName.Type);
                var typeArgument = editedSnippet.GetNamedArgument(type);

                if (typeArgument == null)
                {
                    return false;
                }

                var inferredType = TypeInfermentService.InferTypeFromNameAndValue(nameArgument.Value,
                                                                                  string.Empty,
                                                                                  platform.Color.MetaType,
                                                                                  platform.ImageSource.MetaType,
                                                                                  TypeInfermentConfiguration.DefaultType,
                                                                                  compilation);

                if (typeArgument.Value == inferredType)
                {
                    return false;
                }

                editedSnippet.SetArgumentValue(type, inferredType);
                return true;
            }

            IReadOnlyList<IWorkUnit> applyCodeSnippet(ICodeSnippet codeSnippet)
            {
                var members = classDeclarationSyntax.Members;

                var anchor = members.FirstOrDefault(m => m.FullSpan.IntersectsWith(location.Position));

                var insertionLocation = InsertionLocation.Start;
                if (anchor != null && location.Position > anchor.Span.End)
                {
                    insertionLocation = InsertionLocation.End;
                }

                return new InsertSyntaxNodesWorkUnit()
                {
                    HostNode = classDeclarationSyntax,
                    SyntaxNodes = codeSnippet.AsMembersList(),
                    Project = context.Project,
                    AnchorNode = anchor,
                    InsertionLocation = insertionLocation,
                    Workspace = context.Workspace,
                }.AsList();
            }

            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Type, TypeInfermentConfiguration.DefaultType);

            return new InsertCodeSnippetWorkUnit()
            {
                CodeSnippet = snippet,
                HelpUrl = "http://docs.mfractor.com/xamarin-forms/custom-controls/bindable-property-wizard/",
                Title = "Create Bindable Property",
                ConfirmButton = "Create Bindable Property",
                OnArgumentValueEditedFunc = onArgumentValueEdited,
                ApplyCodeSnippetFunc = applyCodeSnippet,
            }.AsList();
        }
    }
}
