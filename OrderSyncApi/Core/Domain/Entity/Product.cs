namespace OrderSyncApi.Core.Domain.Entity;
public record Product(int ProductId, decimal Value)
{
    public int ProductId { get; init; } = ProductId > 0 ? ProductId : throw new ArgumentException("ProductId must be greater than 0");
    public decimal Value { get; init; } = Value >= 0 ? Value : throw new ArgumentException("Value must be non-negative");
}