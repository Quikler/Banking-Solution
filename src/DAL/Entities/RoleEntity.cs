using Microsoft.AspNetCore.Identity;

namespace DAL.Entities;

public class RoleEntity : IdentityRole<Guid>
{
    public RoleEntity() { }

    public RoleEntity(string role) : base(role) { }
}
