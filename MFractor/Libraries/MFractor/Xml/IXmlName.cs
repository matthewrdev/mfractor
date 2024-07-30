namespace MFractor.Xml
{
    public interface IXmlName
    {
        string FullName { get; set; }
        string Namespace { get; set; }
        string LocalName { get; set; }
        bool HasNamespace { get; }

        string ToString();
    }
}