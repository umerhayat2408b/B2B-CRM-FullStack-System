namespace B2B_PRO.Models
{
    public class CompanyN
    {
        public int Id { get; set; }

        public string Name { get; set; }        

        public string CompanyName { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<User> Users { get; set; }

    }
}