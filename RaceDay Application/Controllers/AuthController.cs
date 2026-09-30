using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;//the JwtSecurityTokenHandler class is used to create and validate JSON Web Tokens (JWTs). It provides methods for generating JWTs, validating their signatures, and extracting claims from them.
using System.Security.Claims;//the Claim class represents a claim, which is a piece of information about the user that is included in the JWT. Claims can be used to store user-specific data, such as their ID, email, roles, and other relevant information.
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;//the SymmetricSecurityKey class is used to create a symmetric security key for signing the JWT. It takes a byte array as input, which is derived from a secret key stored in the configuration settings.
using RaceDayApplication.DTOs;
using RaceDayApplication.Models;

namespace RaceDayApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;// UserManager is a service provided by ASP.NET Core Identity that allows you to manage users, including creating, updating, deleting, and retrieving user information.
        private readonly RoleManager<IdentityRole> _roleManager;// RoleManager is a service provided by ASP.NET Core Identity that allows you to manage roles, including creating, updating, deleting, and retrieving role information.
        private readonly IConfiguration _configuration;// IConfiguration is a service that provides access to configuration settings, such as those defined in appsettings.json or environment variables.

        public AuthController(//the constructor of the AuthController class, which is used to inject dependencies into the controller.
            UserManager<ApplicationUser> userManager,//the injected UserManager service is assigned to the private field _userManager.
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;//the injected UserManager service is assigned to the private field _userManager.
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
            //the Register method is an action method that handles user registration requests. It takes a RegisterDto object as input, which contains the user's registration details.
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);//the FindByEmailAsync method of the UserManager service is called to check if a user with the provided email already exists in the database.
            if (userExists != null)
                return BadRequest(new { Message = "User with this email already exists." });
            
            var user = new ApplicationUser//an instance of the ApplicationUser class is created to represent the new user being registered.
            {
                UserName = model.Email,//the UserName property of the ApplicationUser object is set to the email provided in the RegisterDto object. This means that the user's email will be used as their username for authentication purposes.
                Email = model.Email,//the UserName and Email properties of the ApplicationUser object are set to the email provided in the RegisterDto object, that communicates with the database(the DTO) to store the user's email as their username and email address.
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Age = (int)model.Age,//the Age property of the ApplicationUser object is set to the age provided in the RegisterDto object. The age is cast to an integer since the Age property in the ApplicationUser class is defined as an integer.
                EmailConfirmed = true//the EmailConfirmed property of the ApplicationUser object is set to true, indicating that the user's email is considered confirmed. This means that the user will not need to go through an email confirmation process after registration.
            };

            //the CreateAsync method of the UserManager service is called to create a new user in the database with the provided details and password.
            //await is used to asynchronously wait for the completion of the CreateAsync method, which creates the user in the database.
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            //the role is determined based on the Role property of the RegisterDto object. If the role is "Organizer" (case-insensitive), it is assigned as "Organizer"; otherwise, it defaults to "Participant".
            string role = string.Equals(model.Role, "Organizer", StringComparison.OrdinalIgnoreCase)
                ? "Organizer"
                : "Participant";

            //the RoleExistsAsync method of the RoleManager service is called to check if the specified role already exists in the database. If the role does not exist, it is created using the CreateAsync method of the RoleManager service.
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);//the AddToRoleAsync method of the UserManager service is called to assign the specified role to the newly created user.

            return StatusCode(201, new { Message = "User registered successfully.", UserId = user.Id, Role = role });
            //the method returns a 201 Created status code along with a JSON response containing a success message
            //the newly created user's ID and the assigned role. This indicates that the user registration was successful.

        }
        //so, the Register method handles user registration, creates a new user in the database, assigns a role to the user, and returns a response indicating the success of the registration process.

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        //this method handles user login requests. It takes a LoginDto object as input, which contains the user's email and password.
        //asynchronous method because it performs asynchronous operations, such as querying the database for user information and checking the password.
        {
            //the FindByEmailAsync method of the UserManager service is called to retrieve the user with the provided email from the database. If no user is found, or if the password does not match, an Unauthorized response is returned.
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { Message = "Invalid credentials." });

            //the GetRolesAsync method of the UserManager service is called to retrieve the roles assigned to the authenticated user.
            //This allows the application to include the user's roles in the JWT claims for authorization purposes.
            var userRoles = await _userManager.GetRolesAsync(user);

            //the list of claims is created to include information about the authenticated user, such as their ID, email, first name, last name, and a unique identifier (JTI) for the JWT.
            var authClaims = new List<Claim> 
             // a claim is a piece of information about the user that is included in the JWT. Claims can be used to store user-specific data, such as their ID, email, roles, and other relevant information.
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())//to create a new claim with the type Jti (JWT ID) and a unique identifier generated using Guid.NewGuid().ToString().
                //the JTI (JWT ID) claim is added to the list of claims, which is a unique identifier for the JWT.
                //This can be used to prevent token replay attacks.
            };
            //the claims will be called or used in other parts of the application e.g. in other controllers,
            //and in authorization policies or when generating the JWT for the authenticated user.

            // Add user roles to claims
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            //the signing key for the JWT is created using a symmetric security key derived from a secret key stored in the appsettings.json.
            //This key is used to sign the JWT and ensure its integrity.
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            //the JwtSecurityToken object is created, which represents the JWT to be issued. It includes information such as the issuer, audience, expiration time, claims, and signing credentials.
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],//the issuer of the JWT, which is typically the URL or identifier of the application issuing the token.
                audience: _configuration["Jwt:Audience"],//the intended audience of the JWT, which is typically the URL or identifier of the application that will consume the token.
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,//the claims that were created earlier, which include information about the authenticated user and their roles.
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            //the signing credentials used to sign the JWT, which include the signing key and the algorithm used for signing (HMAC SHA-256 in this case),
            //which will have to match the algorithm specified in the JWT header when the token is validated by the server,
            //that comes from the client when making requests to the API.
            );

            //the method returns an Ok response with a JSON object containing the generated JWT, its expiration time, the user's roles, and a success message.
            //The JWT can be used by the client for subsequent authenticated requests to the API.
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                //the WriteToken method of the JwtSecurityTokenHandler class is called to serialize the JwtSecurityToken object into a string representation of the JWT to be returned to the client.
                expiration = token.ValidTo,
                //the ValidTo property of the JwtSecurityToken object is accessed to retrieve the expiration time of the token, which indicates when the token will no longer be valid.
                roles = userRoles,
                message = "Login successful."
            });
            //so , the Login method handles user authentication, generates a JWT for the authenticated user, and returns it to the client along with relevant information such as expiration time and user roles.

            //so JWT uses signing credentials to ensure the integrity and authenticity of the token,
            //allowing the server to verify that the token has not been tampered with and was issued by a trusted source
            //by using claims to store user-specific information that can be used for authorization and access control in the application.
        }
    }
}
