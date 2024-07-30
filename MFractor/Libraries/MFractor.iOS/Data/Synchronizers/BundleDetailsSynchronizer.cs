using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using MFractor.iOS.Data.Models;
using MFractor.iOS.Data.Repositories;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Synchronisation;
using Microsoft.CodeAnalysis;

namespace MFractor.iOS.Data.Synchronizers
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class BundleDetailsSynchronizer : ITextResourceSynchroniser
    {
        public string[] SupportedFileExtensions { get; } = new string[] { ".plist" };

        public Task<bool> CanSynchronise(Solution solution, Project project, IProjectFile projectFile)
        {
            return Task.FromResult(projectFile.Name == "Info.plist");
        }

        public bool IsAvailable(Solution solution, Project project)
        {
            return project.IsIOSProject();
        }

        public async Task<bool> Synchronise(
            Solution solution, 
            Project project, 
            IProjectFile projectFile, 
            ITextProvider textProvider, 
            IProjectResourcesDatabase database)
        {
            var keyValues = GetBundleEntries(textProvider.GetText());
            var bundleIdentifier = GetBundleIdentifier(keyValues);

            var repo = database.GetRepository<BundleDetailsRepository>();
            var details = new BundleDetails();
            details.BundleIdentifier = bundleIdentifier;
            repo.Insert(details);

            return false;
        }
        
        IReadOnlyDictionary<string, string> GetBundleEntries(string content) => 
            XDocument.Parse(content)
                .Descendants("dict")
                .SelectMany(d => d.Elements("key").Zip(d.Elements().Where(e => e.Name != "key"), (k, v) => new { Key = k, Value = v }))
                .ToDictionary(i => i.Key.Value, i => i.Value.Value);

        string GetBundleIdentifier(IReadOnlyDictionary<string, string> keyValues) => 
            keyValues.TryGetValue("CFBundleIdentifier", out var id) ? id : default;

    }
}
