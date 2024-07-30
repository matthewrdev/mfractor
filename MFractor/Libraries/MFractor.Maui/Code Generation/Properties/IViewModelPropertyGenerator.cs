using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.CSharp.CodeGeneration;

namespace MFractor.Maui.CodeGeneration.CSharp
{
    public interface IViewModelPropertyGenerator : ICodeGenerator, IInstancePropertyGenerator
    {
        string DefaultEnumerableType { get; set; }

        ICodeSnippet ValueSnippet { get; set; }
    }
}
