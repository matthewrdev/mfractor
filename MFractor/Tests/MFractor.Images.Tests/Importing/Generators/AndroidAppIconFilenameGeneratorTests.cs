using System;
using MFractor.Images.Importing.Generators;
using MFractor.Images.Models;

namespace MFractor.Images.Tests.Importing.Generators
{
    public class AndroidAppIconFilenameGeneratorTests : BaseAppIconFilenameGeneratorTests
    {
        protected override IAppIconFilenameGenerator Generator => new AndroidAppIconFilenameGenerator();

        protected override IconSet IconGroup => IconSet.AndroidLauncher;

        protected override IconIdiom Idiom => IconIdiom.AndroidPhone;
    }
}
