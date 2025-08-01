namespace Contracts.V1.Requests.Transactions;

public class TransferRequest
{
    public required Guid ToUserId { get; set; }
    public required decimal Amount { get; set; }
}
