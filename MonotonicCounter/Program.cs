using System.Diagnostics.Metrics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddHttpClient<CounterStoreClient>(c => c.BaseAddress = new($"http://{builder.Configuration["CounterStoreServiceHostName"]!}:8081"));

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

var meter = new Meter("Examples.MonotonicCounter", "1.0.0");
var incrementRequests = meter.CreateCounter<int>("srv.increment-request.count", "requests", "Number of increment operations");
var getRequests = meter.CreateCounter<int>("srv.get-request.count", "requests", "Number of get operations");

app.MapPost("/increment/{counter}/{delta}", async (string counter, long delta, CounterStoreClient client, ILogger<Program> logger) =>
    {
        logger.LogInformation("Increment counter operation invoked for Counter {counter} with delta {delta}", counter, delta);
        incrementRequests.Add(1);
        var response = await client.Client.PostAsync($"/increment/{counter}/{delta}", null);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<StoreResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    })
    .WithName("IncrementCounter")
    .WithOpenApi();

app.MapGet("/{counter}", async (string counter, CounterStoreClient client, ILogger<Program> logger) =>
    {
        logger.LogInformation("Get counter operation invoked for counter {counter}", counter);
        getRequests.Add(1);
        return await client.Client.GetFromJsonAsync<StoreResponse>($"/{counter}");
    })
    .WithName("GetCounter")
    .WithOpenApi();

app.Run();

internal record StoreResponse(string Name, long Value);

internal record CounterStoreClient(HttpClient Client);