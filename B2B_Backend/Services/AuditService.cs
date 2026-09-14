using B2B_PRO.Models;

namespace B2B_PRO.Services
{
    public class AuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveLog(
            string action,
            string user,
            string role,
            int companyId,
            string details)
        {
            AuditLog log = new AuditLog
            {
                Action = action,
                UserName = user,
                UserRole = role,
                CompanyId = companyId,
                Details = details
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }
    }
}