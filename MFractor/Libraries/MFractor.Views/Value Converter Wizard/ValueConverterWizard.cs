using System;
using Xwt;
using MFractor.Views.Branding;
using System.Collections.Generic;
using MFractor.Workspace;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Views.ValueConverterWizard
{
    public class ValueConverterWizard : Xwt.Dialog
    {
        VBox root;

        ValueConverterWizardControl valueConverterWizardControl;
        Button generateButton;

        public ProjectIdentifier Project { get; }
        public event EventHandler<ValueConverterGenerationEventArgs> OnGenerate;

        public ValueConverterWizard(ProjectIdentifier project, IXamlPlatform platform)
        {
            Title = "Value Converter Wizard";

            Width = 1080;
            Height = 720;

            Project = project;

            Build(platform);

            valueConverterWizardControl.Project = project;
        }

        void Build(IXamlPlatform platform)
        {
            root = new VBox();

            valueConverterWizardControl = new ValueConverterWizardControl(platform);

            root.PackStart(valueConverterWizardControl, true, true);

            generateButton = new Button("Generate Value Converter");
            generateButton.Clicked += GenerateButton_Clicked;

            root.PackStart(generateButton);

            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter("https://docs.mfractor.com/xamarin-forms/value-converters/value-converter-wizard/", "Value Converter Wizard"));

            Content = root;
        }

        public void Apply(string name, string inputType, string outputType)
        {
            valueConverterWizardControl.Apply(name, inputType, outputType);
        }

        public void SetXamlEntryTargetFiles(bool generateXamlEntry, IReadOnlyList<IProjectFile> projectFiles)
        {
            valueConverterWizardControl.SetXamlEntryTargetFiles(generateXamlEntry, projectFiles);
        }

        void GenerateButton_Clicked(object sender, EventArgs e)
        {
            if (!valueConverterWizardControl.IsValid)
            {
                return;
            }

            var options = valueConverterWizardControl.Options;

            var args = new ValueConverterGenerationEventArgs(options);
            OnGenerate?.Invoke(this, args);
        }
    }
}
