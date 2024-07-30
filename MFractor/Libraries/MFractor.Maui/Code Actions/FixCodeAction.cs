using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;

using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    /// <summary>
    /// A code action for XAML that generates code.
    /// <para/>
    /// Code actions that implement this class will appear within the Generate menu.
    /// </summary>
    public abstract class FixCodeAction : XamlCodeAction, IFixCodeAction
    {
        [Import]
        protected IAnalysisResultStore AnalysisResultStore { get; set; }

        /// <summary>
        /// The type of <see cref="IXmlSyntaxCodeAnalyser "/> that this code action fixes.
        /// </summary>
        /// <value>The type of the code analyser.</value>
        public abstract Type TargetCodeAnalyser { get; }

        public virtual IEnumerable<Type> TargetCodeAnalysers => TargetCodeAnalyser.AsList();

        /// <summary>
        /// The category that this code action belongs to.
        /// </summary>
        /// <returns>The category.</returns>
        public sealed override CodeActionCategory Category => CodeActionCategory.Fix;

        public sealed override IReadOnlyList<IWorkUnit> Execute(IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            throw new InvalidOperationException("A fix must be targetted to a either an xml node or attribute");
        }

        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            throw new InvalidOperationException("A fix must be targetted to a either an xml node or attribute");
        }

        public sealed override bool CanExecute(IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            throw new InvalidOperationException("A fix must be targetted to a either an xml node or attribute");
        }

        public sealed override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var issue = IssuesFor(syntax, location.Position).FirstOrDefault();

            if (issue == null)
            {
                return false;
            }

            return CanExecute(issue, syntax, document, context, location);
        }

        protected virtual bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            throw new NotImplementedException();
        }

        public sealed override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var issue = IssuesFor(syntax, location.Position).FirstOrDefault();

            if (issue == null)
            {
                return false;
            }

            return CanExecute(issue, syntax, document, context, location);
        }

        protected virtual bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            throw new NotImplementedException();
        }

        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var issue = IssuesFor(syntax, location.Position).FirstOrDefault();

            if (issue == null)
            {
                return null;
            }

            return Suggest(issue, syntax, document, context, location);
        }

        protected virtual IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            throw new NotImplementedException();
        }

        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var issue = IssuesFor(syntax, location.Position).FirstOrDefault();

            if (issue == null)
            {
                return null;
            }

            return Suggest(issue, syntax, document, context, location);
        }

        /// <summary>
        /// Suggest the available code actions given the provided <paramref name="issue"/>, <paramref name="syntax"/>, <paramref name="document"/>, <paramref name="context"/> and <paramref name="location"/>.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="issue">Issue.</param>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        protected virtual IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            throw new NotImplementedException();
        }

        public sealed override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var issue = IssuesFor(syntax, location.Position).FirstOrDefault();

            if (issue == null)
            {
                return null;
            }

            return Execute(issue, syntax, document, context, suggestion, location);
        }

        protected virtual IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            throw new NotImplementedException();
        }

        public sealed override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var issue = IssuesFor(syntax, location.Position).FirstOrDefault();

            if (issue == null)
            {
                return null;
            }

            return Execute(issue, syntax, document, context, suggestion, location);
        }

        protected virtual IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            throw new NotImplementedException();
        }

        protected IEnumerable<ICodeIssue> IssuesFor(XmlNode node, int position)
        {
            var issues = node.Get(MFractor.MetaDataKeys.Analysis.Issues, default(List<ICodeIssue>));

            return FindCandidateIssues(issues, position);
        }

        protected IEnumerable<ICodeIssue> IssuesFor(XmlAttribute attr, int position)
        {
            var issues = attr.Get(MFractor.MetaDataKeys.Analysis.Issues, default(List<ICodeIssue>));

            return FindCandidateIssues(issues, position);
        }

        /// <summary>
        /// Given the <paramref name="position"/>, finds 
        /// </summary>
        /// <returns>The candidate issues.</returns>
        /// <param name="issues">Issues.</param>
        /// <param name="position">Position.</param>
        protected IEnumerable<ICodeIssue> FindCandidateIssues(IEnumerable<ICodeIssue> issues, int position)
        {
            if (issues == null || !issues.Any())
            {
                return Array.Empty<ICodeIssue>();
            }

            return issues.Where(i => TargetCodeAnalysers.Contains(i.AnalyserType))
                         .Where(i => FileLocationHelper.IsBetween(position, i.Span))
                         .ToList();
        }
    }
}
