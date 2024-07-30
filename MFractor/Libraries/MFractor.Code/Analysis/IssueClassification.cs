using System;

namespace MFractor.Code.Analysis
{
	/// <summary>
	/// The classification of an issue. 
	/// </summary>
	public enum IssueClassification
	{
		/// <summary>
		/// This issue is an error (runtime or compilation).
		/// </summary>
		Error,

		/// <summary>
		/// The issue is a warning.
		/// </summary>
		Warning,

		/// <summary>
		/// This issue is an improvement.
		/// </summary>
		Improvement,
	}

}
