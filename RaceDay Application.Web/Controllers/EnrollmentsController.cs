using Microsoft.AspNetCore.Mvc;
using RaceDayApplication.Web.Models;
using RaceDayApplication.Web.Services;


namespace RaceDayApplication.Web.Controllers
{
    // Controller responsible for managing user event enrollment views
    public class EnrollmentsController : Controller
    {
        private readonly ApiService _apiService;

        // Injects ApiService dependency to handle backend API communication
        public EnrollmentsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Enrollments/Index - Fetches the authenticated user's enrolled events and passes them to the Index view
        public async Task<IActionResult> Index()
        {
            // Calls backend API endpoint: api/Enrollments/my-events to retrieve user enrollments
            var enrollments = await _apiService.GetAsync<List<MyEnrollmentViewModel>>("Enrollments/my-events");

            // Ensures a non-null list is passed to the view if no records are returned
            if (enrollments == null)
            {
                enrollments = new List<MyEnrollmentViewModel>();
            }

            return View(enrollments);
        }
    }
}