using System;
using MFractor.Images.Importing.Generators;
using MFractor.Images.Models;
using Xunit;

namespace MFractor.Images.Tests.Importing.Generators
{
    public class IOSAppIconFilenameGeneratorTests : BaseAppleAppIconFilenameGeneratorTests
    {
        protected override IAppIconFilenameGenerator Generator => new IOSAppIconFilenameGenerator();

        protected override IconSet IconGroup => IconSet.Notification;

        protected override IconIdiom Idiom => IconIdiom.IPhone;

        [Fact]
        public void Test_GenerateFileName_HasIdiom()
        {
            const string expectedContains = "-iphone";
            var iconImage = new IconImage(Idiom, IconGroup, ImageScale.Points_1x);
            var sut = Generator;

            var result = sut.Generate(iconImage);

            Assert.Contains(expectedContains, result);
        }

        [Fact]
        public void Test_GenerateFileName_HasRoleName()
        {
            const string expectedContains = "-notification";
            var iconImage = new IconImage(Idiom, IconGroup, ImageScale.Points_1x);
            var sut = Generator;

            var result = sut.Generate(iconImage);

            Assert.Contains(expectedContains, result);
        }
    }
}
