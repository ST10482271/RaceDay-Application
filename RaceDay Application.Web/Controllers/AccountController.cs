using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDayApplication.Web.Models;
using RaceDayApplication.Web.Services;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace RaceDayApplication.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        // Injects the ApiService dependency to handle backend API communication
        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Account/Login - Displays the login form view
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login - Authenticates user credentials, stores the JWT token, and signs the user into the cookie scheme
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Return the form view if client-side validation fails
            if (!ModelState.IsValid) return View(model);

            // 1. Call the Web API login endpoint and retrieve the token response
            var tokenResponse = await _apiService.PostAsync<LoginViewModel, TokenResponseDto>("auth/login", model);

            // Verify that a valid token was returned from the API
            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
            {
                // 1. Store the JWT token in session storage for subsequent API requests via ApiService
                HttpContext.Session.SetString("JWTToken", tokenResponse.Token);

                // 2. Initialize base user claims with email and JWT reference
                var claims = new List<Claim>
               {
                 new Claim(ClaimTypes.Name, model.Email),
                 new Claim("JWTToken", tokenResponse.Token)
               };

                // Read and parse the JWT token to extract embedded claims (such as Roles and NameIdentifier) from the backend
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(tokenResponse.Token))
                {
                    var jwtToken = handler.ReadJwtToken(tokenResponse.Token);
                    foreach (var claim in jwtToken.Claims)
                    {
                        // Add all token claims except duplicate Name entries
                        if (claim.Type != ClaimTypes.Name)
                        {
                            claims.Add(new Claim(claim.Type, claim.Value));
                        }
                    }
                }

                // Build the claims identity and sign the user into the cookie authentication scheme
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                TempData["SuccessMessage"] = "Logged in successfully!";
                return RedirectToAction("Index", "Home");
            }

            // Add model-level error if login fails and return the view
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: /Account/Register - Displays the user registration form view
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register - Submits registration details to the Web API backend
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validate model state before proceeding
            if (!ModelState.IsValid)
                return View(model);

            // Send registration payload to the API registration endpoint
            var response = await _apiService.SimplePostAsync("auth/register", model);

            // Redirect to login on successful registration
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));
            }

            // Display error message if registration fails
            ModelState.AddModelError(string.Empty, "Registration failed. Please check your details.");
            return View(model);
        }

        // POST: /Account/Logout - Clears user session and signs out of the authentication cookie
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}