namespace OrderSyncApi.Core.Domain.Entity;
public record Order(int OrderId, DateTime Date, List<Product> Products)
{
    public int OrderId { get; init; } = OrderId > 0 ? OrderId : throw new ArgumentException("OrderId must be greater than 0");
    public decimal Total => Products.Sum(p => p.Value);
    public DateTime Date { get; init; } = Date != default ? Date : throw new ArgumentException("Date must be a valid date");
    public List<Product> Products { get; init; } = Products ?? throw new ArgumentNullException(nameof(Products));
}