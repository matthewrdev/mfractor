using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Ide.DeleteOutputFolders;
using MFractor.IOC;
using Microsoft.CodeAnalysis;
using Xwt;

namespace MFractor.Views.DeleteOutputFolders
{
    public delegate void DeleteOutputFoldersConfigurationConfirmedDelegate();

    public class DeleteOutputFoldersConfigurationDialog : Xwt.Dialog
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        public IDeleteOutputFoldersConfigurationService DeleteOutputFoldersConfigurationService {get;set;}

        VBox root;

        Label titleLabel;

        CheckBox deleteBinCheckbox;
        CheckBox deleteObjCheckbox;
        CheckBox deletePackagesCheckbox;
        CheckBox deleteVsWorkfolderCheckbox;

        Button confirmButton;

        public string ConfigName { get; private set; }
        string Identifier { get; set; }

        public DeleteOutputFoldersConfigurationConfirmedDelegate DeleteOutputFoldersConfigurationConfirmedDelegate { get; set; }

        public DeleteOutputFoldersConfigurationDialog(Project project)
        {
            Resolver.ComposeParts(this);

            var name = Path.GetFileName(project.FilePath);
            var identifier = DeleteOutputFoldersConfigurationService.GetIdentifier(project);
            var options = DeleteOutputFoldersConfigurationService.GetOptionsOrDefault(project);

            Setup(name, identifier, options);
        }

        public DeleteOutputFoldersConfigurationDialog(Solution solution)
        {
            Resolver.ComposeParts(this);

            var name = Path.GetFileName(solution.FilePath);
            var identifier = DeleteOutputFoldersConfigurationService.GetIdentifier(solution);
            var options = DeleteOutputFoldersConfigurationService.GetOptionsOrDefault(solution);

            Setup(name, identifier, options);
        }

        public DeleteOutputFoldersConfigurationDialog(IDeleteOutputFoldersConfiguration configuration)
        {
            Resolver.ComposeParts(this);

            Setup(configuration.Name, configuration.Identifier, configuration.Options);
        }

        void Setup(string name, string identifier, IDeleteOutputFoldersOptions options)
        {
            Width = 400;

            Title = "Configure Delete Output Folders";

            Build();

            Apply(options);

            titleLabel.Text = "Please choose which folders you would like to delete:";
            titleLabel.TooltipText = "This will configure the folders that MFractor will delete each time Delete Output Folders is used.\n\nYou can configure or reset these settings under MFractor -> Preferences -> Delete Output Folders.";

            ConfigName = name;
            Identifier = identifier;
        }

        void Apply(IDeleteOutputFoldersOptions options)
        {
            if (options == null)
            {
                Apply(true, true, false, false);
            }
            else
            {
                Apply(options.DeleteBin, options.DeleteObj, options.DeletePackages, options.DeleteVisualStudioWorkingFolder);
            }
        }

        void Apply(bool deleteBin, bool deleteObj, bool deletePackages, bool deleteVisualStudioWorkingFolder)
        {
            deleteBinCheckbox.Active = deleteBin;
            deleteObjCheckbox.Active = deleteObj;
            deletePackagesCheckbox.Active = deletePackages;
            deleteVsWorkfolderCheckbox.Active = deleteVisualStudioWorkingFolder;
        }

        void Build()
        {
            root = new VBox();

            titleLabel = new Label();
            titleLabel.Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold);

            root.PackStart(titleLabel);

            deleteBinCheckbox = new CheckBox() { Label = "Delete bin folder", TooltipText = "Delete the bin folder that contains the output binaries." };
            deleteObjCheckbox = new CheckBox() { Label = "Delete obj folder", TooltipText = "Delete the intermediate files contained in the obj folder. Please also enable 'Delete packages cache' to also remove cached nuget packages." };
            deletePackagesCheckbox = new CheckBox() { Label = "Delete packages cache", TooltipText = "Delete the nuget packages cache that is within the obj folder." };
            deleteVsWorkfolderCheckbox = new CheckBox() { Label = "Delete .vs working folder", TooltipText = "Delete the .vs working folder. Only applicable to solutions" };

            root.PackStart(deleteBinCheckbox);
            root.PackStart(deleteObjCheckbox);
            root.PackStart(deletePackagesCheckbox);
            root.PackStart(deleteVsWorkfolderCheckbox);

            confirmButton = new Button()
            {
                Label = "Apply and delete output folders"
            };

            confirmButton.Clicked += ConfirmButton_Clicked;

            root.PackStart(confirmButton);

            root.PackStart(new HSeparator());
            root.PackStart(new Branding.BrandedFooter("http://docs.mfractor.com/utilities/delete-output-folders/", "Delete Output Folders"));

            Content = root;
        }

        void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            var options = new MutableDeleteOutputFoldersOptions()
            {
                DeleteBin = deleteBinCheckbox.Active,
                DeleteObj = deleteObjCheckbox.Active,
                DeletePackages = deletePackagesCheckbox.Active,
                DeleteVisualStudioWorkingFolder = deleteVsWorkfolderCheckbox.Active,
            };

            DeleteOutputFoldersConfigurationService.SetOptions(ConfigName, Identifier, options);

            try
            {
                DeleteOutputFoldersConfigurationConfirmedDelegate?.Invoke();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            Close();
            Dispose();
        }
    }
}
