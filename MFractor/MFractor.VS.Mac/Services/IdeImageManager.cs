using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Ide;
using Microsoft.VisualStudio.Core.Imaging;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IIdeImageManager))]
    class IdeImageManager : IIdeImageManager
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Dictionary<string, Guid> nameToGuidMap = new Dictionary<string, Guid>();

        public Guid ImageTooltipId { get; } = Guid.Parse("ee3e3211-e992-4e0a-822b-576eb5fbe1fe");

        public bool HasImage(string name)
        {
            return ImageService.HasIcon(name);
        }

        public void AddImage(ImageEntry iconEntry)
        {
            if (iconEntry != null)
            {
                AddImage(iconEntry.Name, iconEntry.FilePath, iconEntry.Guid);
            }
            else
            {
                log?.Warning("Null icon entry provided to ImageService");
            }
        }

        public void AddImages(IReadOnlyList<ImageEntry> icons)
        {
            if (icons == null || !icons.Any())
            {
                return;
            }

            AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                foreach (var icon in icons.Where(i => i != null))
                {
                    RegisterIdeImage(icon.Name, icon.FilePath, icon.Guid);
                }
            });
        }

        void RegisterIdeImage(string name, string filePath, Guid guid)
        {
            if (string.IsNullOrEmpty(name)
                || string.IsNullOrEmpty(filePath)
                || File.Exists(filePath))
            {
                return;
            }

            try
            {
                var image = Xwt.Drawing.Image.FromFile(filePath);

                ImageService.AddImage(new ImageId(guid, guid.GetHashCode()), image);

                if (!ImageService.HasIcon(name))
                {
                    ImageService.AddIcon(name, image);
                }
                nameToGuidMap[name] = guid;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public bool HasImage(Guid guid)
        {
            return nameToGuidMap.Any(kp => kp.Value == guid);
        }

        public void AddImage(string name, string filePath, Guid guid)
        {
            AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                RegisterIdeImage(name, filePath, guid);
            });
        }

        public Guid GetGuid(string name)
        {
            if (nameToGuidMap.TryGetValue(name, out var guid))
            {
                return guid;
            }

            return default;
        }

        public string GetImage(Guid guid)
        {
            return nameToGuidMap.FirstOrDefault(kp => kp.Value == guid).Key;
        }

        public void SetImageTooltipFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));
            }

            try
            {

                var imageId = new ImageId(ImageTooltipId, ImageTooltipId.GetHashCode());
                var image = Xwt.Drawing.Image.FromFile(filePath);
                ImageService.AddImage(imageId, image);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
