using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;

using MFractor.Code.Documents;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    /// <summary>
    /// A code action that executes inside a XAML document.
    /// </summary>
    public abstract class XamlCodeAction : XmlCodeAction
    {
        /// <summary>
        /// For this <see cref="XmlCodeAction"/>, is it valid with the provided <paramref name="context"/>?
        /// </summary>
        /// <returns><c>true</c>, if feature context state valid was ised, <c>false</c> otherwise.</returns>
        /// <param name="context">Context.</param>
        protected override bool IsFeatureContextStateValid(IFeatureContext context)
        {
            if (!(context is IXamlFeatureContext xamlContext))
            {
                return false;
            }

            // If the project is null then the semantic model and syntax tree is not available.
            return xamlContext.Project != null;
        }

        /// <summary>
        /// Can this <see cref="MFractor.Code.CodeActions.XmlCodeAction"/> execute in this document?
        /// </summary>
        /// <returns><c>true</c>, if execute was caned, <c>false</c> otherwise.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public sealed override bool CanExecute(IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            if (!(context is IXamlFeatureContext xamlContext))
            {
                return false;
            }

            if (!(document is IParsedXamlDocument xamlDocument))
            {
                return false;
            }

            return CanExecute(xamlDocument, xamlContext, location);
        }

        /// <summary>
        /// Can this <see cref="MFractor.Code.CodeActions.XmlCodeAction"/> execute with this xml node?
        /// </summary>
        /// <returns><c>true</c>, if execute was caned, <c>false</c> otherwise.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public sealed override bool CanExecute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            if (!(context is IXamlFeatureContext xamlContext))
            {
                return false;
            }

            if (!(document is IParsedXamlDocument xamlDocument))
            {
                return false;
            }

            return CanExecute(syntax, xamlDocument, xamlContext, location);
        }

        /// <summary>
        /// Can this <see cref="MFractor.Code.CodeActions.XmlCodeAction"/> execute with this xml attribute?
        /// </summary>
        /// <returns><c>true</c>, if execute was caned, <c>false</c> otherwise.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public sealed override bool CanExecute(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            if (!(context is IXamlFeatureContext xamlContext))
            {
                return false;
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return false;
            }

            return CanExecute(syntax, xamlDocument, xamlContext, location);
        }

        /// <summary>
        /// Suggest available code action choices to execute in the given document.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            return Suggest(xamlDocument, xamlContext, location);
        }

        /// <summary>
        /// Suggest available code action choices to execute against the given xml node.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            return Suggest(syntax, xamlDocument, xamlContext, location);
        }

        /// <summary>
        /// Suggest available code action choices to execute against the given xml attribute.
        /// </summary>
        /// <returns>The suggest.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        /// <param name="location">Location.</param>
        public sealed override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return Array.Empty<ICodeActionSuggestion>();
            }

            return Suggest(syntax, xamlDocument, xamlContext, location);
        }

        public sealed override IReadOnlyList<IWorkUnit> Execute(IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return Execute(xamlDocument, xamlContext, suggestion, location);
        }

        public sealed override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return Execute(syntax, xamlDocument, xamlContext, suggestion, location);
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
        public sealed override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return Execute(syntax, xamlDocument, xamlContext, suggestion, location);
        }

        /// <summary>
        /// Can this code action execute against the given <paramref name="document"/>, <paramref name="context"/> and <paramref name="location"/>?
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual bool CanExecute(IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        public virtual bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        public virtual bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return false;
        }

        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return Array.Empty<ICodeActionSuggestion>();
        }

        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return Array.Empty<ICodeActionSuggestion>();
        }

        public virtual IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return Array.Empty<ICodeActionSuggestion>();
        }

        public virtual IReadOnlyList<IWorkUnit> Execute(IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return Array.Empty<IWorkUnit>();
        }

        public virtual IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return Array.Empty<IWorkUnit>();
        }

        public virtual IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return Array.Empty<IWorkUnit>();
        }

        /// <summary>
        /// Is this code action available in the provided <paramref name="document"/> and <paramref name="context"/>.
        /// </summary>
        /// <returns><c>true</c>, if available in document was ised, <c>false</c> otherwise.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        public sealed override bool IsAvailableInDocument(IParsedXmlDocument document, IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return false;
            }

            var xamlDocument = document as IParsedXamlDocument;
            if (xamlDocument == null)
            {
                return false;
            }

            return IsAvailableInDocument(xamlDocument, xamlContext);
        }

        /// <summary>
        /// Is this code action available in the provided <paramref name="document"/> and <paramref name="context"/>.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool IsAvailableInDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return true;
        }
    }
}
