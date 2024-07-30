using System;
using System.Diagnostics;

namespace MFractor.Licensing.MachineIdentification
{
    [DebuggerDisplay("{Name} - {FingerPrint}")]
    public class MachineId
    {
        public MachineId(string machineFingerPrint, string machineName)
        {
            if (string.IsNullOrEmpty(machineFingerPrint))
            {
                throw new ArgumentException("message", nameof(machineFingerPrint));
            }

            if (string.IsNullOrEmpty(machineName))
            {
                throw new ArgumentException("message", nameof(machineName));
            }

            FingerPrint = machineFingerPrint;
            Name = machineName;
        }

        public string FingerPrint { get; }

        public string Name { get; }
    }
}
