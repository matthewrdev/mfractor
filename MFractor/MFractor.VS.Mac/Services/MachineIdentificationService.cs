using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using MFractor.Licensing.MachineIdentification;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMachineIdentificationService))]
    class MachineIdentificationService : IMachineIdentificationService
    {
#if TRIAL
        readonly string sessionTrialGuid = Guid.NewGuid().ToString().Replace("-", "");
#endif

        public string GetMachineFingerprint()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "sh",
                Arguments = "-c \"ioreg -rd1 -c IOPlatformExpertDevice | awk '/IOPlatformUUID/'\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UserName = Environment.UserName
            };
            var builder = new StringBuilder();
            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
                builder.Append(process.StandardOutput.ReadToEnd());
            }

            var ioPlatformUUID = builder.ToString().Split('=')[1].Trim().Replace("\"", "");

#if TRIAL
            ioPlatformUUID = "testing-" + ioPlatformUUID + sessionTrialGuid;
#endif

            return ioPlatformUUID;
        }

        public string GetMachineName()
        {
            return Environment.MachineName;
        }

        public MachineId GetMachineId()
        {
            var fingerPrint = GetMachineFingerprint();
            var machineName = GetMachineName();

            return new MachineId(fingerPrint, machineName);
        }
    }
}