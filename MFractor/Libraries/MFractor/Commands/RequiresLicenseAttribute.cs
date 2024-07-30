using System;
namespace MFractor.Commands.Attributes
{
    /// <summary>
    /// Indicates that the command requires a valid license to execute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequiresLicenseAttribute : Attribute
    {
    }
}
