namespace OrderSyncApi.Core.Domain.Entity;
public record User(int UserId, string Name, List<Order> Orders)
{
    public int UserId { get; init; } = UserId > 0 ? UserId : throw new ArgumentException("UserId must be greater than 0");
    public string Name { get; init; } = !string.IsNullOrWhiteSpace(Name) ? Name : throw new ArgumentException("Name cannot be null or empty");
    public List<Order> Orders { get; init; } = Orders ?? throw new ArgumentNullException(nameof(Orders));
}