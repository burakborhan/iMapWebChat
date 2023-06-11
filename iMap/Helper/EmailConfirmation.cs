using iMap.ViewModels;
using System.Net.Mail;

namespace iMap.Helper
{
    public static class EmailConfirmation
        {
        public static void EmailConfirmationHelper(string link, string email)
    {

        MailMessage mailMessage = new MailMessage();

        SmtpClient smtpClient = new SmtpClient("smtp.office365.com");

        mailMessage.From = new MailAddress("iMapCommunication@outlook.com");
        mailMessage.To.Add(email);

        mailMessage.Subject = $"www.iMapMapCommunication.com :: Email confirmation";
        mailMessage.Body = "<h2> Click the link below to confirm your e-mail.</h2></hr>";
        mailMessage.Body += $"<a href = '{link}'>E-mail confirm link</a>";
        mailMessage.IsBodyHtml = true;
        smtpClient.Port = 587;
        smtpClient.Credentials = new System.Net.NetworkCredential("iMapCommunication@outlook.com", "iMapMapCommunication");
        smtpClient.EnableSsl = true;

        smtpClient.Send(mailMessage);
    }
}
}
