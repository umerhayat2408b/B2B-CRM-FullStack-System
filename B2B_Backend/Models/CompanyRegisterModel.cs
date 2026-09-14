namespace B2B_PRO.Models
{
    public class CompanyRegisterModel
    {
        // Company
        public string Name { get; set; }

        public string CompanyName { get; set; }

        public string Email { get; set; }

        // Tenant Admin
        public string AdminName { get; set; }

        public string AdminEmail { get; set; }

        public string AdminPassword { get; set; }
    }
}