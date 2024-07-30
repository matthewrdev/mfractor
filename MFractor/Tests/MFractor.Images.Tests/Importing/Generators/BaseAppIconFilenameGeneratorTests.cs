using System;
using System.Collections.Generic;
using MFractor.Images.Importing.Generators;
using MFractor.Images.Models;
using Xunit;

namespace MFractor.Images.Tests.Importing.Generators
{
    public abstract class BaseAppIconFilenameGeneratorTests
    {
        protected abstract IAppIconFilenameGenerator Generator { get; }

        protected abstract IconSet IconGroup { get; }

        protected abstract IconIdiom Idiom { get; }

        [Fact]
        public void Test_GenerateFilename_HasName()
        {
            var iconImage = new IconImage(Idiom, IconGroup, ImageScale.Points_1x);
            var sut = Generator;

            var result = sut.Generate(iconImage);

            Assert.StartsWith(iconImage.Name, result, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void Test_GenerateFilename_HasExtension()
        {
            const string expectedExtension = ".png";
            var iconImage = new IconImage(Idiom, IconGroup, ImageScale.Points_1x);
            var sut = Generator;

            var result = sut.Generate(iconImage);

            Assert.EndsWith(expectedExtension, result, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
