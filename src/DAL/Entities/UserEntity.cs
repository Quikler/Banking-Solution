using Microsoft.AspNetCore.Identity;

namespace DAL.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public new string Email
    {
        get => base.Email ?? "";
        set => base.Email = value;
    }

    public Guid BalanceId { get; set; }
    public BalanceEntity Balance { get; set; } = null!;
}
