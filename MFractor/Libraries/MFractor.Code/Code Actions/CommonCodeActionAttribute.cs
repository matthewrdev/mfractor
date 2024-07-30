using System;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// When an <see cref="ICodeAction"/> is marked with the <see cref="CommonCodeActionAttribute"/>, this informs the code action engine the code action is frequently used or particularly useful.
    /// <para/>
    /// Common code actions are sufaced as additional link-actions in tooltips.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false)]
    public class CommonCodeActionAttribute : Attribute
    {
    }
}
