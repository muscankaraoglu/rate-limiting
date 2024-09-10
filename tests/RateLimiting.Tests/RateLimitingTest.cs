using System.Collections.Concurrent;
using Xunit.Sdk;

namespace RateLimiting.Tests
{
    public class RateLimitingTest
    {
        private static readonly HttpClient client = new HttpClient();
        private static ConcurrentBag<int> statusCodes = new ConcurrentBag<int>();


        [Fact]
        static async Task Main()
        {
            string url = "http://localhost:5000/WeatherForecast"; // Test etmek istediðin endpoint URL'si
            int numberOfRequests = 100; // Göndermek istediðin toplam istek sayýsý
            int concurrencyLevel = 10;  // Ayný anda kaç istek gönderileceði

            // Testi baþlat
            await RunTest(url, numberOfRequests, concurrencyLevel);

            // Sonuçlarý yazdýr
            Console.WriteLine("Test tamamlandý.");
            Console.WriteLine($"Toplam istek sayýsý: {numberOfRequests}");
            Console.WriteLine($"429 (Too Many Requests) hatasý sayýsý: {statusCodes.Count(code => code == 429)}");
        }


        private static async Task RunTest(string url, int numberOfRequests, int concurrencyLevel)
        {
            bool hasError = false;
            var tasks = new Task[concurrencyLevel];

            for (int i = 0; i < concurrencyLevel; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    for (int j = 0; j < numberOfRequests / concurrencyLevel; j++)
                    {
                        var response = await client.GetAsync(url);
                        statusCodes.Add((int)response.StatusCode);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            hasError = true;
                            Console.WriteLine("429 Too Many Requests hatasý alýndý.");
                        }
                    }
                });
            }

            await Task.WhenAll(tasks);
            Assert.True(hasError);
        }
    }
}