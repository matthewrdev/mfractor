using System;

namespace MFractor
{
    public class ToolBarAction
    {
        public ToolBarAction(string message, Action action)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("message", nameof(message));
            }

            Message = message;
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public string Message { get; }

        public Action Action { get; }
    }
}