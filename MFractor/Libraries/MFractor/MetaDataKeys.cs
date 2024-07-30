using System;

namespace MFractor
{
    /// <summary>
    /// A collection of keys that can be used to retrieve content from an <see cref="IMetaDataObject"/>.
    /// </summary>
    [Obsolete("The concept of meta-data keys will be removed from MFractors APIs shortly.")]
	public static class MetaDataKeys
	{
        /// <summary>
        /// A collection of keys that can be used to retrive content from an <see cref="MFractor.Code.Analysis.ICodeIssue"/>.
        /// </summary>
        public static class Analysis
        {
            /// <summary>
            /// A list of issues attached to a piece of Syntax.
            /// </summary>
            public const string Issues = "com.mfractor.code.analysis.issues";
        }
	}
}
