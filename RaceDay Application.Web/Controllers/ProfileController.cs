using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayApplication.Web.Models;
using RaceDayApplication.Web.Services;

namespace RaceDayApplication.Web.Controllers
{
    [Authorize] // Ensures both Participants and Organizers must be logged in
    public class ProfileController : Controller
    {
       
            private readonly ApiService _apiService;

            public ProfileController(ApiService apiService)
            {
                _apiService = apiService;
            }

        

        // GET: /Profile/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Calls GET api/auth/profile
            var profile = await _apiService.GetAsync<ProfileViewModel>("auth/profile");

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Failed to load user profile.";
                return RedirectToAction("Index", "Home");
            }

            return View(profile);
        }

        // POST: /Profile/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model, IFormFile? profilePicture)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Local Profile Picture upload handling
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                model.ProfilePictureUrl = "/uploads/" + uniqueFileName;
            }

            // Calls PUT api/auth/profile using the UpdateProfileDto structure
            var response = await _apiService.PutAsync<ProfileViewModel>("auth/profile", model);

            if (response != null)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to update profile. Check your details or current password.");
            return View(model);
        }


          
                      
    }
}
