namespace Tourist.API.Models.Dto
{
    public class UsersInfoDto : RegisterRequestDto
    {
        public string Id { get; set; }
        public string Role { get; set; } // Changed to single role
    }

    public class UserUpdateDto
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Role { get; set; }
    }
}
