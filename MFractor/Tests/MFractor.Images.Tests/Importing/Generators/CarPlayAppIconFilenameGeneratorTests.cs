using System;
using System.Collections.Generic;
using MFractor.Images.Importing.Generators;
using MFractor.Images.Models;
using Xunit;

namespace MFractor.Images.Tests.Importing.Generators
{
    public class CarPlayAppIconFilenameGeneratorTests : BaseAppleAppIconFilenameGeneratorTests
    {
        protected override IAppIconFilenameGenerator Generator => new CarPlayAppIconFilenameGenerator();

        protected override IconSet IconGroup => IconSet.CarPlay;

        protected override IconIdiom Idiom => IconIdiom.CarPlay;

        [Fact]
        public void Test_GenerateFilename_HasCorrectFormat()
        {
            const string expectedStartsWith = "appicon";
            const string expectedContains = "-car";
            var iconImage = new IconImage(Idiom, IconSet.CarPlay, ImageScale.Points_2x);
            var sut = new CarPlayAppIconFilenameGenerator();

            var result = sut.Generate(iconImage);

            Assert.StartsWith(expectedStartsWith, result);
            Assert.Contains(expectedContains, result);
        }

        [Fact]
        public void Test_GenerateFilename_ShouldThrowException_WhenNameIsNull()
        {
            var iconImage = new IconImage(Idiom, IconSet.CarPlay, ImageScale.Points_2x);
            iconImage.Name = null;
            var sut = new CarPlayAppIconFilenameGenerator();

            Assert.Throws<ArgumentNullException>(() => _ = sut.Generate(iconImage));
        }
    }
}
