using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    public class AssemblyDeclaration
    {
        public string AssemblyKeyword { get; }

        public string AssemblyName { get; }

        public AssemblyDeclaration(string keyword,
                                    string name)
        {
            AssemblyKeyword = keyword;
            AssemblyName = name;
        }
    }
}

