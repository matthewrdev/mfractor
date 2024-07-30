using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MFractor.Utilities;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMailingListService))]
    class MailingListService : IMailingListService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        const string apiKey = "REDACTED";
        const string listId = "REDACTED";

        public void RegisterForMailingList(string userEmail)
        {
            var validator = new EmailValidationHelper();
            if (!validator.IsValidEmail(userEmail))
            {
                log?.Info("Input email '" + userEmail + "' is not valid. Ignoring.");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var endPoint = $"https://us13.api.mailchimp.com/3.0/lists/{listId}/members";

                    var json = "{\"email_address\": \"" + userEmail + "\", \"status\": \"pending\"}";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("apikey", apiKey);

                    var result = await client.PostAsync(endPoint, content);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }).ConfigureAwait(false);
        }
    }
}

