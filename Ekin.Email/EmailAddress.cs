using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekin.Email
{
    public class EmailAddress
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        public EmailAddress()
        {

        }
        
        public EmailAddress(string email, string name = null)
        {
            this.Email = email;
            this.Name = name;
        }

        internal SendGrid.Helpers.Mail.EmailAddress GetSendGridEmailAddress()
        {
            return new SendGrid.Helpers.Mail.EmailAddress(Email, Name);
        }

        internal System.Net.Mail.MailAddress GetMailAddress()
        {
            return new System.Net.Mail.MailAddress(Email, Name);
        }

    }
}
