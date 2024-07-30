using System;

namespace MFractor.Commands
{
    /// <summary>
    /// A factory that creates new <see cref="ILinkCommand"/>'s from labels and urls.
    /// </summary>
    public interface ILinkCommandFactory
    {
        ILinkCommand Create(string label, string url);

        ILinkCommand Create(string label, string url, string descripion);
    }
}
