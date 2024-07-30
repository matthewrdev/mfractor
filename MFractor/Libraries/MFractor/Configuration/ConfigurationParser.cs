using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;

namespace MFractor.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IConfigurationParser))]
    class ConfigurationParser : IConfigurationParser
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        internal class PropertySetting : IPropertySetting
        {
            public string Name { get; }
            public string Value { get; }
            public IConfigurationSource Parent { get; }
            public PropertyAssignmentType AssignmentType { get; }
            public Dictionary<string, string> MetaData { get; }

            public string ConfigurationId { get; }

            public PropertySetting(string name,
                                   string value,
                                   IConfigurationSource parent,
                                   PropertyAssignmentType assignmentType,
                                   string configurationId,
                                   Dictionary<string, string> metaData)
            {
                Name = name;
                Value = value;
                Parent = parent;
                AssignmentType = assignmentType;
                ConfigurationId = configurationId;
                MetaData = metaData ?? new Dictionary<string, string>();
            }
        }

        internal class ConfigurationSet : IConfigurationSource
        {

            public ConfigurationSet(ConfigurationId id,
                                    ConfigurationId parentId,
                                    string name,
                                    string filePath,
                                    ConfigurationScope scope)
            {
                Name = name;
                FilePath = filePath;
                Scope = scope;
                Id = id;
                ParentId = parentId;
            }

            public string FilePath { get; }

            public string Name { get; set; }

            public ConfigurationScope Scope { get; }

            public Dictionary<string, List<IPropertySetting>> Settings
            {
                get;
                set;
            }

            public ConfigurationId Id { get; }

            public ConfigurationId ParentId { get; }
        }

        public IConfigurationSource Parse(ConfigurationId id, 
                                          ConfigurationId parentId, 
                                          string filePath, 
                                          ConfigurationScope scope)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return Parse(id, parentId, filePath, stream, scope);
            }
        }

        public IConfigurationSource Parse(ConfigurationId id, 
                                          ConfigurationId parentId, 
                                          string filePath, 
                                          Stream content,
                                          ConfigurationScope scope)
        {
            IConfigurationSource set = null;
            try
            {
                var xml = new XmlDocument();
                xml.Load(content);

                set = CreateConfigurationSet(id, parentId, filePath, xml, scope);
            }
            catch (Exception ex)
            {
                log?.Warning($"Failed to parse the configuration file '{filePath}' with id {id}. Reason: {ex.ToString()}");
            }

            return set;
        }

        public IConfigurationSource Parse(ConfigurationId id, 
                                          ConfigurationId parentId, 
                                          string filePath, 
                                          string content, 
                                          ConfigurationScope scope)
        {
            IConfigurationSource set = null;
            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(content);

                set = CreateConfigurationSet(id, parentId, filePath, xml, scope);
            }
            catch (Exception ex)
            {
                log?.Info($"Failed to parse the configuration file '{filePath}' with id {id}. Reason: {ex.ToString()}");
            }

            return set;
        }

        private IConfigurationSource CreateConfigurationSet(ConfigurationId id, 
                                                            ConfigurationId parentId,
                                                            string filePath, 
                                                            XmlDocument document,
                                                            ConfigurationScope scope)
        {
            var packageName = "";
            var mfractorNodes = document.SelectNodes(@"/mfractor");
            if (mfractorNodes != null && mfractorNodes.Count > 0)
            {
                var rootNode = mfractorNodes[0];
                var packageNameAttr = rootNode.Attributes.GetNamedItem("package");
                if (packageNameAttr != null)
                {
                    packageName = packageNameAttr.Value;
                }
            }

            var set = new ConfigurationSet(id, parentId, packageName, filePath, scope);
            var configureNodes = document.SelectNodes(@".//configure");
            var suppressionNodes = document.SelectNodes(@".//suppress");

            var settings = new Dictionary<string, List<IPropertySetting>>();

            foreach (var n in configureNodes)
            {

                if (n is XmlNode configure)
                {
                    var idAttr = configure.Attributes.GetNamedItem("id");

                    if (idAttr == null)
                    {
                        continue;
                    }

                    var props = new List<PropertySetting>();

                    var properties = configure.SelectNodes(".//property");

                    foreach (var p in properties)
                    {
                        var property = p as XmlNode;

                        if (property != null)
                        {
                            var propertyName = property.Attributes.GetNamedItem("name");
                            var propertyValue = property.Attributes.GetNamedItem("value");

                            var attributes = property.Attributes;

                            var assignmentType = PropertyAssignmentType.AttributeLiteral;
                            var value = "";
                            if (propertyValue == null)
                            {
                                assignmentType = PropertyAssignmentType.NodeInnerValue;
                                value = property.InnerText;
                            }
                            else
                            {
                                value = propertyValue.Value;
                            }

                            var metaData = new Dictionary<string, string>();
                            for (var i = 0; i < property.Attributes.Count; ++i)
                            {
                                var attr = property.Attributes[i];

                                if (attr.Name == "name" || attr.Name == "value")
                                {
                                    continue;
                                }

                                metaData.Add(attr.Name, attr.Value);
                            }

                            props.Add(new PropertySetting(propertyName.Value, value, set, assignmentType, idAttr.Value, metaData));
                        }
                    }

                    settings.Add(idAttr.Value, props.Cast<IPropertySetting>().ToList());
                }
            }

            set.Settings = settings;

            return set;
        }
    }
}
