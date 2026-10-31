using Microsoft.AspNetCore.Mvc;
using RaceDayApplication.Web.Models;
using RaceDayApplication.Web.Services;

namespace RaceDayApplication.Web.Controllers
{
    // Controller responsible for displaying race events and processing user event enrollments
    public class EventsController : Controller
    {
        private readonly ApiService _apiService;

        // Injects ApiService dependency to handle backend API communication
        public EventsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Events/Index - Fetches all events or filters events by category name if provided
        public async Task<IActionResult> Index(string? categoryName)
        {
            string endpoint = "Events";

            // Append URL-encoded category filter parameter if specified
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                endpoint = $"Events?categoryName={Uri.EscapeDataString(categoryName)}";
            }

            // Retrieve events list from API endpoint and pass current filter state to ViewData
            var events = await _apiService.GetAsync<List<EventViewModel>>(endpoint);
            ViewData["CurrentCategory"] = categoryName;

            // Return view with events list or an empty list if null
            return View(events ?? new List<EventViewModel>());
        }

        // POST: /Events/Enrol/{id} - Sends enrollment request for a specific race event to the Web API
        [HttpPost]
        public async Task<IActionResult> Enrol(string eventName)
        {
            // Submit enrollment payload to the backend enrollment endpoint
            var response = await _apiService.SimplePostAsync<object?>($"Enrollments/enroll/{eventName}", null);

            // Set feedback banner message based on API response status
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully enrolled in the race!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to enrol. You may already be enrolled.";
            }

            // Redirect user back to events list index
            return RedirectToAction(nameof(Index));
        }
    }
}