using System;
namespace MFractor.IOC
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ExportAssemblyAttribute : Attribute
    {
        public ExportAssemblyAttribute()
        {
        }
    }
}
