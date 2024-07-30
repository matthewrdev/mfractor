namespace MFractor.Licensing.Activation
{
    interface ILicenseRequestResult
    {
        bool Success { get; }

        string LicenseContent { get; }

        string StatusMessage { get; }

        string StatusDetail { get; }
    }
}
