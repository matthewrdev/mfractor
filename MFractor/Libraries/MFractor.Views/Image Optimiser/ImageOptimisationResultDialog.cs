using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MFractor.Images.Optimisation;
using MFractor.Utilities;
using MFractor.Views.Branding;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Xwt;

namespace MFractor.Views.ImageOptimiser
{
    public class ImageOptimisationResultDialog : Dialog
    {
        VBox root;

        ListView imagesListView;
        ListStore imagesListDataStore;
        readonly DataField<string> imageNameField = new DataField<string>();
        readonly DataField<string> sizeBeforeField = new DataField<string>();
        readonly DataField<string> sizeAfterField = new DataField<string>();
        readonly DataField<string> sizeSavingsField = new DataField<string>();

        Button confirmButton;

        public List<OptimisationResult> Results { get; }

        public ImageOptimisationResultDialog(params OptimisationResult[] results)
            : this((IEnumerable<OptimisationResult>)results)
        {
        }

        public ImageOptimisationResultDialog(IEnumerable<OptimisationResult> results)
        {
            Title = "Image Optimisation Results";

            Results = results?.ToList() ?? new List<OptimisationResult>();

            Build();

            Populate();

            Width = 600;
            Height = 300;
        }

        void Build()
        {
            root = new VBox();

            long sizeBefore = 0;
            long sizeAfter = 0;

            foreach (var result in Results)
            {
                sizeBefore += result.SizeBeforeInBytes;
                sizeAfter += result.SizeAfterInBytes;
            }

            var projectsCount = LinqExtensions.DistinctBy(Results, (OptimisationResult r) => r.ProjectFile.CompilationProject).Count();
            var pluralisation = projectsCount == 1 ? " project" : " projects";

            var count = Results.Count;

            var filesPluralisation = count == 1 ? " file" : " files";

            var differenceBytes = sizeBefore - sizeAfter;

            var differenceSize = MFractor.Utilities.FileSizeHelper.GetFormattedFileSize(differenceBytes);

            var percentage = ((double)sizeAfter / (double)sizeBefore) * 100.0;

            var savedPercent = 100.0 - percentage;

            var savings = " (" + String.Format("{0:0.#}", savedPercent) + "%/" + differenceSize + " smaller)";

            var message = "MFractor has finished optimising " + count + filesPluralisation + " in " + projectsCount + pluralisation + savings + ".";

            var summaryLabel = new Label(message);

            root.PackStart(summaryLabel);

            BuildSummaryList();

            root.PackStart(new VSeparator());

            confirmButton = new Button("Ok");
            confirmButton.Clicked += (sender, e) =>
            {
                this.Close();
            };
            root.PackEnd(new BrandedFooter("https://docs.mfractor.com/image-management/optimising-image-assets/", "Image Optimisation"));
            root.PackEnd(confirmButton);

            this.Content = root;
        }

        void Populate()
        {
            imagesListDataStore.Clear();

            foreach (var result in Results)
            {
                var name = result.ProjectFile.CompilationProject.Name + " - " + result.ProjectFile.VirtualPath;
                var row = imagesListDataStore.AddRow();

                if (result.Success)
                {
                    imagesListDataStore.SetValue(row, imageNameField, name);
                    imagesListDataStore.SetValue(row, sizeBeforeField, result.BeforeSize);
                    imagesListDataStore.SetValue(row, sizeAfterField, result.AfterSize);
                    imagesListDataStore.SetValue(row, sizeSavingsField, result.DifferenceSummary);
                }
                else
                {
                    imagesListDataStore.SetValue(row, imageNameField, name + "(Failed - " + result.FailureMessage + ")");

                    imagesListDataStore.SetValue(row, sizeBeforeField, string.Empty);
                    imagesListDataStore.SetValue(row, sizeAfterField, string.Empty);
                    imagesListDataStore.SetValue(row, sizeSavingsField, string.Empty);
                }
            }

            if (Results.Any())
            {
                imagesListView.SelectRow(0);
            }
        }


        IProjectFile SelectedFile
        {
            get
            {
                if (Results == null)
                {
                    return null;
                }

                if (imagesListView.SelectedRow < 0 || imagesListView.SelectedRow >= Results.Count)
                {
                    return null;
                }

                var selection = Results[imagesListView.SelectedRow];

                return selection.ProjectFile;
            }
        }

        void BuildSummaryList()
        {
            imagesListView = new ListView();

            imagesListDataStore = new ListStore(imageNameField, sizeBeforeField, sizeAfterField, sizeSavingsField);
            imagesListView.DataSource = imagesListDataStore;

            imagesListView.Columns.Add("Image", new TextCellView(imageNameField)
            {
                Editable = false,
            });

            imagesListView.Columns.Add("Before", new TextCellView(sizeBeforeField)
            {
                Editable = false,
            });

            imagesListView.Columns.Add("After", new TextCellView(sizeAfterField)
            {
                Editable = false,
            });

            imagesListView.Columns.Add("Result", new TextCellView(sizeSavingsField)
            {
                Editable = false,
            });

            imagesListView.ButtonPressed += (sender, e) =>
            {
                if (e.Button == PointerButton.Left && e.MultiplePress == 2 && SelectedFile != null)
                {
                    Process.Start(SelectedFile.FilePath);
                }
            };

            imagesListView.WidthRequest = 500;

            root.PackStart(imagesListView, true, true);
        }
    }
}