using MFractor.Attributes;

namespace MFractor.CSharp.CodeGeneration.ClassFromClipboard
{
    public enum NamespaceMode
    {
        [Description("Automatic")]
        Automatic,

        [Description("Preserve")]
        Preserve,

        [Description("Custom")]
        Custom,
    }
}