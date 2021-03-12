using System;
using System.Collections.Generic;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AzureDurableFunctions.Mail
{
    public static class MailSender
    {
        public static void SendEmail(string emailAddress, string sendersName, List<string> message)
        {
            var msg = new SendGridMessage();          
            msg.SetFrom(new EmailAddress("jonah@jonahandersson.tech", "SendGrid Test From APp"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress("cjonah@hotmail.se", "Jonah Andersson"),
                new EmailAddress("anna@example.com", "Anna Lidman"),
                new EmailAddress("peter@example.com", "Peter Saddow")
            };
                        msg.AddTos(recipients);

                        msg.SetSubject("Testing the SendGrid C# Library");

                        msg.AddContent(MimeType.Text, "Hello World plain text!");
                        msg.AddContent(MimeType.Html, "<p>Hello World!</p>");

        }
    }
}
