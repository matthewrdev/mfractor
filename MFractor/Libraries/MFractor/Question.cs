using System;

namespace MFractor
{
    public class Question
    {
        public string Message { get; }

        public string Title { get; }

        public Question(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}