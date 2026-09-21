using Microsoft.AspNetCore.Identity;
namespace RaceDayApplication.Models
{
    public class ApplicationUser : IdentityUser// This class represents an application user and inherits from IdentityUser, which is a part of ASP.NET Core Identity framework. It provides properties and methods for managing user authentication and authorization.
    {
        public string FirstName { get; set; } = string.Empty; // This property represents the first name of the user. It is initialized to an empty string to avoid null reference issues.
        public string LastName { get; set; } = string.Empty;// This property represents the last name of the user. It is also initialized to an empty string for the same reason.
        public int Age { get; set; }// This property represents the age of the user. It is of type int, which means it can hold whole numbers. The default value for an int is 0, so if not set, it will be 0.
    }
}
