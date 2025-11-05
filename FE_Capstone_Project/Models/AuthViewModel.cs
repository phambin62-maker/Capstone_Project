namespace FE_Capstone_Project.Models
{
    public class AuthViewModel
    {
        // Dùng cho login
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Message {get; set;}
        // Dùng cho register
        
    }
    public class RegisterViewModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Message {get; set;}
    }
}

