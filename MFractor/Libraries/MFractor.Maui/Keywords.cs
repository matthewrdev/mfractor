using System;
namespace MFractor.Maui
{
    /// <summary>
    /// A collection of keywords that are commonly used within XAML.
    /// </summary>
	public static class Keywords
	{
        /// <summary>
        /// A collection of keywords that are used within XML namespace declarations.
        /// </summary>
		public static class Xmlns
		{
            /// <summary>
            /// Used to specify the .NET namespace to target in an xmlns declaration.
            /// </summary>
			public const string Namespace = "clr-namespace";

            /// <summary>
            /// Used to specify the .NET assembly to target in an xmlns declaration.
            /// </summary>
            public const string Assembly = "assembly";

            /// <summary>
            /// Used to specify a .NET namespace in the current assembly within a xmlns declaration.
            /// </summary>
            public const string Using = "using";

            /// <summary>
            /// Used to specify the target platform (such as iOS, Android) in an xmlns declaration.
            /// <para/>
            /// The <see cref="TargetPlatform"/> keyword is when consuming native views in XAML.
            /// </summary>
			public const string TargetPlatform = "targetPlatform";
		}

        /// <summary>
        /// A collection of keywords related to the Microsoft schema.
        /// <para/>
        /// See <see cref="XamlSchemas.Microsoft"/>.
        /// </summary>
		public static class MicrosoftSchema
		{
            /// <summary>
            /// The local name for "x:Arguments" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// "x:Arguments" is used to define the parameters provided to an elements constructor.
            /// </summary>
            public const string Arguments = "Arguments";

			/// <summary>
			/// The local name for "x:TypeArguments" where 'x' would reference the microsoft xaml schema.
			/// <para/>
			/// Specifies constructor arguments for a non-default constructor.
			/// </summary>
			public const string TypeArguments = "TypeArguments";

			/// <summary>
			/// The local name for "x:Name" where 'x' would reference the microsoft xaml schema.
			/// <para/>
			/// Specifies a runtime object name for the XAML element. Setting x:Name is similar to declaring a variable in code.
			/// </summary>
			public const string CodeBehindName = "Name";

            /// <summary>
            /// The local name for "x:Key" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// Specifies a unique user-defined key for each resource in a ResourceDictionary. 
            /// The key's value is used to retrieve the XAML resource, and is typically used 
            /// as the argument for the StaticResource markup extension.
            /// </summary>
            public const string DictionaryKey = "Key";

            /// <summary>
            /// The local name for "x:Class" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// Specifies the namespace and class name for a class defined in XAML. 
            /// The class name must match the class name of the code-behind file. 
            /// Note that this construct can only appear in the root element of a XAML file.
            /// </summary>
            public const string CodeBehindClass = "Class";

            /// <summary>
            /// The local name for "x:Static" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// Refers to the "x:Static" markup extension that performs an static evaluation on the element that follows the expression.
            /// </summary>
            public const string StaticExpression = "Static";

            /// <summary>
            /// The local name for "x:Type" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// Refers to the "x:Type" markup extension that will perform a typeof() operation on the element that follows.
            /// </summary>
            public const string TypeofExpression = "Type";

            /// <summary>
            /// The local name for "x:Null" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// Refers to the "x:Null" markup extension that will return a null value.
            /// </summary>
            public const string NullExpression = "Null";

            /// <summary>
            /// The local name for "x:Reference" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// Refers to the "x:Reference" markup extension that refers to an named element within the xaml document.
            /// </summary>
            public const string ReferenceExpression = "Reference";

            /// <summary>
            /// The local name for "x:FactoryMethod" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// Specifies constructor arguments for a non-default constructor.
            /// </summary>
            public const string FactoryMethod = "FactoryMethod";

            /// <summary>
            /// The local name for "x:DataType" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// The x:DataType keyword specifies the compile time binding context type.
            /// </summary>
            public const string DataType = "DataType";

            /// <summary>
            /// The local name for "x:FieldModifier" where 'x' would reference the microsoft xaml schema.
            /// <para/>
            /// The x:FieldModifier keyword is used to change the access level for generated code-behind fields.
            /// </summary>
            public const string FieldModifier = "FieldModifier";
		}
	}
}
