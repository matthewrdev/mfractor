using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Code.WorkUnits
{
    public class EncapsulateXmlSyntaxWorkUnit : XmlWorkUnit
    {
        public XmlNode Target { get; set; }

        public XmlNode NewParent { get; set; }
    }
}
