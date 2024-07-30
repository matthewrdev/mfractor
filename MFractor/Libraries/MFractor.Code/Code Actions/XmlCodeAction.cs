using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Documents;
using MFractor.IOC;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A code action that is available in XML files.
    /// </summary>
    public abstract class XmlCodeAction : CodeAction
    {
        readonly Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService = new Lazy<IXmlFormattingPolicyService>(() => Resolver.Resolve<IXmlFormattingPolicyService>());

        /// <summary>
        /// Gets the xml formatting policy service.
        /// </summary>
        protected IXmlFormattingPolicyService XmlFormattingPolicyService => xmlFormattingPolicyService.Value;

        /// <summary>
        /// For this <see cref="XmlCodeAction"/>, is it valid with the provided <paramref name="context"/>?
        /// </summary>
        /// <returns><c>true</c>, if feature context state valid was ised, <c>false</c> otherwise.</returns>
        /// <param name="context">Context.</param>
        protected abstract bool IsFeatureContextStateValid(IFeatureContext context);

        /// <summary>
        /// Can this code action operation execute in the given context.
        /// </summary>
        /// <returns><c>true</c>, if the refactoring can execute, <c>false</c> otherwise.</returns>
        /// <param name="context">The code action bundle</param>
        /// <param name="location">The execution context in the documentation that triggered the action</param>
        public sealed override bool CanExecute(IFeatureContext context, InteractionLocation location)
        {
            var parsedDocument = context.Document as ParsedXmlDocument;
            if (parsedDocument == null)
            {
                return false;
            }

            if (!IsFeatureContextStateValid(context))
            {
                return false;
            }

            ApplyConfiguration(context.ConfigurationId);

            if (this.Filter == XmlExecutionFilters.XmlDocument)
            {
                return CanExecute(parsedDocument, context, location);
            }

            var syntax = context.GetSyntax(default(XmlSyntax));
            if (syntax == null)
            {
                return false;
            }

            if (syntax is XmlNode)
            {
                return CanExecute(syntax as XmlNode, parsedDocument, context, location);
            }

            if (syntax is XmlAttribute)
            {
                return CanExecute(syntax as XmlAttribute, parsedDocument, context, location);
            }

            return false;
        }

        /// <summary>
        /// Suggest a list of available code actions within the context.
        /// </summary>
        /// <returns>The actions that this operation supports</returns>
        /// <param name="context">Bundle.</param>
        /// <param name="location">The location (position and selection) the code action was triggered at</param>
        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(IFeatureContext context, InteractionLocation location)
        {
            var parsedDocument = context.Document as ParsedXmlDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            ApplyConfiguration(context.ConfigurationId);

            if (this.Filter == XmlExecutionFilters.XmlDocument)
            {
                return Suggest(parsedDocument, context, location);
            }

            var syntax = context.GetSyntax(default(XmlSyntax));
            if (syntax == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            if (syntax is XmlNode)
            {
                return Suggest(syntax as XmlNode, parsedDocument, context, location);
            }

            if (syntax is XmlAttribute)
            {
                return Suggest(syntax as XmlAttribute, parsedDocument, context, location);
            }

            return Array.Empty<ICodeActionSuggestion>();
        }

        /// <summary>
        /// Execute the code action, building a collection of workUnits that should be applied by the IDE.
        /// </summary>
        /// <param name="context">Bundle.</param>
        /// <param name="suggestion">The code action suggestion to execute</param>
        /// <param name="location">The execution context in the documentation that triggered the action</param>
        public sealed override IReadOnlyList<IWorkUnit> Execute(IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var parsedDocument = context.Document as ParsedXmlDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            ApplyConfiguration(context.ConfigurationId);
            if (this.Filter == XmlExecutionFilters.XmlDocument)
            {
                return Execute(parsedDocument, context, suggestion, location);
            }

            var syntax = context.GetSyntax(default(XmlSyntax));
            if (syntax is XmlNode xmlNode)
            {
                return Execute(xmlNode, parsedDocument, context, suggestion, location);
            }

            if (syntax is XmlAttribute xmlAttribute)
            {
                return Execute(xmlAttribute, parsedDocument, context, suggestion, location);
            }

            return Array.Empty<IWorkUnit>();
        }

        /// <summary>
        /// Can this <see cref="XmlCodeAction"/> execute in this document?
        /// </summary>
        /// <returns><c>true</c>, if execute was caned, <c>false</c> otherwise.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public virtual bool CanExecute(IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        /// <summary>
        /// Can this <see cref="XmlCodeAction"/> execute with this xml node?
        /// </summary>
        /// <returns><c>true</c>, if execute was caned, <c>false</c> otherwise.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public virtual bool CanExecute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        /// <summary>
        /// Can this <see cref="XmlCodeAction"/> execute with this xml attribute?
        /// </summary>
        /// <returns><c>true</c>, if execute was caned, <c>false</c> otherwise.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public virtual bool CanExecute(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        /// <summary>
        /// Suggest available code action choices to execute in the given document.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            return Array.Empty<ICodeActionSuggestion>();
        }

        /// <summary>
        /// Suggest available code action choices to execute against the given xml node.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
		{
			return Array.Empty<ICodeActionSuggestion>();
        }

        /// <summary>
        /// Suggest available code action choices to execute against the given xml attribute.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
		{
			return Array.Empty<ICodeActionSuggestion>();
        }

        /// <summary>
        /// Execute the code action.
        /// </summary>
        /// <returns>The execute.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="suggestion">Suggestion.</param>
        /// <param name="location">Location.</param>
        public virtual IReadOnlyList<IWorkUnit> Execute(IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
			return Array.Empty<IWorkUnit>();
        }

        /// <summary>
        /// Execute the code action.
        /// </summary>
        /// <returns>The execute.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="suggestion">Suggestion.</param>
        /// <param name="location">Location.</param>
        public virtual IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
			return Array.Empty<IWorkUnit>();
        }

        /// <summary>
        /// Execute the code action.
        /// </summary>
        /// <returns>The execute.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="suggestion">Suggestion.</param>
        /// <param name="location">Location.</param>
        public virtual IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
			return Array.Empty<IWorkUnit>();
        }

        /// <summary>
        /// If the code action can execute against a specific document.
        /// 
        /// Use this to disable the code action based on a cheap evaulation condition.
        /// 
        /// For instance, if a certain namespace is expected to be present, returning false can skip unnecessary validation the code action
        /// would otherwise have to do in the CanExecute method.
        /// </summary>
        /// <returns><c>true</c>, if this refactoring operation can execute against the document, <c>false</c> otherwise.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">The context for the code action</param>
        public sealed override bool IsInterestedInDocument(IParsedDocument document, IFeatureContext context)
        {
            var parsedDocument = document as ParsedXmlDocument;
            if (parsedDocument == null)
            {
                return false;
            }

            return IsAvailableInDocument(parsedDocument, context);
        }

        /// <summary>
        /// Is this code action available in the provided <paramref name="document"/> and <paramref name="context"/>.
        /// </summary>
        /// <returns><c>true</c>, if available in document was ised, <c>false</c> otherwise.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        public abstract bool IsAvailableInDocument(IParsedXmlDocument document, IFeatureContext context);

        /// <summary>
        /// Suggest available code action choices to execute in the given document.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public IReadOnlyList<ICodeActionSuggestion> Suggest(XmlSyntax syntax, IFeatureContext context, InteractionLocation location)
        {
            var parsedDocument = context.Document as ParsedXmlDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            if (Filter == XmlExecutionFilters.XmlDocument)
            {
                if (syntax is XmlNode)
                {
                    return Suggest(parsedDocument, context, location);
                }
            }

            if (Filter == XmlExecutionFilters.XmlNode)
            {
                if (syntax is XmlNode)
                {
                    return Suggest(syntax as XmlNode, parsedDocument, context, location);
                }
            }

            if (Filter == XmlExecutionFilters.XmlAttribute)
            {
                if (syntax is XmlAttribute)
                {
                    return Suggest(syntax as XmlAttribute, parsedDocument, context, location);
                }
            }

            return Array.Empty<ICodeActionSuggestion>();
        }

        /// <summary>
        /// Execute the code action.
        /// </summary>
        /// <returns>The execute.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="context">Context.</param>
        /// <param name="suggestion">Suggestion.</param>
        /// <param name="location">Location.</param>
        public IReadOnlyList<IWorkUnit> Execute(XmlSyntax syntax, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var parsedDocument = context.Document as ParsedXmlDocument;
            if (parsedDocument == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            if (syntax is XmlNode)
            {
                return Execute(syntax as XmlNode, parsedDocument, context, suggestion, location);
            }

            if (syntax is XmlAttribute)
            {
                return Execute(syntax as XmlAttribute, parsedDocument, context, suggestion, location);
            }

            return Array.Empty<IWorkUnit>();
        }
    }
}
