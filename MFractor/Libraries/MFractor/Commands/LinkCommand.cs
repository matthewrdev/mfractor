using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace MFractor.Commands
{
    /// <summary>
    /// A <see cref="ICommand"/> implementation that opens the <see cref="Url"/>.
    /// <para/>
    /// For automatic UTM source tracking and analytics, use <see cref="ILinkCommandFactory"/> to create link commands.
    /// </summary>
    [PartNotDiscoverable]
    public class LinkCommand : ICommand, ILinkCommand
    {
        readonly IUrlLauncher urlLauncher;

        public LinkCommand(string label,
                           string url,
                           IUrlLauncher urlLauncher)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("message", nameof(url));
            }

            Label = label;
            Url = url;
            this.urlLauncher = urlLauncher ?? throw new ArgumentNullException(nameof(urlLauncher));
        }

        public LinkCommand(string label,
                           string description,
                           string url,
                           IUrlLauncher urlLauncher)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("message", nameof(url));
            }

            if (urlLauncher is null)
            {
                throw new ArgumentNullException(nameof(urlLauncher));
            }

            Label = label;
            Description = description;
            Url = url;
        }

        /// <summary>
        /// The label to display.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// The description of this command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The url/link that this command should open.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Occurs when the link is clicked.
        /// </summary>
        public event EventHandler<LinkCommandClickedEventArgs> LinkClicked;

        /// <summary>
        /// Execute the command given the provided <paramref name="commandContext"/>.
        /// <para/>
        /// For link commands this will open the <see cref="Url"/> in the users preferred browser.
        /// </summary>
        /// <param name="commandContext"></param>
        public virtual void Execute(ICommandContext commandContext)
        {
            urlLauncher.OpenUrl(Url);

            LinkClicked?.Invoke(this, new LinkCommandClickedEventArgs(this));
        }

        /// <summary>
        /// The execution state of this command, that is, can this command execute, what is it's label/description and any child commands it should show.
        /// <para/>
        /// It is safe to return null from this method.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public virtual ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState(true, true, Label, Description);
        }
    }
}
