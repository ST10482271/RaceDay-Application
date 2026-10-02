using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApplication.Data;
using RaceDayApplication.Models;
using System.Security.Claims;

namespace RaceDayApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Participant")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly AppDbContext _context;// Dependency injection of the database context

        // Constructor to initialize the controller with the database context, so we can access information from the database in the controller methods
        public EnrollmentsController(AppDbContext context)
        {
            _context = context;// Assign the injected context to the private field so it is accessible and secure
        }

        //this endpoint is used to enroll a participant in an event by providing the event name as a parameter in the URL
        [HttpPost("enroll/{eventName}")]
        public async Task<IActionResult> Enroll(string eventName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;// Get the current user's ID from the claims

            // Find the event in the database using the event name
            var sportsEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Name.ToLower() == eventName.Trim().ToLower());// Find the event by name, ignoring case and whitespace
            //using lambda expression to filter the events by name, and using FirstOrDefaultAsync to return the first matching event or null if not found
            //firstOrDefaultAsync comes from the Microsoft.EntityFrameworkCore namespace and is used to asynchronously retrieve the first element of a sequence.

            // Check if the event exists
            if (sportsEvent == null)
                return NotFound(new { Message = $"Event '{eventName}' was not found." });

            //  Check if user is already enrolled using the found event's ID
            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EventId == sportsEvent.Id && e.ParticipantId == userId);

            // Check if the user is already enrolled in the event
            if (existing != null)
                return BadRequest(new { Message = "You are already enrolled in this event." });

            //  Create the enrollment, instantiate a new Enrollment object, and set its properties
            var enrollment = new Enrollment
            {
                EventId = sportsEvent.Id,
                ParticipantId = userId,
                Status = EnrollmentStatus.Pending// Set the initial status of the enrollment to Pending
            };

            //  Save the enrollment to the database
            _context.Enrollments.Add(enrollment);// Add the new enrollment to the database context
            await _context.SaveChangesAsync();// Save the changes to the database asynchronously

            //  Return a success response with the enrollment ID
            return StatusCode(201, new { Message = "Enrollment submitted successfully.", EnrollmentId = enrollment.Id });
        }

        //this endpoint is used to retrieve the list of events that the current user is enrolled in, along with their enrollment status and result if available
        [HttpGet("my-events")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;//Get the current user's ID from the claims

            // Query the database for the enrollments of the current user, including related event and category information, and project the results into an anonymous type
            var myEnrollments = await _context.Enrollments
                .Include(e => e.Event)//Include meaning that the related Event entity will be loaded along with the Enrollment entity
                .ThenInclude(ev => ev.Category)
                .Include(e => e.Result)
                .Where(e => e.ParticipantId == userId)//Filter the enrollments to only include those for the current user

                //Project the results into an anonymous type with selected properties from the Enrollment, Event, Category, and Result entities
                .Select(e => new// Select meaning that we are creating a new object with the specified properties from the Enrollment entity and its related entities
                {
                    e.Id,
                    EventName = e.Event.Name,
                    e.Event.EventDate,
                    e.Event.Location,
                    Category = e.Event.Category.Name,
                    Status = e.Status.ToString(),
                    Result = e.Result != null ? new// Check if the Result is not null, and if so, create a new anonymous object with the Position and FinishTime properties
                    {
                        e.Result.Position,
                        FinishTime = e.Result.FinishTime.ToString(@"hh\:mm\:ss")
                    } : null
                })
                .ToListAsync();//this will return a list of enrollments for the current user, including event details, category, status, and result if available.

            // Return the list of enrollments as a JSON response
            return Ok(myEnrollments);
        }
    }
}
