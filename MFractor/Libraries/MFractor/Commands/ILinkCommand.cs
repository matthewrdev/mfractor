using System;
using MFractor.Commands;

namespace MFractor.Commands
{
    public class LinkCommandClickedEventArgs : EventArgs
    {
        public LinkCommandClickedEventArgs(ILinkCommand linkCommand)
        {
            LinkCommand = linkCommand;
        }

        public ILinkCommand LinkCommand { get; }
    }

    public interface ILinkCommand : ICommand
    {
        /// <summary>
        /// The label to display.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The description of this command.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The url/link that this command should open.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Occurs when the link is clicked.
        /// </summary>
        event EventHandler<LinkCommandClickedEventArgs> LinkClicked;
    }
}