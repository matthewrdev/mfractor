using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.Semantics;
using MFractor.Work;

namespace MFractor.Maui.CodeGeneration.Xaml
{
    public interface IRenameXamlNamespaceGenerator : ICodeGenerator
    {
        List<IWorkUnit> RenameNamespace(string currentNamespace, 
                                        string newNamespace, 
                                        IParsedXamlDocument context,
                                        IXamlSemanticModel xamlSemanticModel,
                                        bool shouldRenameNamespaceDeclaration);
    }
}
