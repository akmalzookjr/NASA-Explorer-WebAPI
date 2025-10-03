using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

public class NasaService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public NasaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["IxfYVjudB9jqhjE0MxhBCbOjR4CHBd59O3SewLDu"]; // Load from User Secrets
    }

    public async Task<dynamic> GetAstronomyPictureAsync()
    {
        var response = await _httpClient.GetStringAsync(
            $"https://api.nasa.gov/planetary/apod?api_key={_apiKey}");

        return JsonConvert.DeserializeObject<dynamic>(response);
    }
}
