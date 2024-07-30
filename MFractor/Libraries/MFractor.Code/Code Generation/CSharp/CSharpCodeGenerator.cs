

namespace MFractor.Code.CodeGeneration.CSharp
{
    /// <summary>
    /// The base class for all C# code generators.
    /// </summary>
    public abstract class CSharpCodeGenerator : CodeGenerator
    {
        /// <summary>
        /// The languages that the C# code generator supports.
        /// <para/>
        /// Always "C#".
        /// </summary>
        /// <value>The languages.</value>
        public sealed override string[] Languages { get; } = new string[] { "C#" };
    }
}
