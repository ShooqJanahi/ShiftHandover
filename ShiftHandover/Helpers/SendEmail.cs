using System.Net;
using System.Net.Mail;

namespace ShiftHandover.Helpers
{
    public static class SendEmailHelper
    {
        public static void Send(string recipientEmail, string username, string plainPassword)
        {
            try
            {
                var fromAddress = new MailAddress("yourEmail@example.com", "Shift Handover System");
                var toAddress = new MailAddress(recipientEmail);
                const string fromPassword = "yourEmailPassword"; // Your App Password or real password
                string subject = "Welcome to Shift Handover";
                string body = $"Dear User,\n\n" +
                              $"Your account has been created successfully.\n\n" +
                              $"Username: {username}\n" +
                              $"Password: {plainPassword}\n\n" +
                              $"Please log in and change your password after your first login for security reasons.\n\n" +
                              $"Regards,\nShift Handover Team";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // Use your SMTP server
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Log error, you can later improve this
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }
    }
}
