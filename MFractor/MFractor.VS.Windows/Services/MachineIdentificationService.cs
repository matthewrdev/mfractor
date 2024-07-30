using System;
using System.ComponentModel.Composition;
using MFractor.Licensing.MachineIdentification;
using MFractor.Utilities;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMachineIdentificationService))]
    class MachineIdentificationService : IMachineIdentificationService
    {
        // Credit: https://www.codeproject.com/Articles/28678/Generating-Unique-Key-Finger-Print-for-a-Computer
        class WindowsFingerprint
        {
            public string Create()
            {
                var fingerPrint = CreateCpuId() + "|" + CreateBiosId() + "|" + CreateMotherboardId();

                var hashedFingerprint = SHA1Helper.FromString(fingerPrint).ToLower();

                return hashedFingerprint;
            }

            string GetValue(string wmiClass, string wmiProperty)
            {
                var result = "";
                var mc = new System.Management.ManagementClass(wmiClass);
                var moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            var item = mo[wmiProperty];
                            result = item?.ToString() ?? string.Empty;
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
                return result;
            }
            string CreateCpuId()
            {
                //Uses first CPU identifier available in order of preference
                //Don't get all identifiers, as it is very time consuming
                var id = GetValue("Win32_Processor", "UniqueId");
                if (string.IsNullOrEmpty(id)) //If no UniqueID, use ProcessorID
                {
                    id = GetValue("Win32_Processor", "ProcessorId");
                    if (string.IsNullOrEmpty(id)) //If no ProcessorId, use Name
                    {
                        id = GetValue("Win32_Processor", "Name");
                        if (string.IsNullOrEmpty(id)) //If no Name, use Manufacturer
                        {
                            id = GetValue("Win32_Processor", "Manufacturer");
                        }
                    }
                }
                return id;
            }
            string CreateBiosId()
            {
                return GetValue("Win32_BIOS", "Manufacturer")
                       + GetValue("Win32_BIOS", "SMBIOSBIOSVersion")
                       + GetValue("Win32_BIOS", "IdentificationCode")
                       + GetValue("Win32_BIOS", "SerialNumber")
                       + GetValue("Win32_BIOS", "ReleaseDate")
                       + GetValue("Win32_BIOS", "Version");
            }

            string CreateMotherboardId()
            {
                return GetValue("Win32_BaseBoard", "Model")
                       + GetValue("Win32_BaseBoard", "Manufacturer")
                       + GetValue("Win32_BaseBoard", "Name")
                       + GetValue("Win32_BaseBoard", "SerialNumber");
            }
        }

        readonly Lazy<string> fingerprint = new Lazy<string>(CreateFingerprint);

        static string CreateFingerprint()
        {
                var fingerprint = new WindowsFingerprint();

                return fingerprint.Create();
        }

        public string GetMachineFingerprint()
        {
            return fingerprint.Value;
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