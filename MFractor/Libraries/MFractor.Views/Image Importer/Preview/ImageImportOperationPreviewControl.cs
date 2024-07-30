using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Images;
using MFractor.Images.Utilities;
using MFractor.IOC;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ImageImporter.Preview
{
    public class ImageImportOperationPreviewControl : ScrollView
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        IImageUtilities ImageUtil { get; set; }

        readonly HBox previewPanel;
        readonly Label previewMessage;

        List<OutputImageSummary> outputImageSummaries = new List<OutputImageSummary>();

        //public string PreviewPlaceholderMessage
        //{
        //    get => previewPlaceholderMessage;
        //    set
        //    {

        //    }
        //}

        public ImageImportOperationPreviewControl()
        {
            Resolver.ComposeParts(this);

            ExpandHorizontal = true;
            ExpandVertical = true;
            HorizontalScrollPolicy = ScrollPolicy.Always;
            VerticalScrollPolicy = ScrollPolicy.Never;

            previewPanel = new HBox()
            {
                HeightRequest = 140,
            };

            previewMessage = new Label()
            {
                Font = Font.SystemFont.WithSize(16).WithWeight(FontWeight.Bold),
                TextAlignment = Alignment.Start,
                VerticalPlacement = WidgetPlacement.Center,
                ExpandHorizontal = true,
            };

            previewPanel.PackStart(previewMessage);

            Content = previewPanel;

            ApplyPreviewPanelMessage("Please choose an image and select a project to preview the output images");
        }

        void ApplyPreviewPanelMessage(string message)
        {
            previewMessage.Text = message;
            previewMessage.Visible = !string.IsNullOrEmpty(message);
        }

        public void Update(Project selection, SourceImage image, IEnumerable<ImportImageOperation> operations)
        {
            var operation = operations.FirstOrDefault(op => op.TargetProject == selection);

            Update(selection, image, operation);
        }

        public void Update(Project selection, SourceImage image, ImportImageOperation operation)
        {
            foreach (var panel in outputImageSummaries)
            {
                panel.Visible = false;
            }

            ApplyPreviewPanelMessage(string.Empty);

            if (selection == null || image == null || image.Exists == false)
            {
                ApplyPreviewPanelMessage("Please choose an image and select a project to preview the output images");

                return;
            }

            if (operation == null)
            {
                ApplyPreviewPanelMessage("The Image Importer ran a problem while preparing the image previews.");
                return;
            }

            try
            {
                var sourceImageSize = ImageUtil.GetImageSize(operation.AnyAppearanceImageFilePath);
                if (operation.SourceSize != null)
                {
                    sourceImageSize = operation.SourceSize;
                }

                var rawImage = image.Image;

                var densities = operation.Densities;
                var index = 0;
                foreach (var density in densities.OrderByDescending(d => d.Scale))
                {
                    var summaryControl = index < outputImageSummaries.Count ? outputImageSummaries[index] : new OutputImageSummary();

                    if (previewPanel.Children != null && previewPanel.Children.Any() && previewPanel.Children.Contains(summaryControl))
                    {
                        previewPanel.Remove(summaryControl);
                    }
                    summaryControl.Visible = true;

                    var virtualPath = ImageDownsamplingHelper.GetVirtualFilePath(operation, density, false);

                    var newSize = ImageDownsamplingHelper.GetTransformedImageSize(sourceImageSize,
                                                                                  density.Scale,
                                                                                  operation.SourceDensity.Scale);

                    var newScale = (density.Scale / operation.SourceDensity.Scale);

                    summaryControl.Update(virtualPath, newSize, rawImage, newScale);

                    previewPanel.PackStart(summaryControl);
                    if (!outputImageSummaries.Contains(summaryControl))
                    {
                        outputImageSummaries.Add(summaryControl);
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                ApplyPreviewPanelMessage("The Image Importer ran into a problem while preparing the image previews.\n" + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var summary in outputImageSummaries)
            {
                summary.Dispose();
            }

            outputImageSummaries.Clear();
        }
    }
}
