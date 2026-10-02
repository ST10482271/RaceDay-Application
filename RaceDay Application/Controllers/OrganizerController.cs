using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApplication.Data;
using RaceDayApplication.DTOs;
using RaceDayApplication.Models;

namespace RaceDayApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Organizer")]// This controller requires the user to be authenticated and have the "Organizer" role, which is checked by the Authorize attribute
    //this is for all the endpoints in this controller, meaning that only users with the "Organizer" role can access them
    public class OrganizerController : ControllerBase
    {
        private readonly AppDbContext _context;// Dependency injection of the database context

        // Constructor to initialize the controller with the database context, so we can access information from the database in the controller methods
        public OrganizerController(AppDbContext context)
        {
            _context = context;// Assign the injected context to the private field so it is accessible and secure
        }

        // Endpoint to update the status of an enrollment by its ID
        [HttpPut("enrollments/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);// Find the enrollment by its ID in the database from enrollments table

            // If the enrollment is not found, return a 404 Not Found response with a message
            if (enrollment == null)
                return NotFound(new { Message = "Enrollment not found." });

            enrollment.Status = dto.Status;// Update the status of the enrollment with the new status from the request body
            await _context.SaveChangesAsync();// Save the changes to the database asynchronously
            //asynchronously so that the thread is not blocked while waiting for the database operation to complete, improving performance and scalability

            return Ok(new { Message = $"Enrollment status updated to {dto.Status}." });
            // Return a 200 OK response with a message indicating the new status of the enrollment
        }

        // Endpoint to post race results for an enrollment
        [HttpPost("results")]
        public async Task<IActionResult> PostResult([FromBody] PostResultDto dto)
        {
            var enrollment = await _context.Enrollments.FindAsync(dto.EnrollmentId);// Find the enrollment by its ID in the database from enrollments table

            // If the enrollment is not found, return a 404 Not Found response with a message
            if (enrollment == null)
                return NotFound(new { Message = "Enrollment not found." });

            // If the enrollment is not accepted, return a 400 Bad Request response with a message
            if (enrollment.Status != EnrollmentStatus.Accepted)
                return BadRequest(new { Message = "Cannot post results for an unaccepted enrollment." });

            // Validate the finish time format (HH:mm:ss) and parse it to a TimeSpan
            if (!TimeSpan.TryParse(dto.FinishTime, out var parsedTime))
                return BadRequest(new { Message = "Invalid time format. Use HH:mm:ss." });

            // Create a new EventResult instance with the provided data and the parsed finish time
            var result = new EventResult
            {
                EnrollmentId = dto.EnrollmentId,
                Position = dto.Position,
                FinishTime = parsedTime
            };

            _context.EventResults.Add(result);// Add the new result to the EventResults table in the database context
            await _context.SaveChangesAsync();// Save the changes to the database asynchronously

            // Return a 201 Created response with a message indicating that the race result was posted successfully
            return StatusCode(201, new { Message = "Race result posted successfully." });
        }

        //endpoint to delete an event by its name
        [HttpDelete("events/{eventName}")]
        public async Task<IActionResult> DeleteEvent(string eventName)
        {
            var eventToDelete = await _context.Events.FirstOrDefaultAsync(e => e.Name == eventName);// Find the event by its name in the database from events table

            // If the event is not found, return a 404 Not Found response with a message
            if (eventToDelete == null)
                return NotFound(new { Message = "Event not found." });

            _context.Events.Remove(eventToDelete);// Remove the event from the Events table in the database context
            await _context.SaveChangesAsync();// Save the changes to the database asynchronously

            // Return a 200 OK response with a message indicating that the event was deleted successfully
            return Ok(new { Message = "Event deleted successfully." });
        }
    }
}
