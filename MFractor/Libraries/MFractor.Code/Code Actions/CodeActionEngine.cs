using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.Code.CodeActions
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeActionEngine))]
    sealed class CodeActionEngine : ICodeActionEngine
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IReadOnlyDictionary<CodeActionCategory, IReadOnlyList<ICodeAction>>> categoryGroupedCodeActions;
        IReadOnlyDictionary<CodeActionCategory, IReadOnlyList<ICodeAction>> CategoryGroupedCodeActions => categoryGroupedCodeActions.Value;

        readonly Lazy<IReadOnlyDictionary<DocumentExecutionFilter, IReadOnlyList<ICodeAction>>> filterGroupedCodeActions;
        IReadOnlyDictionary<DocumentExecutionFilter, IReadOnlyList<ICodeAction>> FilterGroupedCodeActions => filterGroupedCodeActions.Value;

        readonly Lazy<IWorkEngine> workEngine;
        readonly Lazy<ICodeActionRepository> codeActionRepository;

        IWorkEngine WorkEngine => workEngine.Value;
        ICodeActionRepository CodeActionRepository => codeActionRepository.Value;

        public IEnumerable<DocumentExecutionFilter> Filters => FilterGroupedCodeActions.Keys.ToList();

        [ImportingConstructor]
        public CodeActionEngine(Lazy<IWorkEngine> workEngine,
                                Lazy<ICodeActionRepository> codeActionRepository)
        {
            this.workEngine = workEngine;
            this.codeActionRepository = codeActionRepository;

            categoryGroupedCodeActions = new Lazy<IReadOnlyDictionary<CodeActionCategory, IReadOnlyList<ICodeAction>>>(() =>
           {
               return CodeActionRepository.CodeActions.GroupBy(ca => ca.Category).ToDictionary(ca => ca.Key, group => (IReadOnlyList<ICodeAction>)group.ToList());
           });

            filterGroupedCodeActions = new Lazy<IReadOnlyDictionary<DocumentExecutionFilter, IReadOnlyList<ICodeAction>>>(() =>
            {
                return CodeActionRepository.CodeActions.GroupBy(ca => ca.Filter).ToDictionary(ca => ca.Key, group => (IReadOnlyList<ICodeAction>)group.ToList());
            });
        }

        public IEnumerable<ICodeAction> GetCodeActionsForCategory(CodeActionCategory category)
        {
            if (!CategoryGroupedCodeActions.ContainsKey(category))
            {
                return Enumerable.Empty<ICodeAction>();
            }

            return CategoryGroupedCodeActions[category];
        }

        public IEnumerable<ICodeAction> GetCodeActionsForFilter(DocumentExecutionFilter filter)
        {
            if (filter is null)
            {
                return Enumerable.Empty<ICodeAction>();
            }

            if (!FilterGroupedCodeActions.ContainsKey(filter))
            {
                return Enumerable.Empty<ICodeAction>();
            }

            return FilterGroupedCodeActions[filter];
        }

        public IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context,
                                                            InteractionLocation location,
                                                            CodeActionCategory category)
        {
            if (context is null
                || context.Document is null)
            {
                return Enumerable.Empty<ICodeAction>();
            }

            var result = new List<ICodeAction>();
            var syntax = context.GetSyntax(default(object));

            var filters = Filters;

            foreach (var filter in filters)
            {
                if (filter.AcceptsDocument(context.Document))
                {
                    var accepted = filter.AcceptsSyntax == null || filter.AcceptsSyntax(syntax);

                    if (accepted)
                    {
                        result.AddRange(GetCodeActionsForFilter(filter));
                    }
                }
            }

            return result
                .Where(op => op.Category == category)
                .Where(op =>
                {
                    try
                    {
                        return op.IsInterestedInDocument(context.Document, context);
                    }
                    catch (Exception ex)
                    {
                        log?.Info("Failed to check if " + op.Identifier + " is interested in document. Reason:");
                        log?.Exception(ex);
                    }
                    return false;
                })
                .Where(op =>
                {
                    var canExecute = false;
                    try
                    {
                        canExecute = op.CanExecute(context, location);
                    }
                    catch (Exception ex)
                    {
                        log?.Info("Failed to check if " + op.Identifier + " could execute. Reason:");
                        log?.Exception(ex);
                    }
                    return canExecute;
                }).ToList();
        }

        public IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context,
                                                            InteractionLocation location)
        {
            return RetrieveCodeActions(context, location, predicate: null);
        }

        public IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context,
                                                            InteractionLocation location,
                                                            IEnumerable<CodeActionCategory> categories)
        {
            if (context is null
                || context.Document is null)
            {
                return Enumerable.Empty<ICodeAction>();
            }

            var result = new List<ICodeAction>();
            var syntax = context.GetSyntax(default(object));
            categories ??= Enumerable.Empty<CodeActionCategory>();

            var filters = Filters;

            foreach (var filter in filters)
            {
                if (filter.AcceptsDocument(context.Document))
                {
                    var accepted = filter.AcceptsSyntax == null || filter.AcceptsSyntax(syntax);

                    if (accepted)
                    {
                        result.AddRange(GetCodeActionsForFilter(filter));
                    }
                }
            }

            return result
                .Where(op => categories.Contains(op.Category))
                .Where(op =>
                {
                    try
                    {
                        return op.IsInterestedInDocument(context.Document, context);
                    }
                    catch (Exception ex)
                    {
                        log?.Info("Failed to check if " + op.Identifier + " is interested in document. Reason:");
                        log?.Exception(ex);
                    }
                    return false;
                })
                .Where(op =>
                {
                    var canExecute = false;
                    try
                    {
                        canExecute = op.CanExecute(context, location);
                    }
                    catch (Exception ex)
                    {
                        log?.Info("Failed to check if " + op.Identifier + " could execute. Reason:");
                        log?.Exception(ex);
                    }
                    return canExecute;
                }).ToList();
        }

        public async Task<bool> Execute(ICodeActionChoice choice, InteractionLocation interactionLocation, IProgressMonitor progressMonitor = null)
        {
            var workUnits = choice.CodeAction.Execute(choice.Context, choice.Suggestion, interactionLocation);

            return await WorkEngine.ApplyAsync(workUnits, choice.Suggestion.Description, progressMonitor);
        }

        public IEnumerable<ICodeAction> RetrieveCommonCodeActions(IFeatureContext context, InteractionLocation location)
        {
            return RetrieveCodeActions(context, location, (ca) => AttributeHelper.HasAttribute<CommonCodeActionAttribute>(ca.GetType()));
        }

        public IEnumerable<ICodeAction> RetrieveCodeActions(IFeatureContext context, InteractionLocation location, Func<ICodeAction, bool> predicate)
        {
            if (context is null
                || context.Document is null)
            {
                return Enumerable.Empty<ICodeAction>();
            }

            var result = new List<ICodeAction>();
            var syntax = context.GetSyntax(default(object));

            var filters = Filters;

            foreach (var filter in filters)
            {
                if (filter.AcceptsDocument(context.Document))
                {
                    var accepted = filter.AcceptsSyntax == null || filter.AcceptsSyntax(syntax);

                    if (accepted)
                    {
                        result.AddRange(GetCodeActionsForFilter(filter));
                    }
                }
            }

            var filtered = result
                .Where(op =>
                {
                    try
                    {
                        return op.IsInterestedInDocument(context.Document, context);
                    }
                    catch (Exception ex)
                    {
                        log?.Info("Failed to check if " + op.Identifier + " is interested in document. Reason:");
                        log?.Exception(ex);
                    }
                    return false;
                });

            if (predicate != null)
            {
                filtered = filtered.Where(predicate);
            }

            return filtered.Where(op =>
                {
                    var canExecute = false;
                    try
                    {
                        canExecute = op.CanExecute(context, location);
                    }
                    catch (Exception ex)
                    {
                        log?.Info("Failed to check if " + op.Identifier + " could execute. Reason:");
                        log?.Exception(ex);
                    }
                    return canExecute;
                }).ToList();

        }
    }
}
