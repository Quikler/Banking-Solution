using System.Net;
using System.Net.Http.Json;
using Contracts;
using Contracts.V1.Responses.Account;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using WebApi.IntegrationTests.Abstractions;
using WebApi.IntegrationTests.Extensions;

namespace WebApi.IntegrationTests.Account;

public class AccountControllerGetAccountsTests(IntegrationTestWebApplicationFactory factory) : BaseAccountTests(factory)
{
    [Fact]
    public async Task GetAccounts_WhenUsersExist_ShouldReturnAllAccounts()
    {
        // Arrange
        // Delete seeded users from db
        await DbContext.Users.ExecuteDeleteAsync();

        var user1 = await this.SignupUserAsync("first@test.com", TestPassword);
        var user2 = await this.SignupUserAsync("second@test.com", TestPassword);

        // Act
        var response = await HttpClient.GetAsync(ApiRoutes.Account.GetAccounts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<List<UserAccountResponse>>();
        content.ShouldNotBeNull();
        content.Count.ShouldBe(2);

        content.ShouldContain(u => u.Id == user1.User.Id && u.Email == user1.User.Email);
        content.ShouldContain(u => u.Id == user2.User.Id && u.Email == user2.User.Email);

        var usersFromDb = await DbContext.Users.ToListAsync();
        usersFromDb.ShouldContain(u => u.Id == user1.User.Id && u.Email == user1.User.Email);
        usersFromDb.ShouldContain(u => u.Id == user2.User.Id && u.Email == user2.User.Email);
    }

    [Fact]
    public async Task GetAccounts_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Act
        // Delete seeded users from db
        await DbContext.Users.ExecuteDeleteAsync();
        var response = await HttpClient.GetAsync(ApiRoutes.Account.GetAccounts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<List<UserAccountResponse>>();
        content.ShouldNotBeNull();
        content.ShouldBeEmpty();
        content.Count.ShouldBe(0);
    }
}
