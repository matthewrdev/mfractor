using System;

namespace MFractor.Xml
{
    public class XmlName : XmlSyntax, IXmlName
    {
        public XmlName()
        {
        }

        public XmlName(string name)
        {
            FullName = name;
            if (name.Contains(":"))
            {
                var components = name.Split(':');
                if (components.Length >= 1)
                {
                    LocalName = components[0];
                }

                if (components.Length >= 2)
                {
                    Namespace = components[1];
                }
            }
            else
            {
                LocalName = name;
                Namespace = "";
            }
        }
        public XmlName(string xmlns, string name)
        {
            FullName = !string.IsNullOrEmpty(xmlns) ? xmlns + ":" + name : name;
            LocalName = name;
            Namespace = xmlns;
        }

        public string FullName { get; set; }

        public string Namespace { get; set; }

        public string LocalName { get; set; }

        public bool HasNamespace
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Namespace);
            }
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}

