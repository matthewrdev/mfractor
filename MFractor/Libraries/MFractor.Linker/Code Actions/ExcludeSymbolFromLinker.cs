using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Configuration;
using MFractor.Linker.CodeGeneration;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Linker.CodeActions
{
    class ExcludeSymbolFromLinker : CSharpCodeAction
    {
        public const string Tip = "The Exclude From Linking code action creates linker config files and ensures a symbol isn't remove by the linker.";

        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.linker.exclude_symbol_from_linking";

        public override string Name => "Exclude Symbol From Linker";

        public override string Documentation => "When creating Android and iOS applications, the Xamarin toolchain uses the Linker to remove unused symbols from the final application build. The **Exclude Symbol From Linker** code action generates a linker.xml entry for the symbol that is selected.";

        const string excludeFromLinkerKeyBase = "com.mfractor.linker.projects.";

        readonly Lazy<IUserOptions> userOptions;
        public IUserOptions UserOptions => userOptions.Value;

        [Import]
        public ILinkerFileGenerator LinkerFileGenerator { get; set; }

        [Import]
        public ILinkerEntryGenerator LinkerEntryGenerator { get; set; }

        [ImportingConstructor]
        public ExcludeSymbolFromLinker(Lazy<IUserOptions> userOptions)
        {
            this.userOptions = userOptions;
        }

        public override bool CanExecute(SyntaxNode syntax,
                                        IParsedCSharpDocument document,
                                        IFeatureContext context,
                                        InteractionLocation location)
        {
            var mobileProjects = context.Project.GetDependentMobileProjects();

            if (!mobileProjects.Any())
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

            // If the user has selected a bulk of items, don't show the action as its kinda annoying.
            if (location.Selection != null)
            {
                if (location.Selection.Value.Length > 0)
                {
                    return false;
                }
            }

            var symbol = model.GetSymbolInfo(syntax).Symbol;
            if (symbol == null)
            {
                symbol = model.GetDeclaredSymbol(syntax);
            }

            return LinkerEntryGenerator.CanCreateLinkerEntry(symbol);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax,
                                                                  IParsedCSharpDocument document,
                                                                  IFeatureContext context,
                                                                  InteractionLocation location)
        {
            context.Project.TryGetCompilation(out var compilation);
            var model = compilation.GetSemanticModel(document.GetSyntaxTree());

            var symbol = model.GetSymbolInfo(syntax).Symbol;
            if (symbol == null)
            {
                symbol = model.GetDeclaredSymbol(syntax);
            }

            return CreateSuggestion($"Exclude {symbol.Name} from linking").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax,
                                                       IParsedCSharpDocument document,
                                                       IFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            context.Project.TryGetCompilation(out var compilation);
            var model = compilation.GetSemanticModel(document.GetSyntaxTree());

            var symbol = model.GetSymbolInfo(syntax).Symbol;
            if (symbol == null)
            {
                symbol = model.GetDeclaredSymbol(syntax);
            }

            var mobileProjects = context.Project.GetDependentMobileProjects();

            IReadOnlyList<IWorkUnit> addLinkerEntry(GenerateCodeFilesResult result)
            {
                var workUnits = new List<IWorkUnit>();

                foreach (var p in result.SelectedProjects)
                {
                    var units = LinkerFileGenerator.AddLinkedSymbols(p, new[] { symbol });
                    workUnits.AddRange(units);
                }

                return workUnits;
            }

            return new GenerateCodeFilesWorkUnit(LinkerFileGenerator.DefaultLinkerFileName,
                                                       null,
                                                       mobileProjects,
                                                       LinkerFileGenerator.DefaultLinkerFileFolder,
                                                       "Exclude From Linking",
                                                       $"Choose projects to exclude {symbol.Name} from linking.",
                                                       string.Empty,
                                                       ProjectSelectorMode.Multiple,
                                                       addLinkerEntry)
            {
                IsFolderPathEditiable = false,
                IsNameEditable = false,
            }.AsList();
        }

        bool IsEnabled(Project project)
        {
            return UserOptions.Get(GetKey(project), true);
        }

        string GetKey(Project project)
        {
            return excludeFromLinkerKeyBase + project.Name.ToLower().Replace(" ", "_");
        }
    }
}
