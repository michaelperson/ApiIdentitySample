using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.VisualBasic;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ApiIdentitySample.Tools
{
    public class EmailSender : IEmailSender
    {
        private readonly string sendGridKey;

        public EmailSender(string SendGridKey)
        {
            sendGridKey = SendGridKey;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(htmlMessage) || string.IsNullOrEmpty(sendGridKey)) 
            {
                throw new InvalidProgramException("Paramètres manquants pour l'envoie de mail");
            }
            await SendWithSendGrid(sendGridKey, email, subject, htmlMessage);
        }

        private async Task SendWithSendGrid(string apikey, string email, string subject, string htmlMessage)
        {
            ISendGridClient client = new SendGridClient(apikey);
            SendGridMessage message = new SendGridMessage()
            {
                From = new EmailAddress("michaelperson@outlook.be", "EmailFromIdentity"),
                Subject = subject,
                PlainTextContent = htmlMessage,
                HtmlContent = htmlMessage
            };
            message.AddTo(email); ;
            message.SetClickTracking(false, false);
           Response reponse = await client.SendEmailAsync(message);
        }
    }
}
