namespace RaceDayApplication.DTOs
{
    public class UpdateProfileDto
    {
        // Properties for updating user profile information, all optional to allow partial updates
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
