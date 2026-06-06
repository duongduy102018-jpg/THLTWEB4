using Microsoft.AspNetCore.Identity.UI.Services;

namespace Webbanhang.Helpers
{
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine("========== DEMO EMAIL ==========");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine(htmlMessage);
            Console.WriteLine("================================");
            return Task.CompletedTask;
        }
    }
}
