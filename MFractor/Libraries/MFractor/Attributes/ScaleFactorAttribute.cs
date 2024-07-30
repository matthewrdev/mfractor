using System;

namespace MFractor.Attributes
{
    /// <summary>
    /// Specifies the scale factor on an enum field.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ScaleFactorAttribute : System.Attribute
    {
        /// <summary>
        /// The scale.
        /// </summary>
        /// <value>The scale.</value>
        public double Scale { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Images.ScaleFactorAttribute"/> class.
        /// </summary>
        /// <param name="scale">Scale.</param>
        public ScaleFactorAttribute(double scale)
        {
            Scale = scale;
        }
    }
}
