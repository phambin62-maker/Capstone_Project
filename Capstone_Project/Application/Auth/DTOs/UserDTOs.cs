namespace BE_Capstone_Project.Application.Auth.DTOs
{
    public class UserDTOs
    {
        public class RegisterDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
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
    }
}
