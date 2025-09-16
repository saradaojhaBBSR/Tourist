namespace Tourist.API.Models.Dto
{
    public class LoginResponseDto
    {
        public string Email { get; set; }
        public string Role { get; set; } // Changed to single role
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
    public class RefreshTokenRequestDto
    {
        public string Email { get; set; }
        public string RefreshToken { get; set; }
    }
}
