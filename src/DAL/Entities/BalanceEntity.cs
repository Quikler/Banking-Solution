namespace DAL.Entities;

public class BalanceEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public decimal Balance { get; set; }
}
