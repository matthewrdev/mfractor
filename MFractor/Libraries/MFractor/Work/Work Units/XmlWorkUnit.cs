using MFractor.Xml;

namespace MFractor.Work.WorkUnits
{
    public abstract class XmlWorkUnit : WorkUnit
    {
        public string FilePath { get; set; }

        public IXmlFormattingPolicy FormattingPolicy { get; set; }
    }
}
