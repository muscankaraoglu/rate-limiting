using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("ddosProtection", opt => 
    {
        opt.PermitLimit = 1000; // Son 10 dakika içinde en fazla 1000 istek
        opt.Window = TimeSpan.FromMinutes(10);
        opt.SegmentsPerWindow = 6; // Her 10 dakikalýk pencere 6 segmente bölünür
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 50;
    });
    options.OnRejected = async (context,token)=>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");
    };
    options.AddTokenBucketLimiter("tokenBucket", opt =>
    {
        opt.TokenLimit = 100; // Dakikada 100 jeton
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.TokensPerPeriod = 100; // Her dakika 100 jeton eklenecek
        opt.AutoReplenishment = true;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10; // Kuyrukta en fazla 10 istek bekletilebilir
    });
    options.AddFixedWindowLimiter(policyName: "userPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
    });
   
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Ýstek sýnýr sayýsýna ulaþtýnýz. {retryAfter.TotalMinutes} dakika sonra tekrar deneyiniz. ", cancellationToken: token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync(
                "Ýstek sýnýrýna ulaþtýnýz. Daha sonra tekrar deneyin. ", cancellationToken: token);
        }
    };
});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseRateLimiter();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();
