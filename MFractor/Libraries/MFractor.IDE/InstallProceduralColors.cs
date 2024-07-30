using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using MFractor.Utilities;

namespace MFractor.Ide
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class InstallColorIcons : IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IIdeImageManager> ideImageManager;
        public IIdeImageManager IdeImageManager => ideImageManager.Value;

        readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        [ImportingConstructor]
        public InstallColorIcons(Lazy<IIdeImageManager> ideImageManager,
                                 Lazy<IApplicationPaths> applicationPaths)
        {
            this.ideImageManager = ideImageManager;
            this.applicationPaths = applicationPaths;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            Task.Run(() =>
            {
                var icons = new List<ImageEntry>();
                try
                {
                    var colors = ColorHelper.GetAllSystemDrawingColors();
                    var colorsFolder = Path.Combine(ApplicationPaths.ApplicationDataPath, "colors");

                    if (!Directory.Exists(colorsFolder))
                    {
                        Directory.CreateDirectory(colorsFolder);
                    }

                    foreach (var c in colors)
                    {
                        var guid = Guid.NewGuid();
                        var name = "color-" + c.Key.ToLower();
                        var fileName = name + ".png";

                        var colorFilePath = Path.Combine(colorsFolder, fileName);

                        var assembly = GetType().Assembly;
                        var colorAsset = ResourcesHelper.LocateMatchingResourceId(assembly, name);

                        if (string.IsNullOrEmpty(colorAsset))
                        {
                            // Non existant color rendering.
                            continue;
                        }

                        if (File.Exists(colorFilePath) == false)
                        {
                            ResourcesHelper.ExportResourceContentToFile(assembly, colorAsset, colorFilePath);
                        }

                        icons.Add(new ImageEntry(name, colorFilePath, guid));
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }

                IdeImageManager.AddImages(icons);

                return true;

            });
        }
    }
}
