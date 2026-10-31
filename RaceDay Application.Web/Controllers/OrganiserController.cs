using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayApplication.Web.Models;
using RaceDayApplication.Web.Services;

namespace RaceDayApplication.Web.Controllers
{
    // Controller responsible for event organizer tasks: managing events, participants, and race results
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        // Injects ApiService dependency to handle backend API requests
        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Organiser/Index - Fetches and displays all events created by the logged-in organizer
        public async Task<IActionResult> Index()
        {
            // Call the Web API endpoint to get the organizer's events
            var events = await _apiService.GetAsync<List<OrganiserEvents>>("Organizer/my-events");

            if (events == null)
            {
                // Fallback to empty list if API returns null
                events = new List<OrganiserEvents>();
            }

            return View(events);
        }

        // GET: /Organiser/CreateEvent - Displays the form to create a new event and loads categories for dropdown
        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            // Fetch categories from API to populate the category dropdown list
            var categories = await _apiService.GetAsync<List<CategoryViewModel>>("Events/categories");
            ViewBag.Categories = categories ?? new List<CategoryViewModel>();

            return View(new CreateEvent());
        }

        // POST: /Organiser/CreateEvent - Submits new event data to the backend API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEvent model)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate categories if form validation fails
                var categories = await _apiService.GetAsync<List<CategoryViewModel>>("Events/categories");
                ViewBag.Categories = categories ?? new List<CategoryViewModel>();
                return View(model);
            }

            // Call Web API POST endpoint to create the event
            var success = await _apiService.PostAsync<CreateEvent, CreateEvent>("Organizer", model);

            if (success != null)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Display error and reload dropdown if API fails
            ModelState.AddModelError(string.Empty, "Failed to create event. Please try again.");

            var categoryList = await _apiService.GetAsync<List<CategoryViewModel>>("Events/categories");
            ViewBag.Categories = categoryList ?? new List<CategoryViewModel>();
            return View(model);
        }

        // GET: /Organiser/Participants/{eventId} - Fetches list of participants registered for a specific event
        [HttpGet]
        public async Task<IActionResult> Participants(int eventId)
        {
            var participants = await _apiService.GetAsync<List<EventParticipants>>($"organizer/events/{eventId}/participants");

            if (participants == null)
            {
                participants = new List<EventParticipants>();
            }

            // Pass eventId to ViewBag for view header context
            ViewBag.EventId = eventId;

            return View(participants);
        }

        // GET: /Organiser/PostResult - Displays form for submitting a participant's race result
        [HttpGet]
        public async Task<IActionResult> PostResult(int enrollmentId, int eventId)
        {
            var model = new PostResults
            {
                EnrollmentId = enrollmentId,
                EventId = eventId
            };
            return View(model);
        }

        // POST: /Organiser/PostResult - Sends race results for an enrolled participant to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostResult(PostResults model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Call Web API endpoint to save the result
            var success = await _apiService.PostAsync<PostResults, PostResults>("Organizer/results", model);

            if (success != null)
            {
                TempData["SuccessMessage"] = "Race result posted successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to post result. Please try again.");
            return View(model);
        }

        // POST: /Organiser/UpdateStatus - Updates participant enrollment status (e.g. Approved, Pending, Cancelled)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int enrollmentId, int eventId, EnrollmentStatus status)
        {
            var dto = new UpdateStatus { Status = status };

            // Send PUT request to API to update enrollment status
            var success = await _apiService.PutAsync<UpdateStatus>($"organizer/enrollments/{enrollmentId}/status", dto);

            if (success != null)
            {
                TempData["SuccessMessage"] = "Participant status updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update participant status.";
            }

            return RedirectToAction(nameof(Participants), new { eventId = eventId });
        }

        // GET: /Organiser/Edit/{id} - Loads existing event details and category dropdown for editing
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Fetch existing event details from API
            var existingEvent = await _apiService.GetAsync<UpdateEvent>($"Organizer/events/{id}");
            if (existingEvent == null)
            {
                return NotFound();
            }

            //  Load categories for dropdown selection
            ViewBag.Categories = await _apiService.GetAsync<List<CategoryViewModel>>("Events/categories")
                                 ?? new List<CategoryViewModel>();

            return View(existingEvent);
        }

        // POST: /Organiser/Edit/{id} - Submits updated event details to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEvent model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _apiService.GetAsync<List<CategoryViewModel>>("Events/categories")
                ?? new List<CategoryViewModel>();
                return View(model);
            }

            // Send PUT request to API endpoint: api/organizer/events/{id}
            var success = await _apiService.PutAsync<UpdateEvent>($"Organizer/events/{id}", model);

            if (success != null)
            {
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to update event. Please try again.");
            ViewBag.Categories = await _apiService.GetAsync<List<CategoryViewModel>>("Events/categories")
            ?? new List<CategoryViewModel>();
            return View(model);
        }

        // POST: /Organiser/Delete/{id} - Sends a delete request to remove an event
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Calls API DELETE endpoint: api/organizer/events/{id}
            var success = await _apiService.DeleteAsync($"Organizer/events/{id}");

            if (success != null)
            {
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the event.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}