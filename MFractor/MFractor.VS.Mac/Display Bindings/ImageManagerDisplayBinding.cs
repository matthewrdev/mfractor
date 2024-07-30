using System;
using MFractor.Analytics;
using MFractor.Images;
using MFractor.Images.ImageManager;
using MFractor.IOC;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Workspace;
using MonoDevelop.Core;
using MonoDevelop.Ide.Desktop;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace MFractor.VS.Mac.DisplayBindings
{
    public class ImageManagerDisplayBinding : DesktopApplication, IExternalDisplayBinding, IAnalyticsFeature
    {
        IProjectService ProjectService => Resolver.Resolve<IProjectService>();

        IAnalyticsService AnalyticsService => Resolver.Resolve<IAnalyticsService>();

        IWorkEngine WorkEngine => Resolver.Resolve<IWorkEngine>();

        public const string ImageManagerDisplayBindingId = "MFractor.ImageManagerDisplayBinding";

        public ImageManagerDisplayBinding() : base(ImageManagerDisplayBindingId,
                                                   "Image Asset Manager", 
                                                   true)
        {
        }

        public ImageManagerDisplayBinding(string id, 
                                          string displayName, 
                                          bool isDefault) : base(id, displayName, isDefault)
        {
        }

        public bool CanHandle (FilePath fileName, string mimeType, Project ownerProject)
        {
            try
            {
                Project = ownerProject.ToCompilationProject();

                ProjectFile = ProjectService.GetProjectFileWithFilePath(Project, fileName);

                return ImageAssetHelper.IsImageAsset(ProjectFile);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public DesktopApplication GetApplication(FilePath fileName, string mimeType, Project ownerProject)
        {
            return this;
        }

        public override void Launch(params string[] files)
        {
            if (Project != null && ProjectFile != null)
            {
                WorkEngine.ApplyAsync(new Images.WorkUnits.OpenImageManagerWorkUnit()
                {
                    Solution = Project.Solution,
                    Options = ImageManagerOptions.Edit,
                    SelectedImageAsset = ProjectFile.FilePath,
                });

                AnalyticsService.Track(this);
            }
        }

        public string Name => "Image Asset Manager";

        public bool CanUseAsDefault => true;

        public Microsoft.CodeAnalysis.Project Project { get; private set; }

        public IProjectFile ProjectFile { get; private set; }

        public string AnalyticsEvent => "Image Asset Manager";
    }
}
