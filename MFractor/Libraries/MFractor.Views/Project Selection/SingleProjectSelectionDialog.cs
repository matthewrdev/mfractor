using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Views.Branding;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ProjectSelection
{
    public class SingleProjectSelectionDialog : Dialog
    {
        VBox container;

        HBox choicesTitleContainer;
        Label choicesTitleLabel;
        Label messagesLabel;

        ComboBox projectSelector;

        Button confirmSelectionButton;

        IReadOnlyList<Project> Projects { get; }
        public string Message { get; }

        public ProjectSelectorMode Mode { get; }

        public event EventHandler<ProjectSelectionEventArgs> OnProjectsSelected;

        public SingleProjectSelectionDialog(IReadOnlyList<Project> projects,
                                            string title,
                                            string message)
        {
            Title = title;
            Icon = Image.FromResource("mfractor_logo.png");

            this.Projects = projects;
            Message = message;
            container = new VBox();

            Build();

            container.PackStart(new HSeparator());
            container.PackStart(new BrandedFooter());

            this.Content = container;

            projectSelector.SelectedItem = projects.First();
        }

        void Build()
        {
            container = new VBox();

            choicesTitleContainer = new HBox();

            choicesTitleLabel = new Label()
            {
                Text = "Projects",
                HeightRequest = 30,
                Font = Font.SystemFont.WithSize(20).WithWeight(FontWeight.Bold),
            };
            choicesTitleContainer.PackStart(choicesTitleLabel);

            container.PackStart(choicesTitleContainer);

            container.PackStart(new HSeparator());

            if (!string.IsNullOrEmpty(Message))
            {
                messagesLabel = new Label();
                messagesLabel.Text = Message;
                container.PackStart(messagesLabel);
                container.PackStart(new HSeparator());
            }

            projectSelector = new ComboBox();
            foreach (var p in Projects)
            {
                projectSelector.Items.Add(p, p.Name);
            }
            container.PackStart(projectSelector);

            confirmSelectionButton = new Button()
            {
                Label = "Confirm",
                HeightRequest = 30,
                Font = Font.SystemFont.WithSize(20).WithWeight(FontWeight.Bold),
            };
            confirmSelectionButton.Clicked += (sender, e) =>
            {
                ConfirmProjectSelection();
            };

            container.PackStart(confirmSelectionButton);
        }

        Project GetChosenProject()
        {
            return projectSelector.SelectedItem as Project;
        }

        bool ConfirmProjectSelection()
        {
            var selection = GetChosenProject();

            var args = new ProjectSelectionEventArgs(selection);

            this.OnProjectsSelected?.Invoke(this, args);

            this.Close();
            return true;
        }
    }
}
