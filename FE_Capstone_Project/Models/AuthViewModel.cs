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
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string? SecurityAnswer { get; set; }
        public string? SecurityQuestion { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
    public class RegisterViewModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }
        public string? Message {get; set;}
    }
}
