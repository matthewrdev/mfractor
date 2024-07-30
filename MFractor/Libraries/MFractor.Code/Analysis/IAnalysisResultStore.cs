using System.Collections.Generic;
using MFractor.Code.Documents;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// Stores the results of an analysis pass.
    /// </summary>
    public interface IAnalysisResultStore
	{
		/// <summary>
		/// Gets the analysis result for the specified <paramref name="document"/>.
		/// </summary>
		/// <returns>The last analysis result for <param name="document"/> or null if no analysis results are stored.</returns>
		/// <param name="document">The document to retrieve.</param>
		IReadOnlyList<ICodeIssue> Retrieve (IParsedXmlDocument document);

		/// <summary>
		/// Gets the analysis result for the specified <paramref name="filePath"/>.
		/// </summary>
		/// <returns>The last analysis result for <param name="filePath"/> or null if no analysis results are stored.</returns>
		/// <param name="filePath">The file path of the document to retrieve.</param>
		IReadOnlyList<ICodeIssue> Retrieve (string filePath);

		/// <summary>
		/// Stores the documents analysis results.
		/// </summary>
		/// <param name="document">The document to store.</param>
		/// <param name="results">Results.</param>
		void Store (IParsedXmlDocument document, IReadOnlyList<ICodeIssue> results);

		/// <summary>
		/// Stores the documents analysis results.
		/// </summary>
		/// <param name="filePath">The filePath of the document to store.</param>
		/// <param name="results">Results.</param>
		void Store (string filePath, IReadOnlyList<ICodeIssue> results);

		/// <summary>
		/// Clears all stored results for the provided <paramref name="document"/>.
		/// </summary>
		/// <param name="document">The document to clear.</param>
		void Clear (IParsedXmlDocument document);

		/// <summary>
		/// Clears all stored results for the provided <paramref name="filePath"/>.
		/// </summary>
		/// <param name="filePath">The file path for the document to clear.</param>
		void Clear (string filePath);

		/// <summary>
		/// Clears all stored analysis results for all documents.
		/// </summary>
		void ClearAll ();
	}
}

