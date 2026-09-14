namespace B2B_PRO.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string Action { get; set; }

        public string UserName { get; set; }

        public string UserRole { get; set; }

        public int CompanyId { get; set; }

        public string Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}