namespace RaceDayApplication.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;// Email should be unique and is used as the username for authentication.
        public string Password { get; set; } = string.Empty;// Password should be hashed before storing in the database, the password hashing is done through the Identity framework in the AuthController.
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;//optional
        public int? Age { get; set; }//optional
        public string Role { get; set; } = "Participant"; // "Participant" or "Organizer" default is "Participant"
    }
    // This DTO is used for user registration.
    // It contains the necessary properties that a participant and organizer must provide when making a request to register a new user.
    // The properties include the user's email, password, first name, last name, phone number, age, and role.
    //It connects to the register endpoint in the AuthController and is used to create a new user account with the provided information.
    // It is a simple object that contains only the necessary properties for user registration, without any additional logic or behavior.
}
