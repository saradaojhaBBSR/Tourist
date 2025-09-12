using System.ComponentModel.DataAnnotations;

namespace Tourist.API.Models.Dto
{
    public class RegisterRequestDto
    {
        [Required] public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string ConfirmPassword { get; set; }
        [Required] public string Country { get; set; }
        [Required] public string State { get; set; }
        [Required] public string City { get; set; }
    }
}
