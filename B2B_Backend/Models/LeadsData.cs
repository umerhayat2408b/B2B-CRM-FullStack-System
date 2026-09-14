namespace B2B_PRO.Models
{
    public class LeadsData
    {
        public int Id { get; set; }
        public string Name { get; set;  }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public int CompanyId { get; set; }

        public CompanyN Company { get; set; }
        public string Status { get; set; } = "New";
    }
}
