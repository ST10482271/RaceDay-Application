using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApplication.Data;
using RaceDayApplication.DTOs;
using RaceDayApplication.Models;
using System.Security.Claims;

namespace RaceDayApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase//inherits from ControllerBase class, which provides the basic functionality for handling HTTP requests and responses in an API controller.
    {
        private readonly AppDbContext _context;//Dependency injection of the database context

        //// Constructor to initialize the controller with the database context, so we can access information from the database in the controller methods
        public EventsController(AppDbContext context)
        {
            _context = context;// Assign the injected context to the private field so it is accessible and secure
        }

        //this endpoint is used to retrieve all events from the database, optionally filtered by category name provided as a query parameter in the URL
        [HttpGet("({categoryName})")]
        public async Task<IActionResult> GetAllEvents([FromQuery] string categoryName)
        {
            var query = _context.Events//access the Events table in the database
                .Include(e => e.Category)// Include the related Category entity for each event, so we can access the category name
                .Include(e => e.Organiser)// Include the related Organiser entity for each event, so we can access the organiser's name
                .AsQueryable();// Convert the query to an IQueryable so we can apply filters dynamically
            //meaning that the query is not executed yet, and we can add more conditions to it before executing it

            // Filter by category name if provided, using a case-insensitive comparison and trimming whitespace
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query = query.Where(e => e.Category.Name.ToLower() == categoryName.Trim().ToLower());
            }

            // Execute the query and project the results into an anonymous type with selected properties, including the category name and organiser's full name
            var events = await query.Select(e => new//Select meaning that we are creating a new object with the specified properties from the Event entity and its related entities
            {
                e.Id,
                e.Name,
                e.EventDate,
                e.Location,
                Category = e.Category.Name,
                Organizer = e.Organiser.FirstName + " " + e.Organiser.LastName
            }).ToListAsync();// Execute the query asynchronously and return the results as a list

            return Ok(events);
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

        //this endpoint is used to retrieve all categories from the database, and it does not require any authentication or authorization.
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategory([FromQuery] Models.Category category)
        {
            //Display all categories available in the database
            var categories = await _context.Categories.ToListAsync();// Execute the query asynchronously and return the results as a list
            return Ok(categories);// Return a 200 OK response with the list of categories
        }

        
    }
}
