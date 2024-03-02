using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Utils
{
    public class MailUtil
    {
        private readonly IConfiguration _configuration;
        public MailUtil(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string email, string subject, string messageBody)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var username = _configuration["EmailSettings:SmtpUsername"];
            var password = _configuration["EmailSettings:SmtpPassword"];
            var fromAddress = _configuration["EmailSettings:FromAddress"];
            var fromName = _configuration["EmailSettings:FromName"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = messageBody };

            using (var client = new SmtpClient())
            {
                client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                client.Authenticate(username, password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
