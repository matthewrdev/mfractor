﻿using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class NavigateToSymbolWorkUnitHandler : WorkUnitHandler<NavigateToSymbolWorkUnit>
    {
        readonly Lazy<IWorkEngine> workUnitEngine;
        IWorkEngine  WorkUnitEngine => workUnitEngine.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public NavigateToSymbolWorkUnitHandler(Lazy<IWorkEngine> workUnitEngine,
                                               Lazy<IDispatcher> dispatcher)
        {
            this.workUnitEngine = workUnitEngine;
            this.dispatcher = dispatcher;
        }

        public override async Task<IWorkExecutionResult> OnExecute(NavigateToSymbolWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var symbol = workUnit.Symbol;

            var syntax = symbol.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax();

            await Dispatcher.InvokeOnMainThreadAsync(async  () =>
           {
               if (syntax != null)
               {
                   var classSyntax = syntax as Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;

                   if (classSyntax == null)
                   {
                       await WorkUnitEngine.ApplyAsync(new NavigateToFileSpanWorkUnit(syntax.Span, syntax.SyntaxTree.FilePath, null));
                   }
                   else
                   {
                       await IdeApp.ProjectOperations.JumpToAsync(symbol, Location.Create(syntax.SyntaxTree, classSyntax.Identifier.Span));
                   }
               }
               else
               {
                    IdeApp.ProjectOperations.JumpToDeclaration(symbol, askIfMultipleLocations:workUnit.AllowMultipleSymbolResolve);
               }
           });

            return default;
        }

    }
}
