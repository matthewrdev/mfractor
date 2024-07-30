using System;
using System.ComponentModel.Composition;
using MFractor.Licensing.MachineIdentification;
using MFractor;

namespace MFractor.Licensing.Activation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILicenseRequestFactory))]
    class LicenseRequestFactory : ILicenseRequestFactory
    {
        readonly Lazy<IMachineIdentificationService> machineIdentificationService;
        public IMachineIdentificationService MachineIdentificationService => machineIdentificationService.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IPlatformService> platformService;
        public IPlatformService PlatformService => platformService.Value;

        [ImportingConstructor]
        public LicenseRequestFactory(Lazy<IMachineIdentificationService> machineIdentificationService,
                                     Lazy<IProductInformation> productInformation,
                                     Lazy<IPlatformService> platformService)
        {
            this.machineIdentificationService = machineIdentificationService;
            this.productInformation = productInformation;
            this.platformService = platformService;
        }

        public LicenseRequest CreateSerialKeyRequest(string emailAddress, string serialKey)
        {
            var machineId = MachineIdentificationService.GetMachineId();

            var request = new LicenseRequest()
            {
                DeviceIdentifiter = machineId.FingerPrint,
                Platform = PlatformService.IsWindows ? "VSWin" : "VSMac",
                Email = emailAddress,
                SerialKey = serialKey,
                MachineName = machineId.Name,
                IdeVersion = ProductInformation.ExternalProductVersion.ToShortString(),
                PlatformVersion = "10.0",
                ProductVersion = ProductInformation.Version.ToShortString(),
            };

            return request;
        }

        public LicenseRequest CreateTrialLicenseRequest(string emailAddress, string licenseeName)
        {
            var machineId = MachineIdentificationService.GetMachineId();

            var request = new LicenseRequest()
            {
                DeviceIdentifiter = machineId.FingerPrint,
                Platform = PlatformService.IsWindows ? "VSWin" : "VSMac",
                Email = emailAddress,
                Name = licenseeName,
                MachineName = machineId.Name,
                IdeVersion = ProductInformation.ExternalProductVersion.ToShortString(),
                PlatformVersion = "10.0",
                ProductVersion = ProductInformation.Version.ToShortString(),
            };

            return request;
        }
    }
}