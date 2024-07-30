using System;
using MFractor.Images.Models;
using Xunit;

namespace MFractor.Images.Tests.Models
{
    public class IconIdiomTests
    {
        [Theory]
        [InlineData(IconIdiom.AndroidPhone, "Android Phone")]
        [InlineData(IconIdiom.IOS, "iOS")]
        [InlineData(IconIdiom.IPhone, "iPhone")]
        [InlineData(IconIdiom.IPad, "iPad")]
        [InlineData(IconIdiom.CarPlay, "CarPlay")]
        public void Test_GetDescription(IconIdiom idiom, string expectedDescription)
        {
            Assert.Equal(expectedDescription, idiom.GetDescription());
        }

        [Theory]
        [InlineData(IconIdiom.AndroidPhone, "android")]
        [InlineData(IconIdiom.IOS, "ios-marketing")]
        [InlineData(IconIdiom.IPhone, "iphone")]
        [InlineData(IconIdiom.IPad, "ipad")]
        [InlineData(IconIdiom.CarPlay, "carplay")]
        public void Test_GetExportName(IconIdiom idiom, string expectedName)
        {
            Assert.Equal(expectedName, idiom.GetImportName());
        }
    }
}
