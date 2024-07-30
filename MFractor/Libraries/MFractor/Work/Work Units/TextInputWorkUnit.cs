using System;
using System.Collections.Generic;

namespace MFractor.Work.WorkUnits
{
    public delegate IReadOnlyList<IWorkUnit> TextInputResultDelegate(string input);

    public delegate bool ValidateTextInputDelegate(string input, out string validationMessage);

    /// <summary>
    /// A <see cref="IWorkUnit"/> that launches a text input dialog.
    /// </summary>
    public class TextInputWorkUnit : WorkUnit
    {
        public string Title { get; }

        public string Message { get; }

        public string Value { get; }

        public string Confirm { get; }

        public string Cancel { get; }

        /// <summary>
        /// The function to generate the <see cref="IWorkUnit"/>'s to apply.
        /// </summary>
        public TextInputResultDelegate TextInputResultDelegate { get; set; }

        /// <summary>
        ///  The function to validate the input string.
        /// </summary>
        public ValidateTextInputDelegate ValidateTextInputDelegate { get; set; }

        public TextInputWorkUnit(string title,
                                 string message,
                                 string placeholder,
                                 string confirm,
                                 string cancel,
                                 TextInputResultDelegate textInputResultDelegate,
                                 ValidateTextInputDelegate validateTextInputDelegate = null)
        {
            Title = title;
            Message = message;
            Value = placeholder;
            Confirm = confirm;
            Cancel = cancel;
            TextInputResultDelegate = textInputResultDelegate ?? throw new ArgumentNullException(nameof(textInputResultDelegate));
            ValidateTextInputDelegate = validateTextInputDelegate;
        }
    }
}
