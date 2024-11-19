namespace lab6_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public struct Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public float Temp { get; set; }
        public string Description { get; set; }
    }

    public class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string ApiKey = "b8b395f38e3311a84b4e5f34eae8ccd4";
        private const int NumberOfRequests = 50; // Кол-во запросов

        public static async Task Main(string[] args)
        {
            List<Weather> weatherList = new List<Weather>();
            Random random = new Random();

            while (weatherList.Count < NumberOfRequests)
            {
                // Генерация случайных координат
                float latitude = (float)(random.NextDouble() * 180 - 90);
                float longitude = (float)(random.NextDouble() * 360 - 180);

                var weatherData = await GetWeatherDataAsync(latitude, longitude);
                if (weatherData != null)
                {
                    weatherList.Add(weatherData.Value);
                }
            }

            // Максимальная и минимальная tem
            var maxTempCountry = weatherList.OrderByDescending(w => w.Temp).First();
            var minTempCountry = weatherList.OrderBy(w => w.Temp).First();

            Console.WriteLine($"Страна с максимальной температурой: {maxTempCountry.Country} ({maxTempCountry.Temp}°C)");
            Console.WriteLine($"Страна с минимальной температурой: {minTempCountry.Country} ({minTempCountry.Temp}°C)");
            Console.WriteLine($"Средняя температура в мире: {weatherList.Average(w => w.Temp)}°C");
            Console.WriteLine($"Количество стран в коллекции: {weatherList.Select(w => w.Country).Distinct().Count()}");

            var firstClearDescription = weatherList.FirstOrDefault(w =>
                w.Description == "clear sky" || w.Description == "rain" || w.Description == "few clouds");
            if (!firstClearDescription.Equals(default(Weather)))
            {
                Console.WriteLine($"Первая найденная страна и местность: {firstClearDescription.Country}, {firstClearDescription.Name}");
            }
        }

        private static async Task<Weather?> GetWeatherDataAsync(float latitude, float longitude)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={ApiKey}&units=metric";
            try
            {
                var response = await client.GetStringAsync(url);
                dynamic json = JsonConvert.DeserializeObject(response);
                if (json != null && json.sys != null)
                {
                    string country = json.sys.country;
                    string name = json.name;
                    float temp = json.main.temp;
                    string description = json.weather[0].description;

                    return new Weather
                    {
                        Country = country,
                        Name = name,
                        Temp = temp,
                        Description = description
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения данных о погоде: {ex.Message}.");
            }

            return null;
        }
    }
}