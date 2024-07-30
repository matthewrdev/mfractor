using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Code.WorkUnits
{
    public class ReplaceXmlSyntaxWorkUnit : XmlWorkUnit
    {
        public XmlSyntax Existing { get; set; }

        public XmlSyntax New { get; set; }

        public bool ReplaceChildren { get; set; }

        public bool GenerateClosingTags { get; set; }
    }
}
