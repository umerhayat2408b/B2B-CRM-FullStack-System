using B2B_PRO;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public EmailService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task SendLeadEmailAsync(string toEmail, string companyName, int companyId)
    {
        var company = await _context.companyName.FindAsync(companyId);

        if (company == null)
        {
            Console.WriteLine("Company not found.");
            return;
        }

        try
        {
            string fromEmail = "umarqureshi8560@gmail.com";
            string appPassword = "ysjg hhuz wyqu xvvx";

            // Note: Capital C for CompanyName and Capital N for Name
            string subject = $"Collaboration with {company.CompanyName}";
            string body = $@"Hi Team {company.CompanyName},

We found your business online and would love to collaborate with you.

Regards
{company.Name}";

            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential(fromEmail, appPassword);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"✅ Email successfully sent to {toEmail}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Email failed to {toEmail}: {ex.Message}");
            throw;
        }
    }
}