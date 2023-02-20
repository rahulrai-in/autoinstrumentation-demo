using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add Redis service
var redisConnection = ConnectionMultiplexer.Connect(builder.Configuration["RedisHost"]!);
var redis = redisConnection.GetDatabase();

var app = builder.Build();

app.MapGet("/{counter}", async (string counter) =>
{
    var value = await redis.StringGetAsync(counter.ToLowerInvariant());
    return new CounterStateResponse(counter.ToLowerInvariant(), value.TryParse(out int count) ? count : 0);
});
app.MapPost("increment/{counter}/{delta}", async (string counter, int delta) =>
{
    // Throws exception if delta is less than 0
    if (delta < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(delta), delta, "Delta should be greater than or equal to 0");
    }

    var value = await redis.StringIncrementAsync(counter.ToLowerInvariant(), delta);
    return new CounterStateResponse(counter.ToLowerInvariant(), value);
});

app.Run();

internal record CounterStateResponse(string Name, long Value);