using System.ComponentModel.DataAnnotations;

namespace RaceDayApplication.Web.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "First Name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public int? Age { get; set; }

        // Optional password update fields
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string? NewPassword { get; set; }

        // Profile Picture preview/upload URL or path
        public string? ProfilePictureUrl { get; set; }



    }
}
