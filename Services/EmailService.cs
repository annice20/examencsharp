using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace examencsharp.Services
{
    public class EmailService
    {
        private readonly string _fromEmail;
        private readonly string _appPassword;

        public EmailService(IConfiguration config)
        {
            _fromEmail   = config["EmailSettings:SenderEmail"]!;
            _appPassword = config["EmailSettings:AppPassword"]!;
        }

        public void EnvoyerCode(string toEmail, string code)
        {
            var fromAddress = new MailAddress(_fromEmail, "Authentification 2FA");
            var toAddress   = new MailAddress(toEmail);

            string body = $@"
                <h2>Authentification à deux facteurs</h2>
                <p>Votre code de vérification est :</p>
                <h1 style='color:blue'>{code}</h1>
                <p>Ce code expire dans 5 minutes.</p>
            ";

            var smtp = new SmtpClient
            {
                Host                  = "smtp.gmail.com",
                Port                  = 587,
                EnableSsl             = true,
                DeliveryMethod        = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials           = new NetworkCredential(_fromEmail, _appPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject    = "Votre code de vérification 2FA",
                Body       = body,
                IsBodyHtml = true
            };

            smtp.Send(message);
        }
    }
}