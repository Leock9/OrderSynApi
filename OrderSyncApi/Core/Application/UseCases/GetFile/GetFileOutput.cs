using OrderSyncApi.Core.Domain.Entity;

namespace OrderSyncApi.Core.Application.UseCases.GetFile;

public class GetFileOutput
{
    public IList<User> Users { get; init; } = null!;
}