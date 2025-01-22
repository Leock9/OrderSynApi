using System.Text.Json;
using OrderSyncApi.Core.Domain.Entity;
using StackExchange.Redis;

namespace OrderSyncApi.Infrastructure.Gateways.Redis;

public interface IFileRecodGateway
{
    Task AddFileAsync(string fileName, IList<User> user);
    Task<(string?, IList<User>?)> GetFileAsync(string fileName);
}

public class FileRecodGateway(IConnectionMultiplexer redis) : IFileRecodGateway
{
    public async Task AddFileAsync(string fileName, IList<User> user)
    {
        var db = redis.GetDatabase();
        var serializedUser = JsonSerializer.Serialize(user);
        await db.StringSetAsync(fileName, serializedUser);
    }

    public async Task<(string?, IList<User>?)> GetFileAsync(string fileName)
    {
        var db = redis.GetDatabase();
        var serializedFile = await db.StringGetAsync(fileName);

        if (string.IsNullOrEmpty(serializedFile))
            return (null, null);

        var users = JsonSerializer.Deserialize<IList<User>>(serializedFile!);
        return (fileName, users);
    }
}