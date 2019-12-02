using Ekin.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekin.Email
{
    public interface IEmailClient
    {
        Task<bool> SendAsync(EmailMessage message);

        bool Send(EmailMessage message);

        LogFactory Logs { get; set; }

    }
}
