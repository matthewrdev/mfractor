using System;
using MFractor.Code.Documents;
using MFractor.Xml;

namespace MFractor.Code.Analysis
{
    public class CodeAnalyserExecutionFilter
    {
        public readonly string Description;

        public readonly Func<IFeatureContext, XmlSyntax, bool> Filter;

        public CodeAnalyserExecutionFilter(string description)
        {
            Description = description;
        }

        public CodeAnalyserExecutionFilter(string description,
                                           Func<IFeatureContext, XmlSyntax, bool> filter)
        {
            Filter = filter;
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
