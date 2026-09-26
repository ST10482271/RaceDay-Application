namespace RaceDayApplication.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    // This DTO is used for user login.
    // It contains the necessary properties that a client must provide when making a request to log in.
    // The properties include the user's email and password.
    //connects to the login endpoint in the AuthController and is used to authenticate users and generate a JWT token for successful logins.
    // It is a simple object that contains only the necessary properties for logging in, without any additional logic or behavior.
}
