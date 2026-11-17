using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_NetCore_WebAPI_Otel_TestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            using (var activity = Telemetry.Source.StartActivity("GenerateWeatherForecast"))
            {

                return Enumerable.Range(1, 5).Select(index =>
                {
                    var summary = Summaries[Random.Shared.Next(Summaries.Length)];

                    activity.AddEvent(
                new ActivityEvent(
                    "weather.summary.selected",
                    tags: new ActivityTagsCollection
                            {
                    { "weather.summary", summary },
                    { "weather.forecast.index", index }
                            }));

                    return new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summary
                    };
                })
                    .ToArray();
            }
        }
    }
}
