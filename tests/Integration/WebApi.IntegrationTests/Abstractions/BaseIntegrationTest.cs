using DAL;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.IntegrationTests.Abstractions;

[Collection("Test collection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected IntegrationTestWebApplicationFactory Factory { get; init; }
    public HttpClient HttpClient { get; init; }
    protected AppDbContext DbContext { get; init; }

    public BaseIntegrationTest(IntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;

        HttpClient = factory.CreateClient();

        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Factory.ResetDatabaseAsync();
}
