namespace LibraryManagement.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class MockEmailService : IEmailService
    {
        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // In a production environment, this would use SmtpClient or SendGrid
            Console.WriteLine("=================================");
            Console.WriteLine($"MOCK EMAIL SENT TO: {toEmail}");
            Console.WriteLine($"SUBJECT: {subject}");
            Console.WriteLine($"BODY: {body}");
            Console.WriteLine("=================================");
            
            return Task.CompletedTask;
        }
    }
}
