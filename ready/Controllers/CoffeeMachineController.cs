using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ready.Interfaces;

namespace ready.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoffeeController : ControllerBase
    {
        private static int _callCount = 0;

        private static readonly HttpClient _httpClient = new HttpClient();

        [HttpGet("brew-coffee")]
        public async Task<IActionResult> BrewCoffee()
        {
            _callCount++;

            if (_callCount % 5 == 0)
            {
                return StatusCode(503);
            }

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
            {
                return StatusCode(418);
            }

            // Check weather service
            var weatherResponse = await GetWeather();
            if (weatherResponse != null && weatherResponse.main.temp > 30)
            {
                return Ok(new
                {
                    message = "Your refreshing iced coffee is ready",
                    prepared = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                });
            }

            return Ok(new
            {
                message = "Your piping hot coffee is ready",
                prepared = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
            });
        }

        private async Task<WeatherResponse> GetWeather()
        {
            try
            {
                var apiKey = "7f3baa3a2edd1d181e81db6c5ca72296"; // Replace with your API key
                var city = "Bali, CM"; // Replace with your city
                var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var weatherResponse =  await JsonSerializer.DeserializeAsync<WeatherResponse>(responseStream);

                    return weatherResponse;
                }

                return null;
            }
            catch (Exception)
            {
                // Log or handle the exception
                return null;
            }
        }
    }

    public class WeatherResponse
    {
        public Main main { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
    }

}

