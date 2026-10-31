using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
/*using RaceDay_Application.DTOs;*/
using RaceDayApplication.Data;
using RaceDayApplication.DTOs;
using RaceDayApplication.Models;
using System.Security.Claims;

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

        //endpoint to delete an event by its Id
        [HttpDelete("events/{Id}")]
        public async Task<IActionResult> DeleteEvent(int Id)
        {
            var eventToDelete = await _context.Events.FirstOrDefaultAsync(e => e.Id == Id);// Find the event by its Id in the database from events table

            // If the event is not found, return a 404 Not Found response with a message
            if (eventToDelete == null)
                return NotFound(new { Message = "Event not found." });

            _context.Events.Remove(eventToDelete);// Remove the event from the Events table in the database context
            await _context.SaveChangesAsync();// Save the changes to the database asynchronously

            // Return a 200 OK response with a message indicating that the event was deleted successfully
            return Ok(new { Message = "Event deleted successfully." });
        }

        //this endpoint is used to create a new event in the database, and it requires the user to be authenticated and have the "Organizer" role.
        //The event details are provided in the request body as a JSON object that matches the EventCreateDto class.
        [HttpPost]
        [Authorize(Roles = "Organizer")]// This endpoint requires the user to be authenticated and have the "Organizer" role, which is checked by the Authorize attribute
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);// Get the user ID of the authenticated user from the claims, which is used to associate the event with the organiser

            var newEvent = new Event// Create a new Event entity and populate its properties from the DTO and the user ID
            {//DTO will communicate between the client and the server, and it will contain the data needed to create a new event
                Name = dto.Name,
                EventDate = dto.EventDate,
                Location = dto.Location,
                CategoryId = dto.CategoryId,
                OrganiserId = userId!// The exclamation mark is used to indicate that the userId is not null, since we expect the user to be authenticated and have a valid ID
                //so the organiser that logged in will be the one that created the event
            };

            // Add the new event to the database context and save the changes asynchronously, which will insert a new record in the Events table
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            // Return a 201 Created response with the new event data, which indicates that the event was successfully created and provides the details of the new event
            return StatusCode(201, newEvent);
        }

        //this endpoint is used to get the event so we can update/edit it
        [HttpGet("events/{id}")]
        public async Task<IActionResult> GetEventForEdit(int id)
        {
            var organiserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            var eventItem = await _context.Events
                .Where(e => e.Id == id && e.OrganiserId == organiserId)
                .Select(e => new UpdateEventDto
                {
                    /*Id = e.Id,*/
                    Name = e.Name,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    CategoryId = e.CategoryId
                })
                .FirstOrDefaultAsync();

            if (eventItem == null)
            {
                return NotFound("Event not found or unauthorized.");
            }

            return Ok(eventItem);
        }

        //this endpoint is used to update the events
        [HttpPut("events/{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var organiserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            //the organiserId is used so other organizers can not update another organizers's event, organizer and event is linked.
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganiserId == organiserId);

            if (existingEvent == null)
            {
                return NotFound("Event not found or you are not authorized to edit it.");
            }

            existingEvent.Name = model.Name;
            existingEvent.EventDate = model.EventDate;
            existingEvent.Location = model.Location;
            existingEvent.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        //endpoint to receive all events created by the orgainzer
        [HttpGet("my-events")]
        public async Task<IActionResult> GetOrganizerEvents()
        {
            // Extract the logged-in organizer's ID from JWT claims
            var organiserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(organiserId))
            {
                return Unauthorized("Organizer ID could not be found from token.");
            }

            // Fetch events created by this organizer and count enrollments
            var events = await _context.Events
                .Where(e => e.OrganiserId == organiserId)
                .Include(e => e.Category)
                .Include(e => e.Enrollments)
                .Select(e => new OrganizerEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    CategoryName = e.Category != null ? e.Category.Name : "Uncategorized",
                    EnrollmentCount = e.Enrollments.Count
                })
                .ToListAsync();

            return Ok(events);
        }

        //endpoint to get all participants from a certain event
        [HttpGet("events/{eventId}/participants")]
        public async Task<IActionResult> GetEventParticipants(int eventId)
        {
            var organiserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            // 1. Verify that the event exists and belongs to this organizer
            var eventItem = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OrganiserId == organiserId);

            if (eventItem == null)
            {
                return NotFound("Event not found or you are not authorized to view its participants.");
            }

            // 2. Project enrollments into EventParticipantDto using exact ApplicationUser properties
            var participants = await _context.Enrollments
                .Where(e => e.EventId == eventId)
                .Include(e => e.Participant)
                .Select(e => new EventParticipantDto
                {
                    EnrollmentId = e.Id,
                    ParticipantName = e.Participant != null
                        ? $"{e.Participant.FirstName} {e.Participant.LastName}".Trim()
                        : "Unknown Participant",
                    Email = e.Participant != null ? e.Participant.Email ?? "" : "",
                    Age = e.Participant != null ? e.Participant.Age : 0,
                    CategoryName = eventItem.Category != null ? eventItem.Category.Name : "Uncategorized",
                    Status = e.Status.ToString()
                })
                .ToListAsync();

            return Ok(participants);
        }
    }
}
