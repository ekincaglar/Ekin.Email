using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekin.Log;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Ekin.Email
{
    public class Sendgrid : IEmailClient
    {
        public SendGridClient Client { get; set; } = null;
        public LogFactory Logs { get; set; }

        public Sendgrid(string APIKey)
        {
            Logs = new LogFactory();
            Client = new SendGridClient(APIKey);
        }

        public async Task<bool> SendAsync(EmailMessage message)
        {
            if (Client == null)
            {
                Logs.AddError("SendGrid", "SendAsync", "SendGrid Client has not been initialised.");
                return false;
            }

            if (message == null)
            {
                Logs.AddError("SendGrid", "SendAsync", "Email Message cannot be null.");
                return false;
            }

            try
            {
                Response response = await Client.SendEmailAsync(message.GetSendgridMessage()).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    return true;
                }
                else
                {
                    SetLogsFromResponse(response);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.AddError("SendGrid", "SendAsync", ex);
                return false;
            }
        }

        public bool Send(EmailMessage message)
        {
            return SendAsync(message).Result;
        }

        private void SetLogsFromResponse(Response response)
        {
            if (Logs == null)
            {
                Logs = new LogFactory();
            }
            if (response == null)
            {
                Logs.AddError("SendGrid", "SendAsync", $"SendGrid response was null or it could not be parsed");
            }
            else if (response.Body == null)
            {
                Logs.AddError("SendGrid", "SendAsync", $"SendGrid returned {response.StatusCode.ToString()} and sent no body with the response");
            }
            else
            {
                var bodyResponse = response.DeserializeResponseBodyAsync(response.Body);
                if (bodyResponse != null)
                {
                    var errors = bodyResponse.Result["errors"];
                    if (errors != null && errors.Count > 0)
                    {
                        foreach (var error in errors)
                        {
                            Logs.AddError("SendGrid", "SendAsync", error.message?.ToString());
                        }
                    }
                }
            }
        }

    }
}
