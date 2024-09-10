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
            string url = "http://localhost:5000/WeatherForecast"; // Test etmek istedi�in endpoint URL'si
            int numberOfRequests = 100; // G�ndermek istedi�in toplam istek say�s�
            int concurrencyLevel = 10;  // Ayn� anda ka� istek g�nderilece�i

            // Testi ba�lat
            await RunTest(url, numberOfRequests, concurrencyLevel);

            // Sonu�lar� yazd�r
            Console.WriteLine("Test tamamland�.");
            Console.WriteLine($"Toplam istek say�s�: {numberOfRequests}");
            Console.WriteLine($"429 (Too Many Requests) hatas� say�s�: {statusCodes.Count(code => code == 429)}");
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
                            Console.WriteLine("429 Too Many Requests hatas� al�nd�.");
                        }
                    }
                });
            }

            await Task.WhenAll(tasks);
            Assert.True(hasError);
        }
    }
}