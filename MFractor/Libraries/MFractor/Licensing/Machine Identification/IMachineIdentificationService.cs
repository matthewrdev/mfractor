using System;

namespace MFractor.Licensing.MachineIdentification
{
    public interface IMachineIdentificationService
    {
        string GetMachineName();

        string GetMachineFingerprint();

        MachineId GetMachineId();
    }
}