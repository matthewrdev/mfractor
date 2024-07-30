using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MFractor.IOC;
using MFractor.Models;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Importing
{
    /// <summary>
    /// Represents a project that can optionally be target of an operation.
    /// </summary>
    public class TargetProject : ObservableBase
    {
        public Project Project { get; }

        bool isSelected = true;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        bool isCleanup = true;
        public bool IsCleanup
        {
            get => isCleanup;
            set => SetProperty(ref isCleanup, value);
        }

        public TargetProject(Project project) => Project = project;
    }

    /// <summary>
    /// Represents an Icon Import operation. Can be used as a View Model.
    /// </summary>
    public class AppIconImport : ObservableBase
    {
        readonly IImageUtilities imageSizeUtils;

        List<TargetProject> projects = new List<TargetProject>();
        List<string> errorMessages = new List<string>();
        ImageSize? imageSize;


        #region Properties

        string imageFilePath;
        /// <summary>
        /// The path to file on the disk that haves the image to be imported as
        /// the Application icon on this operation.
        /// </summary>
        public string ImageFilePath
        {
            get => imageFilePath;
            set
            {
                SetProperty(ref imageFilePath, value);
                LoadImageMetadata();
                Validate();
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(IsValidImageFilePath));
            }
        }

        /// <summary>
        /// Gets a read-only list of the Error Messages that outcome from the validation.
        /// </summary>
        public IReadOnlyList<string> ErrorMessages => errorMessages;

        /// <summary>
        /// Gets a flag indicating if the Image File Path is valid.
        /// </summary>
        public bool IsValidImageFilePath => ValidateImageFilePath();

        /// <summary>
        /// Gets a flag indicating if the image has the minimum recommended size.
        /// </summary>
        public bool IsMinimumSize => MinimumSizeValidator();

        /// <summary>
        /// Gets a flag indicating if all the required parameters for the import
        /// operation is valid.
        /// </summary>
        public bool IsValid => Validate();

        /// <summary>
        /// Gets the validation errors as a detailed description.
        /// </summary>
        public string ValidationErrors => GetFormattedValidationErrors();

        /// <summary>
        /// Gets a read-only list of the selectable project targets.
        /// </summary>
        public IReadOnlyList<TargetProject> Projects => projects;

        #endregion

        public AppIconImport(IImageUtilities imageSizeUtils)
        {
            this.imageSizeUtils = imageSizeUtils;
        }

        public AppIconImport(IReadOnlyList<Project> projects, IImageUtilities imageSizeUtils)
        {
            this.imageSizeUtils = imageSizeUtils;
            this.projects.AddRange(
                projects
                    .Where(p => p.IsMobileProject())
                    .Select(p => new TargetProject(p))
            );
        }

        #region Support Methods

        void LoadImageMetadata()
        {
            if (IsValidImageFilePath)
            {
                imageSize = imageSizeUtils.GetImageSize(ImageFilePath);
            }
        }

        bool Validate()
        {
            var isValid = true;
            errorMessages.Clear();

            isValid &= ValidateImageFilePath();
            isValid &= ValidateWithMessage(TargetsValidator, "You must select at least one target project for the import operation.");
            isValid &= ValidateWithMessage(AspectRationValidator, "The image must have the same width and height.");

            return isValid;
        }

        bool ValidateWithMessage(Func<bool> validator, string message)
        {
            var isValid = validator();
            if (!isValid)
            {
                errorMessages.Add(message);
            }
            return isValid;
        }

        bool ValidateImageFilePath() => !string.IsNullOrWhiteSpace(ImageFilePath) && File.Exists(ImageFilePath);

        bool TargetsValidator() => projects.Any(p => p.IsSelected);

        bool AspectRationValidator() => imageSize != null && imageSize.Width == imageSize.Height;

        bool MinimumSizeValidator() => imageSize != null && imageSize.Width >= 1024 && imageSize.Height >= 1024;

        string GetFormattedValidationErrors()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Please correct the following issues to import the icon:");
            errorMessages.ForEach(m => builder.AppendLine($"- {m}"));
            return builder.ToString();
        }

        #endregion
    }
}
