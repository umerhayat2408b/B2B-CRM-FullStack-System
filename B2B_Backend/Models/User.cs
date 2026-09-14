namespace B2B_PRO.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; } = "Employee";

        public int? CompanyId { get; set; }

        public CompanyN Company { get; set; }
    }

}