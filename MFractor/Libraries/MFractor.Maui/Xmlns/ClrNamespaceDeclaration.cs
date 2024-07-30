using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    public class ClrNamespaceDeclaration
    {
        public string Keyword { get; }

        public string Namespace { get; }

        public ClrNamespaceDeclaration(string keyword,
                                       string @namespace)
        {
            Keyword = keyword;
            Namespace = @namespace;
        }
    }
}

