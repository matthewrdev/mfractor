using MFractor.Code.CodeGeneration;
using MFractor.Xml;

namespace MFractor.Localisation.CodeGeneration
{
    public interface IResXEntryGenerator : ICodeGenerator
    {
        bool IncludeCommentWhenEmpty { get; set; }
        
        XmlNode GenerateSyntax(string key, string value, string comment);

        string GenerateCode(string key, string value, string comment, string indent);
    }
}
