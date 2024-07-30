using System;
using System.Collections.Generic;
using MFractor.Images.Models;
using Xunit;

namespace MFractor.Images.Tests.Importing.Generators
{
    public abstract class BaseAppleAppIconFilenameGeneratorTests : BaseAppIconFilenameGeneratorTests
    {
        [Theory]
        [MemberData(nameof(SuffixData))]
        public void Test_GenerateFilename_HasCorrectSuffix(ImageScale scale, string expectedSuffix)
        {
            var iconImage = new IconImage(Idiom, IconGroup, scale);
            var sut = Generator;

            var result = sut.Generate(iconImage);

            Assert.EndsWith(expectedSuffix, result);
        }

        [Fact]
        public void Test_GenerateFilename_ForBaseScale_HasNoDensitySuffix()
        {
            var iconImage = new IconImage(Idiom, IconGroup, ImageScale.Points_1x);
            var sut = Generator;

            var result = sut.Generate(iconImage);

            Assert.DoesNotMatch("@.x.\\w{3,}$", result);
        }

        public static IEnumerable<object[]> SuffixData => new[]
        {
            new object[] { ImageScale.Points_2x, "@2x.png" },
            new object[] { ImageScale.Points_3x, "@3x.png" },
        };

    }
}
