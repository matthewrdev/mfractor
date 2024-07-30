using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Views.Branding;
using MFractor.Views.Controls.Collection;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ProjectSelection
{
    public class MultiProjectSelectionDialog : Dialog
    {
        VBox container;

        HBox choicesTitleContainer;
        Label choicesTitleLabel;
        Label messagesLabel;

        CollectionView collectionView;

        Button confirmSelectionButton;

        public string Message { get; }

        public ProjectSelectorMode Mode { get; }

        public event EventHandler<ProjectSelectionEventArgs> OnProjectsSelected;

        public MultiProjectSelectionDialog(IReadOnlyList<Project> projects,
                                           IReadOnlyList<Project> enabledProjects,
                                           string title,
                                           string message)
        {
            Title = title;
            Icon = Image.FromResource("mfractor_logo.png");

            Size = new Size(640, 480);
            Message = message;
            container = new VBox();

            Build();

            container.PackStart(new HSeparator());
            container.PackStart(new BrandedFooter());

            this.Content = container;

            collectionView.Items = projects.Select(p => new ProjectSelectionCollectionItem(p) { IsChecked = enabledProjects.Contains(p) }).ToList();
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

            var options = new CollectionViewOptions().WithSelectionCheckboxColumn("Include")
                                                     .WithPrimaryLabelColumn("Project");

            collectionView = new CollectionView(options);

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

        bool ConfirmProjectSelection()
        {
            var selection = collectionView.Items.OfType<ProjectSelectionCollectionItem>().Where(i => i.IsChecked).Select(i => i.Project).ToList();

            var args = new ProjectSelectionEventArgs(selection);

            this.OnProjectsSelected?.Invoke(this, args);

            this.Close();
            return true;
        }
    }
}
