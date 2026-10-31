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
        [HttpGet]
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
        

        //this endpoint is used to retrieve all categories from the database, and it does not require any authentication or authorization.
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategory([FromQuery] Models.Category category)
        {
            //Display all categories available in the database
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description
                })
                .ToListAsync();// Execute the query asynchronously and return the results as a list
            return Ok(categories);// Return a 200 OK response with the list of categories
        }

        
    }
}
