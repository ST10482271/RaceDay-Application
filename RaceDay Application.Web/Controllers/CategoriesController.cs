using Microsoft.AspNetCore.Mvc;
using RaceDayApplication.Web.Models;
using RaceDayApplication.Web.Services;

namespace RaceDayApplication.Web.Controllers
{
    // Controller responsible for displaying event categories in the MVC application
    public class CategoriesController : Controller
    {
        private readonly ApiService _apiService;

        // Injects ApiService dependency to handle communication with backend API endpoints
        public CategoriesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Categories/Index - Fetches all event categories from the Web API and passes them to the Index view
        public async Task<IActionResult> Index()
        {
            // Retrieves category list from backend endpoint: api/events/categories
            var categories = await _apiService.GetAsync<List<CategoryViewModel>>("events/categories");

            // Returns the view with categories or an empty list if null
            return View(categories ?? new List<CategoryViewModel>());
        }
    }
}
