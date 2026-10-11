using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApplication.Controllers;
using RaceDayApplication.Data;
using RaceDayApplication.Models;
using Xunit;

namespace RaceDayApplication.Tests
{
    public class EnrollmentsControllerTest
    {
        // Helper method to create an in-memory database context for testing
        private AppDbContext GetInMemoryDbContext() {

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // Helper method to set the user in the controller context for testing
        private void SetUserInContext(ControllerBase controller, string userId) {

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "Participant")
            }, "TestAuthentication"));

            // Set the HttpContext of the controller to a new DefaultHttpContext with the created ClaimsPrincipal
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // Test for successful enrollment of a participant in an event
        [Fact]
        public async Task Successful_Enrollment()
        {
            // Arrange
            // Create an in-memory database context and add a sample event
            var context = GetInMemoryDbContext();
            var sportsEvent = new Event { Id = 1, Name = "Sprint 100m", Location = "Durban" };
            context.Events.Add(sportsEvent);
            // Save changes to the in-memory database
            await context.SaveChangesAsync();

            // Create an instance of the EnrollmentsController and set the user in the context
            var controller = new EnrollmentsController(context);

            // Set the user in the controller context to simulate a logged-in participant
            SetUserInContext(controller, "Participant-678");

            // Act
            // Call the Enroll method of the controller with the event name
            var result = await controller.Enroll("Sprint 100m");

            // Assert
            // Verify that the result is of type ObjectResult and has a status code of 201
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
            Assert.Equal(1, await context.Enrollments.CountAsync()); // Ensure that the enrollment was added to the database

        }


        // Test for attempting to enroll a participant who is already enrolled in the same event
        [Fact]
        public async Task Already_Enrolled() {

            // Arrange
            // Create an in-memory database context and add a sample event
            var context = GetInMemoryDbContext();
            var sportsEvent = new Event { Id = 1, Name = "Sprint 100m", Location = "Durban" };
            context.Events.Add(sportsEvent);

            // Add an existing enrollment for the same participant and event
            context.Enrollments.Add(new Enrollment { EventId = 1, ParticipantId = "Participant-678" });
            await context.SaveChangesAsync();

            // Create an instance of the EnrollmentsController and set the user in the context
            var controller = new EnrollmentsController(context);
            SetUserInContext(controller, "Participant-678");

            // Act
            // Call the Enroll method of the controller with the event name
            var result = await controller.Enroll("Sprint 100m");

            // Assert
            // Verify that the result is of type BadRequestObjectResult, indicating that the participant is already enrolled
            //with a message indicating that the participant is already enrolled
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("You are already enrolled in this event.", badRequestResult.Value.ToString());
        }

    }
}
