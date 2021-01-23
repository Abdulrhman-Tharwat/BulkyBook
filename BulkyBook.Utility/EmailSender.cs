using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public interface IEmailSenderNew
    {
        Task SendEmailAsync(string host, int? port, bool ssl, string emailSender, string password, string email, string subject, string message, string fromName, string fromEmail);
    }

    public class EmailSenderNew : IEmailSenderNew
    {
        private readonly EmailOptions emailOption;
        public EmailSenderNew(IOptions<EmailOptions> options)
        {
            emailOption = options.Value;
        }
        public Task SendEmailAsync(string host, int? port, bool ssl, string emailSender, string password, string email, string subject, string message, string fromName, string fromEmail)
        {
            return Execute(host, port, ssl, emailSender, password, subject, message, email, fromName, fromEmail);
        }
        private Task Execute(string host, int? port, bool ssl, string emailSender, string password, string subject, string message, string emails, string fromName, string fromEmail)
        {
            SmtpClient client = new SmtpClient(host)
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(emailSender, password),
                Port = (int)port,//587,
                EnableSsl = ssl
            };

            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8,

            };
            if (!string.IsNullOrEmpty(emails))
            {
                foreach (var email in emails.Split(","))
                {
                    mailMessage.To.Add(new MailAddress(email));
                }
            }
            return client.SendMailAsync(mailMessage);
        }

    }


        //private Task Execute(string sendGridKey, string subject, string message, string email)
        //{
        //    try
        //    {
        //        var client = new SendGridClient(sendGridKey);
        //        var from = new EmailAddress("faroonyang75@gmail.com", "Bulky Books");
        //        var to = new EmailAddress(email, "End User");
        //        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
        //        return client.SendEmailAsync(msg);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions emailOption;
        public EmailSender(IOptions<EmailOptions> options)
        {
            emailOption = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }
        //private Task Execute(string sendGridKey, string subject, string message, string email)
        //{
        //    try
        //    {
        //        var client = new SendGridClient(sendGridKey);
        //        var from = new EmailAddress("faroonyang75@gmail.com", "Bulky Books");
        //        var to = new EmailAddress(email, "End User");
        //        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
        //        return client.SendEmailAsync(msg);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}

    }

}
