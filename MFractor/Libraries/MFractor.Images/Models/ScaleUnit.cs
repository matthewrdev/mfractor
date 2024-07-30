using System;
using System.Linq;

namespace MFractor.Images.Models
{
    /// <summary>
    /// Describe the available Scale Units.
    /// </summary>
    public enum ScaleUnit
    {
        /// <summary>
        /// Density Independent Pixels.
        /// </summary>
        [DisplayName("dp")]
        DP,

        /// <summary>
        /// Points.
        /// </summary>
        [DisplayName("pt")]
        Points,
    }

    public static class ScaleUnitExtensions
    {
        /// <summary>
        /// Get the name of this enum element.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>The presentation name of the unit.</returns>
        public static string GetName(this ScaleUnit unit)
        {
            var type = typeof(ScaleUnit);
            var fieldInfo = type.GetField(unit.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false) as DisplayNameAttribute[];
            return attributes?.FirstOrDefault()?.Name;
        }
    }

    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; }

        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }

}
