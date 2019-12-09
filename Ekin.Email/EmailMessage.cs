using System;
using System.Collections.Generic;
using System.IO;
using Ekin.Log;
using SendGrid.Helpers.Mail;

namespace Ekin.Email
{
    public class EmailMessage
    {
        public EmailAddress From { get; set; }
        public EmailAddress To { get; set; }
        public List<EmailAddress> ToMultiple { get; set; }
        public EmailAddress CC { get; set; }
        public List<EmailAddress> CCMultiple { get; set; }
        public EmailAddress BCC { get; set; }
        public List<EmailAddress> BCCMultiple { get; set; }
        public EmailAddress ReplyTo { get; set; }
        public string Subject { get; set; }
        public string PlainTextContent { get; set; }
        public string HtmlContent { get; set; }
        public bool ForceHtml { get; set; }
        public List<System.Net.Mail.Attachment> Attachments { get; set; }
        public System.Net.Mail.AlternateView AlternateView { get; set; }

        public LogFactory Logs { get; private set; }

        public EmailMessage()
        {
            Logs = new LogFactory();
            Attachments = new List<System.Net.Mail.Attachment>();
        }

        public EmailMessage(EmailAddress From, EmailAddress To, string Subject, string PlainTextContent, string HtmlContent, bool ForceHtml, EmailAddress CC = null, EmailAddress ReplyTo = null, EmailAddress BCC = null)
        {
            this.From = From;
            this.To = To;
            this.Subject = Subject;
            this.PlainTextContent = PlainTextContent;
            this.HtmlContent = HtmlContent;
            this.ForceHtml = ForceHtml;

            if (CC != null)
                this.CC = CC;

            if (ReplyTo != null)
                this.ReplyTo = ReplyTo;

            if (BCC != null)
                this.BCC = BCC;
        }

        public EmailMessage(EmailAddress From, List<EmailAddress> ToMultiple, string Subject, string PlainTextContent, string HtmlContent, bool ForceHtml, EmailAddress CC = null, EmailAddress ReplyTo = null, EmailAddress BCC = null)
        {
            this.From = From;
            this.ToMultiple = ToMultiple;
            this.Subject = Subject;
            this.PlainTextContent = PlainTextContent;
            this.HtmlContent = HtmlContent;
            this.ForceHtml = ForceHtml;

            if (CC != null)
                this.CC = CC;

            if (ReplyTo != null)
                this.ReplyTo = ReplyTo;

            if (BCC != null)
                this.BCC = BCC;
        }

        #region Private Helpers

        private string GetPlainTextFromHtml()
        {
            return System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(HtmlContent, @"<(.|\n)*?>", ""));
        }

        #endregion

        #region Internal functions

        internal System.Net.Mail.MailMessage GetSmtpMessage()
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage()
            {
                From = From.GetMailAddress(),
                IsBodyHtml = !string.IsNullOrWhiteSpace(HtmlContent),
                Subject = Subject,
                Body = string.IsNullOrWhiteSpace(HtmlContent) ? PlainTextContent : HtmlContent
            };

            if (To != null)
            {
                message.To.Add(To.GetMailAddress());

            }

            if (ToMultiple?.Count > 0)
            {
                foreach (EmailAddress emailAddress in ToMultiple)
                {
                    message.To.Add(emailAddress.GetMailAddress());
                }
            }

            // Add plain text alternative if the body is HTML
            if (message.IsBodyHtml)
            {
                message.AlternateViews.Add(System.Net.Mail.AlternateView.CreateAlternateViewFromString(GetPlainTextFromHtml(), new System.Net.Mime.ContentType("text/plain")));
            }

            if (CC != null && !string.IsNullOrEmpty(CC.Email))
                message.CC.Add(CC.GetMailAddress());

            if (CCMultiple?.Count > 0)
            {
                foreach (EmailAddress emailAddress in CCMultiple)
                {
                    message.CC.Add(emailAddress.GetMailAddress());
                }
            }

            if (BCC != null && !string.IsNullOrEmpty(BCC.Email))
                message.Bcc.Add(BCC.GetMailAddress());

            if (BCCMultiple?.Count > 0)
            {
                foreach (EmailAddress emailAddress in BCCMultiple)
                {
                    message.Bcc.Add(emailAddress.GetMailAddress());
                }
            }

            if (ReplyTo != null && !string.IsNullOrEmpty(ReplyTo.Email))
                message.ReplyToList.Add(ReplyTo.GetMailAddress());

            if (Attachments != null)
            {
                foreach (var attachment in Attachments)
                {
                    message.Attachments.Add(attachment);
                }
            }

            if (AlternateView != null)
            {
                message.AlternateViews.Add(AlternateView);
            }

            return message;
        }

        internal SendGrid.Helpers.Mail.SendGridMessage GetSendgridMessage()
        {
            SendGridMessage msg = new SendGridMessage();
            msg.SetFrom(From.GetSendGridEmailAddress());

            if (To != null)
            {
                msg.AddTo(To.GetSendGridEmailAddress());
            }

            // Add multiple To addresses
            if (ToMultiple?.Count > 0)
            {
                foreach (EmailAddress emailAddress in ToMultiple)
                {
                    msg.AddTo(emailAddress.GetSendGridEmailAddress());
                }
            }

            //CC Email Addresses
            if (CC != null && !string.IsNullOrEmpty(CC.Email))
                msg.AddCc(CC.GetSendGridEmailAddress());

            // Add multiple CC addresses
            if (CCMultiple?.Count > 0)
            {
                foreach (EmailAddress emailAddress in CCMultiple)
                {
                    msg.AddCc(emailAddress.GetSendGridEmailAddress());
                }
            }

            //BCC Email Addresses
            if (BCC != null && !string.IsNullOrEmpty(BCC.Email))
                msg.AddBcc(BCC.GetSendGridEmailAddress());

            // Add multiple BCC addresses
            if (BCCMultiple?.Count > 0)
            {
                foreach (EmailAddress emailAddress in BCCMultiple)
                {
                    msg.AddBcc(emailAddress.GetSendGridEmailAddress());
                }
            }

            //Reply To Email Addresses
            if (ReplyTo != null && !string.IsNullOrEmpty(ReplyTo.Email))
                msg.SetReplyTo(ReplyTo.GetSendGridEmailAddress());

            msg.SetSubject(Subject);

            if (!string.IsNullOrWhiteSpace(HtmlContent))
            {
                msg.AddContent("text/html", HtmlContent);
                msg.AddContent("text/plain", string.IsNullOrWhiteSpace(PlainTextContent) ? GetPlainTextFromHtml() : PlainTextContent);
            }
            else
            {
                msg.PlainTextContent = PlainTextContent;
            }

            if (AlternateView?.ContentType != null)
            {
                string alternateViewContent = string.Empty;
                using (StreamReader reader = new StreamReader(AlternateView.ContentStream))
                {
                    alternateViewContent = reader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(alternateViewContent))
                    {
                        string mediaType = AlternateView.ContentType.MediaType;
                        if (AlternateView.ContentType.Parameters.ContainsKey("method"))
                        {
                            mediaType += "; method=" + AlternateView.ContentType.Parameters["method"];
                        }
                        msg.AddContent(mediaType, alternateViewContent);
                    }
                }
            }

            if (Attachments != null)
            {
                foreach (var attachment in Attachments)
                {
                    if (attachment.ContentStream is FileStream)
                    {
                        var fullPath = (attachment.ContentStream as System.IO.FileStream).Name;
                        var fileInfo = new FileInfo(fullPath);

                        if (fileInfo.Exists)
                        {
                            var contentId = attachment.ContentId;
                            var disposition = "attachment";
                            var filename = attachment.Name;
                            var type = attachment.ContentType.MediaType;
                            var bytes = File.ReadAllBytes(fullPath);
                            var content = Convert.ToBase64String(bytes);

                            msg.AddAttachment(filename, content, type, disposition, contentId);
                        }
                        else
                        {
                            Logs.AddError("EmailMessage", "GetSendgridMessage", $"File not found for attachment {attachment.Name}");
                        }
                    }
                    else
                    {
                        // Filename is a required field in Sendgrid so we have to ensure we have one
                        string fileName = attachment.ContentDisposition?.FileName;
                        if (string.IsNullOrWhiteSpace(fileName))
                            fileName = attachment.Name;
                        if (string.IsNullOrWhiteSpace(fileName))
                            fileName = Guid.NewGuid().ToString("N");

                        msg.AddAttachmentAsync(fileName, attachment.ContentStream, attachment.ContentType?.MediaType, attachment.ContentDisposition?.DispositionType, attachment.ContentId);
                    }
                }
            }

            return msg;
        }

        #endregion
    }
}
