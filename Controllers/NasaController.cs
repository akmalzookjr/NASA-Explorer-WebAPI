using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class NasaController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "IxfYVjudB9jqhjE0MxhBCbOjR4CHBd59O3SewLDu"; // later move to appsettings.json
                                                                                  // Cache storage (in-memory)
    private static string _cachedEpicJson = string.Empty;
    private static DateTime _lastEpicFetch = DateTime.MinValue;


    public NasaController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ✅ Astronomy Picture of the Day
    public async Task<IActionResult> Apod()
    {
        var url = $"https://api.nasa.gov/planetary/apod?api_key={_apiKey}";
        var json = await _httpClient.GetStringAsync(url);
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        return View(result);
    }

    // ✅ Mars Rover Search Form
    [HttpGet]
    public IActionResult MarsRover()
    {
        return View(new List<JsonElement>()); // Empty list on first load
    }

    // ✅ Mars Rover Search Result
    [HttpPost]
    public async Task<IActionResult> MarsRover(string rover, int sol)
    {
        var url = $"https://api.nasa.gov/mars-photos/api/v1/rovers/{rover}/photos?sol={sol}&api_key={_apiKey}";
        var json = await _httpClient.GetStringAsync(url);

        var result = JsonSerializer.Deserialize<JsonElement>(json);

        var photos = result.GetProperty("photos").EnumerateArray();

        return View(photos);
    }

    // ✅ Asteroid Dashboard
    public async Task<IActionResult> Asteroids()
    {
        // 1. Get date range (today → next 7 days)
        var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

        // 2. NASA NeoWs API URL
        var url = $"https://api.nasa.gov/neo/rest/v1/feed?start_date={startDate}&end_date={endDate}&api_key={_apiKey}";

        // 3. Call NASA API → get JSON string
        var json = await _httpClient.GetStringAsync(url);

        // 4. Deserialize JSON
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        // 5. Get "near_earth_objects"
        var nearEarthObjects = result.GetProperty("near_earth_objects");

        // 6. Prepare dataset (date + count of asteroids)
        var asteroidData = new List<object>();

        foreach (var day in nearEarthObjects.EnumerateObject())
        {
            var date = day.Name;                      // e.g. "2025-09-29"
            var count = day.Value.GetArrayLength();   // how many asteroids that day
            asteroidData.Add(new { Date = date, Count = count });
        }

        // 7. Pass data to View
        ViewData["AsteroidData"] = JsonSerializer.Serialize(asteroidData);

        // 8. Render the View (Asteroids.cshtml)
        return View();
    }

    // ✅ EPIC Earth Images
    // ✅ EPIC Earth Images with caching
    public async Task<IActionResult> Epic()
    {
        try
        {
            var url = $"https://api.nasa.gov/EPIC/api/natural/images?api_key={_apiKey}";
            var json = await _httpClient.GetStringAsync(url);

            // Save to cache
            _cachedEpicJson = json;
            _lastEpicFetch = DateTime.UtcNow;

            var result = JsonSerializer.Deserialize<JsonElement>(json);
            var images = result.EnumerateArray().Take(10).ToList();

            return View(images);
        }
        catch (HttpRequestException)
        {
            if (!string.IsNullOrEmpty(_cachedEpicJson))
            {
                // Use cached response if API is down
                var result = JsonSerializer.Deserialize<JsonElement>(_cachedEpicJson);
                var images = result.EnumerateArray().Take(10).ToList();

                ViewData["Error"] = "NASA EPIC API is unavailable. Showing cached images.";
                return View(images);
            }

            // No cache available
            ViewData["Error"] = "NASA EPIC API is currently unavailable. Please try again later.";
            return View(new List<JsonElement>());
        }
    }




}