using System;
using System.Net.Http;

namespace MFractor
{
    /// <summary>
    /// Provides a single instance of the <see cref="HttpClient"/> for use across MFractor.
    /// <para/>
    /// See: https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
    /// </summary>
    public interface ISharedHttpClient
    {
        /// <summary>
        /// The shared <see cref="HttpClient"/> instance.
        /// </summary>
        HttpClient HttpClient { get; }
    }
}