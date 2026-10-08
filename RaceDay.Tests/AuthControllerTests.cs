using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using RaceDayApplication.Controllers;
using RaceDayApplication.DTOs;
using RaceDayApplication.Models;
using Xunit;

namespace RaceDayApplication.Tests
{
    public class AuthControllerTests
    {
        // Helper method to create a mock UserManager
        private Mock<UserManager<ApplicationUser>> GetMockUserManager()//mock user manager
        {
            var store = new Mock<IUserStore<ApplicationUser>>();//mock user store, that comes from the user manager, and is used to create a user manager

            // Mock the UserManager constructor parameters null because we don't need them for our tests because we are not testing the user manager itself, but the controller that uses it.
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        // Helper method to create a mock RoleManager
        private Mock<RoleManager<IdentityRole>> GetMockRoleManager()//mock role manager
        {
            var store = new Mock<IRoleStore<IdentityRole>>();//mock role store, that comes from the role manager, and is used to create a role manager

            // Mock the RoleManager constructor parameters null because we don't need them for our tests because we are not testing the role manager itself, but the controller that uses it.
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }

        [Fact]// Test for successful registration
        public async Task Register_ValidUser()
        {
            // Arrange
            var userManagerMock = GetMockUserManager();//mock user manager
            var roleManagerMock = GetMockRoleManager();

            // Setup the mock to return a successful result when creating a user and adding to a role, using It.IsAny to match any input parameters.
            //application user is the user that is being created, and string is the password that is being passed to the create async method, and the role is the role that is being passed to the add to role async method.
            //IDentityResult is the result of the operation, and we are returning a successful result.
            userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Setup the mock to return a successful result when adding a user to a role, using It.IsAny to match any input parameters.
            userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Setup the mock to return true when checking if a role exists, simulating a scenario where the role is already present in the database.
            roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                           .ReturnsAsync(true);

            //mock configuration, that is used to get the JWT secret key from the appsettings.json file
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Jwt:Key"]).Returns("dummy-secret-key");
            configMock.Setup(x => x["Jwt:Issuer"]).Returns("dummy-issuer");
            configMock.Setup(x => x["Jwt:Audience"]).Returns("dummy-audience");


            //create an instance of the AuthController with the mocked dependencies
            var controller = new AuthController(userManagerMock.Object, roleManagerMock.Object, configMock.Object);

            // Create an instance of RegisterDto with valid data for the test
            var dto = new RegisterDto
            {
                Email = "participant@test.com",
                Password = "Password123!",
                Role = "Participant"
            };

            // Act
            // Call the Register method of the AuthController with the test data
            var result = await controller.Register(dto);

            // Assert
            // Verify that the result is of type ObjectResult that comes from Microsoft.AspNetCore.Mvc namespace, which indicates a successful operation.
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact] // Test for login with invalid credentials
        public async Task Login_InvalidCredentials()
        {
            // Arrange
            var userManagerMock = GetMockUserManager();//mock user manager

            // Setup the mock to return null when searching for a user by email, simulating a scenario where the user does not exist in the database.
            userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                           .ReturnsAsync((ApplicationUser)null!); // User not found

            // Setup the mock to return false when checking the password, simulating a scenario where the provided password is incorrect.
            userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                           .ReturnsAsync(false); // Invalid password

            //mock configuration, that is used to get the JWT secret key from the appsettings.json file
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Jwt:Key"]).Returns("dummy-secret-key");

            //mock role manager, that is used to get the roles of the user from the database
            var roleManagerMock = GetMockRoleManager();

            //create an instance of the AuthController with the mocked dependencies
            var controller = new AuthController(userManagerMock.Object, roleManagerMock.Object, configMock.Object);

            // Create an instance of LoginDto with invalid credentials for the test
            var dto = new LoginDto { Email = "wrong@test.com", Password = "WrongPassword" };

            // Act
            // Call the Login method of the AuthController with the test data
            var result = await controller.Login(dto);

            // Assert
            // Verify that the result is of type UnauthorizedObjectResult that comes from Microsoft.AspNetCore.Mvc namespace, which indicates an unauthorized operation.
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}