using OrderSyncApi.Core.Domain.Entity;
using OrderSyncApi.Infrastructure.Gateways.Redis;
using OrderSyncApi.Infrastructure.Services;

namespace OrderSyncApi.Core.Application.UseCases.OrderSync;

public interface IOrderSyncUseCase : IUseCase<OrderSyncInput, OrderSyncOutput>
{
}

public class OrderSyncUseCase : IOrderSyncUseCase
{
    private readonly ILogger<OrderSyncUseCase> _logger;
    private readonly IBatchService _batchService;
    private readonly IFileRecodGateway _fileRecodGateway;

    public OrderSyncUseCase
    (
            IBatchService batchService, 
            IFileRecodGateway fileRecodGateway, 
            ILogger<OrderSyncUseCase> logger)
    {
        _batchService = batchService;
        _fileRecodGateway = fileRecodGateway;
        _logger = logger;
    }

    public async Task<OrderSyncOutput> HandleAsync(OrderSyncInput input, CancellationToken cancellationToken)
    {
        IList<User> users;
        var fileName = Path.GetFileNameWithoutExtension(input.File.FileName);
        var (fileRecord, usersRecod) = await _fileRecodGateway.GetFileAsync(fileName);

        if (fileRecord != null && usersRecod != null)
        {
            _logger.LogWarning($"Returning cached data for file {fileRecord}");
            users = usersRecod!;
        }
        else
        {
            _logger.LogWarning($"Processing file {fileName}");
            users = await ProcessAndFilterFileAsync(input, cancellationToken);
            await _fileRecodGateway.AddFileAsync(fileName, users);
        }
        
        if (InputHasFilters(input))
            users = await _batchService.ApplyFiltersAsync(users, input.OrderId, input.StartDate, input.EndDate,
                cancellationToken);
        
        return new OrderSyncOutput { Users = users };
    }

    private async Task<IList<User>> ProcessAndFilterFileAsync(OrderSyncInput input, CancellationToken cancellationToken)
    {
        var filePath = Path.GetTempFileName()!;
        await using (var stream = File.Create(filePath))
        {
            await input.File.CopyToAsync(stream, cancellationToken);
        }

        return await _batchService.ProcessFileAsync(filePath, 100, cancellationToken);
    }

    private static bool InputHasFilters(OrderSyncInput input)
    {
        return input.OrderId.HasValue || input.StartDate.HasValue || input.EndDate.HasValue;
    }
}