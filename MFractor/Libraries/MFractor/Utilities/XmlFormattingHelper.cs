using System;
using System.IO;
using System.Text;
using System.Xml;

namespace MFractor.Utilities
{
    public static class XmlFormattingHelper
    {
        public static string FormatXml(string xml)
        {
            try
            {
                var result = "";

                using (var mStream = new MemoryStream())
                {
                    var writer = new XmlTextWriter(mStream, Encoding.Unicode);
                    var document = new XmlDocument();

                    try
                    {
                        var byteArray = Encoding.UTF8.GetBytes(xml);
                        using (var stream = new MemoryStream(byteArray))
                        {
                            using var xtr = new XmlTextReader(stream) { Namespaces = false };
                            document.Load(xtr);
                        }

                        writer.Formatting = System.Xml.Formatting.Indented;

                        // Write the XML into a formatting XmlTextWriter
                        document.WriteContentTo(writer);
                        writer.Flush();
                        mStream.Flush();

                        // Have to rewind the MemoryStream in order to read
                        // its contents.
                        mStream.Position = 0;

                        // Read MemoryStream contents into a StreamReader.
                        var streamReader = new StreamReader(mStream);

                        // Extract the text from the StreamReader.
                        var formattedXml = streamReader.ReadToEnd();

                        result = formattedXml;
                    }
                    catch (XmlException)
                    {
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}