using System;
using MFractor.Attributes;

namespace MFractor.CodeSnippets
{
    /// <summary>
    /// A collection of predefined code snippet argument names, strongly tied to enum values for ease of use.
    /// <para/>
    /// To get the string version of the argument name, use <see cref="CodeSnippetsExtensions.ToArgumentName(ReservedCodeSnippetArgumentName)"/>.
    /// </summary>
    public enum ReservedCodeSnippetArgumentName
    {
        /// <summary>
        /// The name to insert into the snippet.
        /// <para/>
        /// This is typically a class, member, variable name etc.
        /// </summary>
        [Description("name")]
        Name,

        /// <summary>
        /// The type to insert into the snippet.
        /// </summary>
        [Description("type")]
        Type,

        /// <summary>
        /// The namespace.
        /// </summary>
        [Description("namespace")]
        Namespace,

        /// <summary>
        /// The platform for the code snippet.
        /// </summary>
        [Description("platform")]
        Platform,

        /// <summary>
        /// The value of the code snippet.
        /// </summary>
        [Description("value")]
        Value,

        /// <summary>
        /// The base type for the code snippet.
        /// </summary>
        [Description("base_type")]
        BaseType,

        /// <summary>
        /// The property name for the code snippet.
        /// </summary>
        [Description("property")]
        Property,

        /// <summary>
        /// The field name for the code snippet.
        /// </summary>
        [Description("field")]
        Field,

        /// <summary>
        /// The exception handler for the code snippet.
        /// </summary>
        [Description("exception_handler")]
        ExceptionHandler,

        /// <summary>
        /// A group of members for a type definition such as a class, interface or struct.
        /// </summary>
        [Description("members")]
        Members,

        /// <summary>
        /// A single member of a type definition such as a class, interface or struct.
        /// </summary>
        [Description("member")]
        Member,
    }
}
