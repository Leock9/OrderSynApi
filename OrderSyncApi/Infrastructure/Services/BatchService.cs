using System.Collections.Concurrent;
using OrderSyncApi.Core.Domain.Entity;

namespace OrderSyncApi.Infrastructure.Services;

public interface IBatchService
{
    Task<IList<User>> ProcessFileAsync(string filePath, int batchSize, CancellationToken cancellationToken);

    Task<IList<User>> ApplyFiltersAsync(
        IList<User> users,
        int? orderId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken);
}

public class BatchService(ILineParser lineParser, ILogger<BatchService> logger) : IBatchService
{
    public async Task<IList<User>> ProcessFileAsync(string filePath, int batchSize, CancellationToken cancellationToken)
    {
        var users = new ConcurrentDictionary<int, User>();
        var batches = SplitFileIntoBatches(filePath, batchSize);

        logger.LogInformation("Iniciando o processamento do arquivo: {FilePath} em lotes de {BatchSize}", filePath,
            batchSize);

        await Parallel.ForEachAsync(batches, new ParallelOptions
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cancellationToken
        }, async (batch, ct) =>
        {
            foreach (var line in batch)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var (isValid, parsedLine, errorMessage) = lineParser.Parse(line);
                    if (!isValid)
                    {
                        logger.LogWarning("Linha inválida detectada: {Line}. Detalhes: {ErrorMessage}", line,
                            errorMessage);
                        continue;
                    }

                    users.AddOrUpdate(parsedLine.UserId,
                        new User(parsedLine.UserId, parsedLine.UserName, [
                            new Order(parsedLine.OrderId, parsedLine.Date,
                                [new Product(parsedLine.ProductId, parsedLine.Value)])
                        ]),
                        (key, existingUser) =>
                        {
                            var order = existingUser.Orders.FirstOrDefault(o => o.OrderId == parsedLine.OrderId);
                            if (order == null)
                            {
                                order = new Order(parsedLine.OrderId, parsedLine.Date, new List<Product>());
                                existingUser.Orders.Add(order);
                            }

                            order.Products.Add(new Product(parsedLine.ProductId, parsedLine.Value));
                            return existingUser;
                        });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro inesperado ao processar linha: {Line}", line);
                }
            }

            await Task.CompletedTask;
        });

        logger.LogInformation("Processamento do arquivo concluído: {FilePath}", filePath);

        return users.Values.ToList();
    }

    private static IEnumerable<string[]> SplitFileIntoBatches(string filePath, int batchSize)
    {
        var lines = File.ReadLines(filePath);
        var batch = new List<string>();

        foreach (var line in lines.Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            batch.Add(line);
            if (batch.Count != batchSize) continue;
            yield return batch.ToArray();
            batch.Clear();
        }

        if (batch.Count != 0)
            yield return batch.ToArray();
    }

    public async Task<IList<User>> ApplyFiltersAsync(
        IList<User> users,
        int? orderId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        var filteredUsers = new ConcurrentBag<User>(); //thread-safe

        await Parallel.ForEachAsync(users, new ParallelOptions
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cancellationToken
        }, async (user, ct) =>
        {
            var filteredOrders = user.Orders.Where(order =>
            {
                var orderDateOnly = DateOnly.FromDateTime(order.Date);
                return (!orderId.HasValue || order.OrderId == orderId.Value) &&
                       (!startDate.HasValue || orderDateOnly >= startDate.Value) &&
                       (!endDate.HasValue || orderDateOnly <= endDate.Value);
            }).ToList();

            if (filteredOrders.Count != 0)
                filteredUsers.Add(new User(user.UserId, user.Name, filteredOrders));
        });

        return filteredUsers.ToList();
    }
}