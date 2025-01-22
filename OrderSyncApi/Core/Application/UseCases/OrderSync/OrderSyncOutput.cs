using OrderSyncApi.Core.Domain.Entity;

namespace OrderSyncApi.Core.Application.UseCases.OrderSync;

public class OrderSyncOutput
{
    public IList<User> Users { get; init; } = null!;
}