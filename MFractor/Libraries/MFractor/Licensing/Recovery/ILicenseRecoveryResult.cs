namespace MFractor.Licensing.Recovery
{
    interface ILicenseRecoveryResult
    {
        string Message { get; }

        bool Success { get; }
    }
}
