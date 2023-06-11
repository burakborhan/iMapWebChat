
using iMap.ViewModels;
using System.Net.Mail;

namespace iMap.Helper
{
    public static class PasswordReset
    {
        public static void PasswordResetEmail(string link , ResetPasswordViewModel resetPasswordView)
        {
            MailMessage mailMessage = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("smtp.office365.com"); 

            mailMessage.From = new MailAddress("iMapCommunication@outlook.com");
            mailMessage.To.Add(resetPasswordView.Email);

            mailMessage.Subject = $"www.iMapMapCommunication.com :: Reset Password";
            mailMessage.Body = "<h2> Click the link below to reset your password.</h2></hr>";
            mailMessage.Body += $"<a href = '{link}'>Password reset link</a>";
            mailMessage.IsBodyHtml = true;
            smtpClient.Port = 587;
            smtpClient.Credentials = new System.Net.NetworkCredential("iMapCommunication@outlook.com", "iMapMapCommunication");
            smtpClient.EnableSsl = true;

            smtpClient.Send(mailMessage);  
            
        }
    }
}
