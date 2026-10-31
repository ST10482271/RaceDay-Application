using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RaceDayApplication.Web.Services
{
    public class ApiService
    {
        // Field to store the injected factory used for creating named HTTP clients
        private readonly IHttpClientFactory _httpClientFactory;

        // Field to store the accessor used to retrieve current HTTP context data (Session, Claims, etc.)
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor to inject dependencies for HTTP client creation and context access
        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory; // Assigns the injected factory to the private field
            _httpContextAccessor = httpContextAccessor; // Assigns the injected context accessor to the private field
        }


        // Helper method to create an HTTP client and attach the user's JWT authentication token
        private HttpClient CreateClientWithToken()
        {
            // Creates a pre-configured HTTP client instance named "RaceDayApi"
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            // 1. Try getting token from Session
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");

            // 2. Fallback: Get token from User Claims (stored in the authentication cookie)
            if (string.IsNullOrEmpty(token))
            {
                // Searches user claims stored in the authentication cookie for the "JWTToken" claim
                token = _httpContextAccessor.HttpContext?.User?.FindFirstValue("JWTToken");
            }

            // 3. Attach the token if found
            if (!string.IsNullOrEmpty(token))
            {
                // Sets the Authorization header using the Bearer scheme with the retrieved token value
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Returns the fully configured HTTP client instance ready for execution
            return client;
        }

        // Generic GET method to retrieve and deserialize resources from a specified API endpoint
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            // Generates an authenticated HTTP client instance
            var client = CreateClientWithToken();

            // Sends an asynchronous GET request to the target API endpoint
            var response = await client.GetAsync(endpoint);

            // Checks if the HTTP response status code indicates success (200-299 range)
            if (response.IsSuccessStatusCode)
            {
                // Reads the raw JSON response content as an asynchronous string
                var content = await response.Content.ReadAsStringAsync();

                // Defines deserialization options for handling case-insensitive keys and string-to-enum mapping
                var options = new JsonSerializerOptions
                {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Ignores casing differences between JSON keys and C# model properties
                    Converters = { new JsonStringEnumConverter() }
                };

               // Deserializes the JSON string content into the requested generic type using the configured options
                return JsonSerializer.Deserialize<T>(content, options);
            }
            // Returns default value (null or default struct) if the request fails or is unsuccessful
            return default;
        }

        // Generic POST method to send data to an API endpoint and parse a typed response
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var client = CreateClientWithToken();

            // Serializes the incoming request data model object into a JSON formatted string
            var json = JsonSerializer.Serialize(data);

            // Encapsulates the JSON string into an HTTP content payload with UTF-8 encoding and application/json media type
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //
            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                // Reads the raw JSON response content string
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserializes the response content into the expected generic response type with case-insensitivity enabled
                return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            // Returns default value if the POST request fails
            return default;
        }

        // Simplified POST method that sends data and returns the raw HttpResponseMessage without explicit deserialization
        public async Task<HttpResponseMessage> SimplePostAsync<T>(string endpoint, T data)
        {
            var client = CreateClientWithToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PostAsync(endpoint, content);
        }

        // Generic PUT method to update a resource by sending JSON data and returning the raw HttpResponseMessage
        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
        {
            var client = CreateClientWithToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PutAsync(endpoint, content);
        }

        // Generic DELETE method to remove a resource at a specified API endpoint and return the raw response message
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            var client = CreateClientWithToken();

            // Sends an asynchronous DELETE request to the specified endpoint and returns the raw HTTP response message
            return await client.DeleteAsync(endpoint);
        }
    }
}
