using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Syntax.Parsers;
using MFractor.Maui.Xmlns;
using MFractor.IOC;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IExpressionParserRepository))]
    class ExpressionParserRepository : PartRepository<IExpressionParser>, IExpressionParserRepository
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        IReadOnlyList<IExpressionParser> ExpressionParsers => Parts;
        protected Dictionary<string, IExpressionParser> Parsers = new Dictionary<string, IExpressionParser>();

        [ImportingConstructor]
        public ExpressionParserRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
            var count = 0;
            foreach (var parser in ExpressionParsers)
            {
                try
                {
                    RegisterExpressionParser(parser);
                    count++;
                }
                catch (Exception ex)
                {
                    log?.Warning("Failed to add the expression parser '" + parser.Identifier + "' into the parser provider cache. Reason: " + ex.ToString());
                }
            }
        }

        public void RegisterExpressionParser(IExpressionParser parser)
        {
            if (Parsers.ContainsKey(parser.Identifier))
            {
                throw new InvalidOperationException("The expression parser provider already has a parser with the identifier: " + parser.Identifier);
            }
            Parsers.Add(parser.Identifier, parser);
        }

        public TParser ResolveParser<TParser>() where TParser : class, IExpressionParser
        {
            return Parsers.Values.OfType<TParser>().FirstOrDefault();
        }

        public IExpressionParser ResolveParser(string identifier)
        {
            if (!this.Parsers.ContainsKey(identifier))
            {
                return null;
            }

            return this.Parsers[identifier];
        }

        public IExpressionParser ResolveParser(XmlAttribute attribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            return this.Parsers.Values.Where(p => p.CanParse(attribute, project, namespaces, xmlnsDefinitions, platform))
                                      .OrderBy(p => p.ParserPriority)
                                      .FirstOrDefault();
        }

        public IExpressionParser ResolveParser(string expression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            return this.Parsers.Values.Where(p => p.CanParse(expression, project, namespaces, xmlnsDefinitions, platform))
                                      .OrderBy(p => p.ParserPriority)
                                      .FirstOrDefault();
        }

        public IExpressionParser ResolveParser(Type type)
        {
            return Parsers.Values.FirstOrDefault(p => p.GetType() == type);
        }
    }
}

