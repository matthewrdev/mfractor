using System;
using MFractor.Maui.XamlPlatforms;
using MFractor.Views.Branding;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.MVVMWizard.Settings
{
    public class ProjectMvvmSettingsDialog : Dialog
    {
        public event EventHandler<ProjectMvvmSettingsSavedEventArgs> MvvmOptionsSaved;

        VBox root;

        Label projectNameLabel;
        ProjectMvvmSettingsControl mvvmSettingsControl;

        Button saveButton;

        public ProjectMvvmSettingsDialog()
        {
            Title = "Project MVVM Settings";

            Width = 640;

            Build();

            BindEvents();
        }

        void BindEvents()
        {
            UnbindEvents();

            saveButton.Clicked += SaveButton_Clicked;
            mvvmSettingsControl.MvvmOptionsSaved += MvvmSettingsControl_MvvmOptionsSaved;
        }

        void MvvmSettingsControl_MvvmOptionsSaved(object sender, ProjectMvvmSettingsSavedEventArgs e)
        {
            MvvmOptionsSaved?.Invoke(this, e);

            this.Close();
            this.Dispose();
        }

        void SaveButton_Clicked(object sender, EventArgs e)
        {
            mvvmSettingsControl.Save();
        }

        void UnbindEvents()
        {
            saveButton.Clicked -= SaveButton_Clicked;
            mvvmSettingsControl.MvvmOptionsSaved -= MvvmSettingsControl_MvvmOptionsSaved;
        }

        void Build()
        {
            root = new VBox();

            projectNameLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Bold),
            };

            root.PackStart(projectNameLabel);
            root.PackStart(new HSeparator());

            mvvmSettingsControl = new ProjectMvvmSettingsControl();
            root.PackStart(mvvmSettingsControl);

            root.PackStart(new HSeparator());

            saveButton = new Button("Save");
            root.PackStart(saveButton);

            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter());

            Content = root;
        }

        public void Load(ProjectIdentifier projectIdentifier, IXamlPlatform platform)
        {
            projectNameLabel.Text = "Project: " + projectIdentifier.Name;

            mvvmSettingsControl.Load(projectIdentifier, platform);
        }
    }
}
