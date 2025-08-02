using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Seed.User;

public static class UserSeed
{
    public static async Task SeedDefaultUsersAsync(this IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();

        var defaultUsers = new[]
        {
            new { Email = "test@test.com", Password = "test@test.com", },
            new { Email = "test2@test.com", Password = "test2@test.com", },
        };

        foreach (var userInfo in defaultUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
            if (existingUser is not null)
                continue;

            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                UserName = userInfo.Email,
                Email = userInfo.Email,
                EmailConfirmed = true,
            };

            var balance = new BalanceEntity
            {
                UserId = user.Id,
                User = user,
                Balance = 0,
            };

            user.Balance = balance;

            var result = await userManager.CreateAsync(user, userInfo.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user '{userInfo.Email}': {errors}");
            }
        }
    }
}
