using System;
using System.IO;
using MFractor.Images.Importing;
using MFractor.Images.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Xunit;

namespace MFractor.Images.Tests.Importing
{
    public class AppIconImportTests
    {
        const string regularSizeIconFilename = "./Resources/appicon-1024.png";
        const string downsizeIconFilename = "./Resources/appicon-512.png";
        const string irregularSizeIconFilename = "./Resources/appicon-irregular.png";

        [Fact]
        public void ImageFile_RegularSize_IsValid()
        {
            var sut = GetAppIconImport();

            sut.ImageFilePath = regularSizeIconFilename;

            Assert.True(sut.IsValid);
        }

        [Fact]
        public void ImageFile_Downsize_IsInvalid()
        {
            var sut = GetAppIconImport();

            sut.ImageFilePath = downsizeIconFilename;

            Assert.False(sut.IsValid);
        }

        [Fact]
        public void ImageFile_Downsize_HasErrorMessage()
        {
            var sut = GetAppIconImport();

            sut.ImageFilePath = downsizeIconFilename;

            Assert.Contains("at least 1024x1024", sut.ValidationErrors);
        }

        [Fact]
        public void ImageFile_IrregularSize_IsInvalid()
        {
            var sut = GetAppIconImport();

            sut.ImageFilePath = irregularSizeIconFilename;

            Assert.False(sut.IsValid);
        }

        [Fact]
        public void ImageFile_IrregularSize_HasErrorMessage()
        {
            var sut = GetAppIconImport();

            sut.ImageFilePath = irregularSizeIconFilename;

            Assert.Contains("aspect ratio must be 1:1", sut.ValidationErrors);
        }

        AppIconImport GetAppIconImport()
        {
            //var project = new Project();
            return new AppIconImport(GetImageUtils());
        }

        IImageUtilities GetImageUtils() => new XImageSizeUtilities();

    }
}
