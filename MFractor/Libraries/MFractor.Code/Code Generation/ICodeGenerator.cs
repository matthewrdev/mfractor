using System;
using System.Collections.Generic;
using MFractor.Configuration;
using MFractor.CodeSnippets;
using MFractor.Work;

namespace MFractor.Code.CodeGeneration
{
    /// <summary>
    /// A code generator is an <see cref="IConfigurable"/> that creates code as syntax nodes, <see cref="IWorkUnit"/>'s or string formatted code.
    /// <para/>
    /// Code generators should always be functional, that is, they should always return a result rather than write the code to disk or into the text editor. This is very important as it allows consumers to mutate the result of code generator, preview it or cancel it.
    /// <para/>
    /// If possible, rather than constructing code through syntax builders, please opt to use <see cref="ICodeSnippet"/>'s. This has two benefits:
    /// <para/>
    /// <b>1.</b> From a maintenance perspective, changing the code generator's ouput is very easy as we can simply edit the snippet template.
    /// <para/>
    /// <b>2.</b> Code generators are <see cref="IConfigurable"/>'s therefore they automatically inherit the users configuration based on the current context. This means that users can overload the snippet and change it to suit their needs.
    /// <para/>
    /// Lastly, when creating a new code generator, please follow these conventions for method naming:
    /// <para/>
    /// <b>Generate</b>: Create an <see cref="IEnumerable{T}"/> of <see cref="IWorkUnit"/>'s.
    /// <para/>
    /// <b>GenerateSyntax</b>: Creates the syntax element.
    /// <para/>
    /// <b>GenerateCode</b>: Creates the raw code.
    /// </summary>
	public interface ICodeGenerator : IConfigurable
	{
        /// <summary>
        /// The languages that the code generator supports.
        /// </summary>
        /// <value>The languages.</value>
		string[] Languages { get; }
	}
}
