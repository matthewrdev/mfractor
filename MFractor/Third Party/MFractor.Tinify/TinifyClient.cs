using System.Net.Http;
using System.Threading.Tasks;

/* We cannot and should not give a namespace and class the same name:
   https://msdn.microsoft.com/en-us/library/ms229026(v=vs.110).aspx */
namespace MFractor.Tinify
{
    using Method = HttpMethod;

    public class TinifyClient
    {
        static object mutex = new object();
        static Client client;

        static string key;
        static string appIdentifier;
        static string proxy;

        static bool validateServerCertificate = true;

        public static string Key
        {
            get => key;

            set
            {
                key = value;
                ResetClient();
            }
        }

        public static string AppIdentifier
        {
            get => appIdentifier;

            set
            {
                appIdentifier = value;
                ResetClient();
            }
        }

        public static string Proxy
        {
            get => proxy;

            set
            {
                proxy = value;
                ResetClient();
            }
        }

        public static bool ValidateServerCertificate
        {
            get => validateServerCertificate;

            set
            {
                validateServerCertificate = value;
                ResetClient();
            }
        }

        static void ResetClient()
        {
            if (client != null)
            {
                client.Dispose();
            }
            client = null;
        }

        public static uint? CompressionCount { get; set; }

        public static Client Client
        {
            get
            {
                if (key == null)
                {
                    throw new AccountException("Provide an API key with Tinify.Key = ...");
                }

                if (client != null)
                {
                    return client;
                }
                else
                {
                    lock (mutex)
                    {
                        if (client == null)
                        {
                            client = new Client(key, appIdentifier, proxy, validateServerCertificate);
                        }
                    }
                    return client;
                }
            }
        }

        public static Task<Source> FromFile(string path)
        {
            return Source.FromFile(path);
        }

        public static Task<Source> FromBuffer(byte[] buffer)
        {
            return Source.FromBuffer(buffer);
        }

        public static Task<Source> FromUrl(string url)
        {
            return Source.FromUrl(url);
        }

        public static async Task<bool> Validate()
        {
            try
            {
                await Client.Request(Method.Post, "/shrink").ConfigureAwait(false);
            }
            catch (AccountException err)
            {
                if (err.Status == 429)
                {
                    return true;
                }
                throw err;
            }
            catch (ClientException)
            {
                return true;
            }
            return false;
        }
    }
}
