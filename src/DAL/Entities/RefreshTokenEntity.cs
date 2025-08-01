namespace DAL.Entities;

public class RefreshTokenEntity : BaseEntity
{
    public string Token { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
}
