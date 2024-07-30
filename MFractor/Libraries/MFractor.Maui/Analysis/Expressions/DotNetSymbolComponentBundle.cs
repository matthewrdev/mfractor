using MFractor.Maui.Syntax.Expressions;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Analysis
{
    class DotNetSymbolComponentBundle
    {
        public DotNetTypeSymbolExpression Expression;

        public string SuggestedClassName;
        public TextSpan ClassSpan;

        public string ReferencedMemberName;
        public string SuggestedMemberName;
        public TextSpan MemberSpan;
    }
}

