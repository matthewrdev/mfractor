using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace MFractor.Rider.Tests
{
    [ZoneDefinition]
    public class MFractorTestEnvironmentZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>, IRequire<IMFractorRiderZone> { }

    [ZoneMarker]
    public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>, IRequire<MFractorTestEnvironmentZone> { }

    [SetUpFixture]
    public class MFractorTestsAssembly : ExtensionTestEnvironmentAssembly<MFractorTestEnvironmentZone> { }
}
