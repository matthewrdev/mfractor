using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for serialising objects to and from XML.
    /// </summary>
    public static class XmlSerializationHelper
    {
        /// <summary>
        /// Serialise the given <paramref name="obj"/> to the <paramref name="filePath"/>.
        /// </summary>
        /// <returns><c>true</c>, if to file was serialized, <c>false</c> otherwise.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="obj">Object.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public static bool SerializeToFile<TObject>(string filePath, TObject obj)
        {
            return SerializeToFile(filePath, obj, typeof(TObject));
        }

        /// <summary>
        /// Serialise the given <paramref name="obj"/> of <paramref name="type"/> to the <paramref name="filePath"/>.
        /// </summary>
        /// <returns><c>true</c>, if to file was serialized, <c>false</c> otherwise.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="obj">Object.</param>
        /// <param name="type">Type.</param>
        public static bool SerializeToFile(string filePath, object obj, Type type)
        {
            if (obj == null || filePath == null)
            {
                return false;
            }

            var fi = new FileInfo(filePath);
            if (Directory.Exists(fi.Directory.FullName) == false)
            {
                Directory.CreateDirectory(fi.Directory.FullName);
            }

            var xml = SerializeToString(obj, type);

            File.WriteAllText(filePath, xml);

            return true;
        }

        /// <summary>
        /// Serialise the given <paramref name="obj"/> to string.
        /// </summary>
        /// <returns>The to string.</returns>
        /// <param name="obj">Object.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public static string SerializeToString<TObject>(TObject obj)
        {
            return SerializeToString(obj, typeof(TObject));
        }

        /// <summary>
        /// Serialise the given <paramref name="obj"/> of <paramref name="type"/> to string.
        /// </summary>
        /// <returns>The to string.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="type">Type.</param>
        public static string SerializeToString(object obj, Type type)
        {
            var serializer = new XmlSerializer(type);
            var sb = new StringBuilder();

            string xml;
            try
            {
                using (TextWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, obj);
                }
                xml = sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return string.Empty;
            }

            return xml;
        }

        /// <summary>
        /// Deserialise the <paramref name="filePath"/> to a <typeparamref name="TObject"/>.
        /// </summary>
        /// <returns>The from file.</returns>
        /// <param name="filePath">File path.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public static TObject DeserializeFromFile<TObject>(string filePath) where TObject : class
        {
            return DeserializeFromFile(filePath, typeof(TObject)) as TObject;
        }

        /// <summary>
        /// Deserialise the <paramref name="filePath"/> to a instance of <typeparamref name="type"/>.
        /// </summary>
        /// <returns>The from file.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="type">Type.</param>
        public static object DeserializeFromFile(string filePath, Type type)
        {
            var xml = File.ReadAllText(filePath);

            return DeserializeFromString(xml, type);
        }

        /// <summary>
        /// Deserialise the string <paramref name="content"/> to a <typeparamref name="TObject"/>.
        /// </summary>
        /// <returns>The from string.</returns>
        /// <param name="content">Content.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public static TObject DeserializeFromString<TObject>(string content) where TObject : class
        {
            return DeserializeFromString(content, typeof(TObject)) as TObject;
        }

        /// <summary>
        /// Deserialise the string <paramref name="content"/> to an instance of <typeparamref name="type"/>.
        /// </summary>
        /// <returns>The from string.</returns>
        /// <param name="content">Content.</param>
        /// <param name="type">Type.</param>
        public static object DeserializeFromString(string content, Type type)
        {
            object instance = null;

            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            try
            {
                var serializer = new XmlSerializer(type);
                using (TextReader reader = new StringReader(content))
                {
                    instance = serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return instance;
        }

        /// <summary>
        /// Deserialise the content of <paramref name="stream"/> to a <typeparamref name="TObject"/>.
        /// </summary>
        /// <returns>The from stream.</returns>
        /// <param name="stream">Stream.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public static TObject DeserializeFromStream<TObject>(StreamReader stream) where TObject : class
        {
            return DeserializeFromStream(stream, typeof(TObject)) as TObject;
        }

        /// <summary>
        /// Deserialise the content of <paramref name="stream"/> to an instance of <typeparamref name="type"/>.
        /// </summary>
        /// <returns>The from stream.</returns>
        /// <param name="stream">Stream.</param>
        /// <param name="type">Type.</param>
        public static object DeserializeFromStream(StreamReader stream, Type type)
        {
            var xml = stream?.ReadToEnd();

            return DeserializeFromString(xml, type);
        }
    }
}

