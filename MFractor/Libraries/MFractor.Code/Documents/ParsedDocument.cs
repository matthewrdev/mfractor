using System;
using System.IO;
using MFractor.Workspace;

namespace MFractor.Code.Documents
{
    /// <summary>
    /// A <see cref="IParsedDocument"/> instance where the <see cref="AbstractSyntaxTree"/> is strongly typed to <typeparamref name="TSyntaxTree"/>.
    /// </summary>
    /// <typeparam name="TSyntaxTree"></typeparam>
    public abstract class ParsedDocument<TSyntaxTree> : IParsedDocument where TSyntaxTree : class
    {
        readonly FileInfo fileInfo;

        /// <summary>
        /// The full file path of the parsed document
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; }

        /// <summary>
        /// The extension of this document.
        /// </summary>
        public string Extension => Path.GetExtension(FilePath);

        /// <summary>
        /// The project file that this parsed file is for.
        /// </summary>
        /// <value>The project file.</value>
        public IProjectFile ProjectFile { get; }

        /// <summary>
        /// The name of the parsed document.
        /// <para/>
        /// To get the absolute file path of the document see <see cref="FilePath"/>.
        /// </summary>
        /// <value>The name.</value>
        public string Name => fileInfo.Name;

        /// <summary>
        /// The abstract syntax tree for this parsed document.
        /// </summary>
        public object AbstractSyntaxTree { get; }

        /// <summary>
        /// Get the <see cref="AbstractSyntaxTree"/> of this document cast as <typeparamref name="TSyntaxTree"/>.
        /// </summary>
        /// <returns></returns>
        public TSyntaxTree GetSyntaxTree()
        {
            return AbstractSyntaxTree as TSyntaxTree;
        }

        /// <summary>
        /// The base constructor for a <see cref="ParsedDocument{TSyntaxTree}"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="syntaxTree"></param>
        /// <param name="projectFile"></param>
        protected ParsedDocument(string filePath, 
                                 TSyntaxTree syntaxTree,
                                 IProjectFile projectFile)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            fileInfo = new FileInfo(filePath);
            FilePath = filePath;
            AbstractSyntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
            ProjectFile = projectFile;
        }
    }
}
