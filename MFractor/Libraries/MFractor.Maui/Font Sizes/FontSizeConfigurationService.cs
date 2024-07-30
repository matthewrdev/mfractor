using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml;
using MFractor.Utilities;

namespace MFractor.Maui.FontSizes
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontSizeConfigurationService))]
    public class FontSizeConfigurationService : IFontSizeConfigurationService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        const string mappingXml = "Resources.Configuration.FontSizeMapping.xml";

        readonly Dictionary<string, IFontSize> fontSizes = new Dictionary<string, IFontSize>();
        public IReadOnlyDictionary<string, IFontSize> FontSizes => fontSizes;

        public string HelpUrl { get; private set; }

        public IFontSize GetNamedFontSize(string name)
        {
            if (FontSizes.TryGetValue(name, out var value))
            {
                return value;
            }

            return default;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            LoadFontSizeMappings();
        }

        void LoadFontSizeMappings()
        {
            try
            {
                var resource = ResourcesHelper.LocateMatchingResourceId(GetType().Assembly, mappingXml);

                var content = ResourcesHelper.ReadResourceContent(this, resource);

                try
                {
                    var document = new XmlDocument();
                    document.LoadXml(content);

                    var root = document.SelectSingleNode(@".//FontSizes");
                    var helpAttr = root.Attributes.GetNamedItem("Url");
                    this.HelpUrl = helpAttr.Value;

                    var nodes = root.SelectNodes(@".//FontSize");

                    foreach (var node in nodes)
                    {
                        var map = node as XmlNode;

                        if (map != null)
                        {
                            var nameAttr = map.Attributes.GetNamedItem("Name");
                            var iosAttr = map.Attributes.GetNamedItem("iOS");
                            var androidAttr = map.Attributes.GetNamedItem("Android");
                            var uwpAttr = map.Attributes.GetNamedItem("UWP");

                            if (nameAttr == null
                                 || iosAttr == null
                                 || androidAttr == null
                                 || uwpAttr == null)
                            {
                                // TODO: Log?
                                continue;
                            }

                            if (!double.TryParse(iosAttr.Value, out var ios)
                                || !double.TryParse(androidAttr.Value, out var android)
                                || !double.TryParse(uwpAttr.Value, out var uwp))
                            {
                                // TODO: Log?
                                continue;
                            }

                            fontSizes[nameAttr.Value] = new FontSize(nameAttr.Value, ios, android, uwp);
                        }
                    }

                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public bool TryGetNamedFontSize(string name, out IFontSize fontSize)
        {
            fontSize = null;

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return FontSizes.TryGetValue(name, out fontSize);
        }
    }
}