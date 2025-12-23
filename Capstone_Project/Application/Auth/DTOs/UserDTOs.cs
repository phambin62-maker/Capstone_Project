namespace BE_Capstone_Project.Application.Auth.DTOs
{
    public class UserDTOs
    {
        public class RegisterDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public string SecurityQuestion { get; set; }
            public string SecurityAnswer { get; set; }
        }

        public class ForgotPasswordDto
        {
            public string Email { get; set; }
        }

        public class ResetPasswordDto
        {
            public string Email { get; set; }
            public string SecurityAnswer { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class UpdateUserDto
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public string? NewPassword { get; set; }


        }
        public class GoogleUserDto
        {
            public string Email { get; set; } = "";
            public string FullName { get; set; } = "";
            public string Provider { get; set; } = "Google";

            
        }
        public class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
        }
    }
}
