namespace WebApi.IntegrationTests.Abstractions;

[CollectionDefinition("Test collection")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebApplicationFactory>;
