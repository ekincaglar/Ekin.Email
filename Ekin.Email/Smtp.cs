using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Ekin.Email
{
    public class Smtp : IEmailClient
    {
        public SmtpClient Client { get; set; } = null;
        public Log.LogFactory Logs { get; set; }

        public Smtp(string Host, int Port, bool SSLEnabled, string Username, string Password)
        {
            Logs = new Log.LogFactory();
            Client = new SmtpClient(Host);
            Client.Port = Port;
            Client.EnableSsl = SSLEnabled;
            Client.DeliveryMethod = SmtpDeliveryMethod.Network;
            Client.UseDefaultCredentials = false;
            Client.Credentials = new NetworkCredential(Username, Password);
        }

        public async Task<bool> SendAsync(EmailMessage message)
        {
            if (Client == null)
            {
                return false;
            }

            if (message == null)
            {
                return false;
            }

            try
            {
                await Client.SendMailAsync(message.GetSmtpMessage());
                return true;
            }
            catch (Exception ex)
            {
                Logs.AddError("Smtp", "SendAsync", ex.Message);
                return false;
            }
        }

        public bool Send(EmailMessage message)
        {
            if (Client == null)
            {
                return false;
            }

            if (message == null)
            {
                return false;
            }

            try
            {
                Client.Send(message.GetSmtpMessage());
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
