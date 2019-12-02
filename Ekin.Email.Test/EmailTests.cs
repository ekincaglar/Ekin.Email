using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ekin.Email.Test
{
    [TestClass]
    public class EmailTests
    {
        private static string fromAddress = @"admin@bbhpro.com";
        private static string sendGridApiKey = "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";
        private static string smtpHost = "127.0.0.1";
        private static string smtpUsername = "username";
        private static string smtpPassword = "password";

        private static List<string> LoadDefaultEmailAddresses()
        {
            var toList = new List<string>();
            toList.Add("mustafa.kipergil@bbh.co.uk");
            return toList;
        }

        private static IEmailClient LoadEmailClient()
        {
            IEmailClient mailClient = new Sendgrid(sendGridApiKey);
            return mailClient;
        }

        [TestMethod]
        public void can_send()
        {
            List<string> toList = LoadDefaultEmailAddresses();

            var mailClient = LoadEmailClient();

            EmailMessage mailMessage = new EmailMessage
            {
                From = new EmailAddress(fromAddress),
                To = new EmailAddress(toList.FirstOrDefault()),
                PlainTextContent = "",
                HtmlContent = "html content",
                ForceHtml = true,
                Subject = "test subject"
            };

            var attachedFileName = @"C:\bbhpro\Reports\Department Utilisation Report (UK-Account Management [UK Agency]).xlsx";
            mailMessage.Attachments.Add(new Attachment(attachedFileName));

            string body = "file body";
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(body);
            streamWriter.Flush();
            memoryStream.Position = 0;
            System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Text.Plain);
            Attachment streamAttachment = new Attachment(memoryStream, ct);
            streamAttachment.ContentDisposition.FileName = $"somename.txt";
            streamAttachment.Name = $"somename.txt";
            streamAttachment.ContentDisposition.Size = memoryStream.Length;
            mailMessage.Attachments.Add(streamAttachment);


            var result = mailClient.Send(mailMessage);

            Assert.AreEqual(result, true);


        }
    }
}
