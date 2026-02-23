using System.Net;
using System.Net.Mail;

namespace examencsharp.Services
{
    public class EmailService
    {
        public void EnvoyerCode(string toEmail, string code)
        {
            var fromAddress = new MailAddress("anniceflorencia@gmail.com", "Authentification 2FA");
            var toAddress = new MailAddress(toEmail);

            const string fromPassword = "dzcjlmjkydrpnxay";
            const string subject = "Votre code de vérification 2FA";
            string body = $@"
                <h2>Authentification à deux facteurs</h2>
                <p>Votre code de vérification est :</p>
                <h1 style='color:blue'>{code}</h1>
                <p>Ce code expire dans 5 minutes.</p>
            ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            smtp.Send(message);
        }
    }
}