using System;
using MFractor.Data.Models;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;


namespace MFractor.Maui.Data.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AppThemeColorDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The primary key of the <see cref="StaticResourceDefinition"/> that is this app theme color.
        /// </summary>
        /// <value>The static resource identifier.</value>
        public int StaticResourceKey { get; set; }

        /// <summary>
        /// The name of this app theme color
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The light theme color value.
        /// </summary>
        public string Light { get; set; }

        /// <summary>
        /// The file offset for the start of the value section of the color declaration.
        /// </summary>
        /// <value>The name start.</value>
        public int LightValueStart { get; set; }

        /// <summary>
        /// The file offset for the end of the light value section of the color declaration.
        /// </summary>
        /// <value>The name end.</value>
        public int LightValueEnd { get; set; }

        /// <summary>
        /// The span of the light color value area.
        /// </summary>
        /// <value>The span.</value>
        
        public TextSpan LightValueSpan => TextSpan.FromBounds(LightValueStart, LightValueEnd);

        /// <summary>
        /// Is the light color value an expression?
        /// </summary>
        
        public bool IsLightValueExpression => ExpressionParserHelper.IsExpression(Light);

        /// <summary>
        /// The light theme color value.
        /// </summary>
        public string Dark { get; set; }

        /// <summary>
        /// The file offset for the start of the dark value section of the color declaration.
        /// </summary>
        /// <value>The name start.</value>
        public int DarkValueStart { get; set; }

        /// <summary>
        /// The file offset for the end of the dark value section of the color declaration.
        /// </summary>
        /// <value>The name end.</value>
        public int DarkValueEnd { get; set; }

        /// <summary>
        /// The span of the dark color value area.
        /// </summary>
        /// <value>The span.</value>
        
        public TextSpan DarkValueSpan => TextSpan.FromBounds(DarkValueStart, DarkValueEnd);

        /// <summary>
        /// Is the dark color value an expression?
        /// </summary>
        
        public bool IsDarkValueExpression => ExpressionParserHelper.IsExpression(Dark);
    }
}