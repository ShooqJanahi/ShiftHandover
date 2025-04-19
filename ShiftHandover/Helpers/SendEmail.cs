using System.Net;
using System.Net.Mail;

namespace ShiftHandover.Helpers
{
    // A static helper class responsible for sending emails (new user account credentials)
    public static class SendEmailHelper
    {
        // Sends a welcome email with username and password to the new user
        public static void Send(string recipientEmail, string username, string plainPassword)
        {
            try
            {
                // Define the sender's email address and display name
                var fromAddress = new MailAddress("ShiftHandover@BAS.com", "Shift Handover System");
                var toAddress = new MailAddress(recipientEmail); // Define the recipient's email address
                const string fromPassword = "senderEmailPassword"; // Sender's email password ( Use an App Password better security)
               
                // Email subject and body content
                string subject = "Welcome to Shift Handover";
                string body = $"Dear User,\n\n" +
                              $"Your account has been created successfully.\n\n" +
                              $"Username: {username}\n" +
                              $"Password: {plainPassword}\n\n" +
                              $"Please log in and change your password after your first login for security reasons.\n\n" +
                              $"Regards,\nShift Handover Team";

                // Configure the SMTP client for sending the email
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // SMTP server address (for Gmail)
                    Port = 587,  // SMTP port (587 for TLS)
                    EnableSsl = true,  // Enable SSL/TLS encryption
                    DeliveryMethod = SmtpDeliveryMethod.Network, // Send through network
                    UseDefaultCredentials = false, // Use custom credentials, not system credentials
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword) // Login credentials
                };

                // Create the email message with sender, recipient, subject, and body
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    // Send the email
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // If sending fails, log the error 
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }
    }
}
