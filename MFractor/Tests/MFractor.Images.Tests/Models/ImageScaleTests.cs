using System;
using MFractor.Images.Models;
using Xunit;

namespace MFractor.Images.Tests.Models
{
    public class ImageScaleTests
    {
        [Fact]
        public void Test_ImageScale_EqualityWithOperator_Equivalence()
        {
            var firstScale = ImageScale.Points_1x;
            var equivalentScale = ImageScale.Points_1x;
            Assert.True(firstScale == equivalentScale);
        }

        [Fact]
        public void Test_ImageScale_InequalityWithOperator_Equivalence()
        {
            var firstScale = ImageScale.Points_1x;
            var differentScale = ImageScale.Points_2x;
            Assert.True(firstScale != differentScale);
        }

        [Fact]
        public void Test_ImageScale_EqualityWithMethod_Equivalence()
        {
            var firstScale = ImageScale.Points_1x;
            var equivalentScale = ImageScale.Points_1x;
            Assert.True(firstScale.Equals(equivalentScale));
        }
    }
}
