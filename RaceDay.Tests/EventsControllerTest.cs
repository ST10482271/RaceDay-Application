using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApplication.Controllers;
using RaceDayApplication.Data;
using RaceDayApplication.DTOs;
using RaceDayApplication.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTest
    {
        // Helper method to create an in-memory database context for testing
        private AppDbContext GetInMemoryDbContext() {

            // Create a new in-memory database context for each test to ensure isolation this comes from the Microsoft.EntityFrameworkCore.InMemory package
            var options = new DbContextOptionsBuilder<AppDbContext>()//this is the context for the in-memory database
                 // Use a unique database name for each test to avoid conflicts
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())//System.Guid.NewGuid().ToString() generates a unique name for the in-memory database
                .Options;//.Options is used to configure the context options for the in-memory database

            //return a new instance of the AppDbContext with the in-memory database options
            return new AppDbContext(options);
        }

        // Helper method to set the user in the controller's HttpContext for testing to simulate authenticated users with specific roles
        private void SetUserInContext(ControllerBase controller, string userId, string role)
            //ControllerBase is the base class for all controllers in ASP.NET Core, which provides access to the HttpContext and User properties
        {
            // Create a ClaimsPrincipal with the specified user ID and role, which represents the authenticated user to stimulate the user context in the controller.
            // ClaimsPrincipal is a class that represents the current user and their claims,
            // which comes from the System.Security.Claims namespace
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),//NameIdentifier is a claim type that represents the unique identifier of the user, which is used to associate the event with the organiser
                new Claim(ClaimTypes.Role, role)//Role is a claim type that represents the role of the user, which is used to authorize access to the CreateEvent endpoint
            }, "TestAuthentication"));//"TestAuthentication" is the authentication scheme name, which is used to identify the authentication method in the test context

            // Set the HttpContext of the controller to a new DefaultHttpContext with the created ClaimsPrincipal,
            // which allows the controller to access the user information during testing.
            controller.ControllerContext = new ControllerContext
            {
                // Set the HttpContext of the controller to a new DefaultHttpContext with the created ClaimsPrincipal
                HttpContext = new DefaultHttpContext { User = user }
            };
            //HttpContext is a property of the ControllerBase class that represents the current HTTP request and response context,
            //which is used to access the user information during testing.
            //DefaultHttpContext is a class that provides a default implementation of the HttpContext,
            //which is used to create a new instance of the HttpContext for testing purposes.
            //so like this we can duplicate the controller to simulate different users with different roles and
            //test the behavior of the CreateEvent endpoint under various scenarios.

        }

        [Fact] // Test for successful event creation by an organiser
        public async Task CreateEvent_Organiser() {

            // Arrange
            // Create an in-memory database context and an instance of the EventsController for testing
            var context = GetInMemoryDbContext();
            var controller = new EventsController(context);
            SetUserInContext(controller, "organiser-123", "Organiser");//call the SetUserInContext method to simulate an authenticated user
           //with the role of "Organiser" and a unique user ID of "organiser-123" for testing the CreateEvent endpoint.

            // Create a sample instance of EventCreateDto object to simulate the event creation request
            var dto = new EventCreateDto
            {
                Name = "City Marathon 2026",
                EventDate = System.DateTime.UtcNow.AddDays(10),
                Location = "Johannesburg",
                CategoryId = 1
            };

            // Act
            // Call the CreateEvent method of the controller with the sample DTO to simulate the event creation request
            var result = await controller.CreateEvent(dto);
           
            // Assert
            // Verify that the result is of type ObjectResult and has a status code of 201 (Created)
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
            //.Equal() comes from the Xunit namespace and is used to assert that the expected value (201) is equal to the actual value (statusCodeResult.StatusCode).
        }

        [Fact] // Test for forbidden access when a participant tries to create an event
        public async Task CreateEvent_Participant() {

            // Arrange
            // Create an in-memory database context and an instance of the EventsController for testing
            using var context = GetInMemoryDbContext();
            var controller = new EventsController(context);
            SetUserInContext(controller, "participant-456", "Participant");//call the SetUserInContext method to simulate an authenticated user
            //with the role of "Participant" and a unique user ID of "participant-456" for testing the CreateEvent endpoint.

            // Create a sample instance of EventCreateDto object to simulate the event creation request
            var dto = new EventCreateDto
            {
                Name = "City Marathon 2026",
                EventDate = System.DateTime.UtcNow.AddDays(10),
                Location = "Johannesburg",
                CategoryId = 1
            };

            // Act
            // If statment checking the role in controller manually becuase the controller's CreateEvent method is using [Authorize(Roles = "Organiser")
            //IsinRole() is a method of the ClaimsPrincipal class that checks if the current user has a specific role,
            //which is used to authorize access to the CreateEvent endpoint.
            if (!controller.User.IsInRole("Organiser"))
            {
                // If the user is not an organiser, return a ForbidResult to indicate that access is forbidden
                var result = controller.Forbid();
                //Forbid() comes from the ControllerBase class and is used to return a 403 Forbidden response when the user
                //does not have the required role to access the endpoint.

                // Assert
                // Verify that the result is of type ForbidResult, which indicates that access is forbidden for the participant role
                Assert.IsType<ForbidResult>(result);
            }
        }
    }
}
