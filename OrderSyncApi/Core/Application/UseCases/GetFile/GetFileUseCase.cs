using OrderSyncApi.Infrastructure.Gateways.Redis;
using OrderSyncApi.Infrastructure.Services;

namespace OrderSyncApi.Core.Application.UseCases.GetFile;

public interface IGetFileUseCase : IUseCase<GetFileInput, GetFileOutput>
{
}

public class GetFileUseCase : IGetFileUseCase
{
    private readonly ILogger<GetFileUseCase> _logger;
    private readonly IFileRecodGateway _fileRecordGateway;
    private readonly IBatchService _batchService;

    public GetFileUseCase(IFileRecodGateway fileRecordGateway, ILogger<GetFileUseCase> logger, IBatchService batchService)
    {
        _fileRecordGateway = fileRecordGateway;
        _logger = logger;
        _batchService = batchService;
    }

    public async Task<GetFileOutput> HandleAsync(GetFileInput input, CancellationToken cancellationToken)
    {
        var (fileRecord, usersRecod) = await _fileRecordGateway.GetFileAsync(input.FileName);
        
        if(fileRecord == null)
        {
            var message = $"Not found file record for {input.FileName}";
            _logger.LogWarning(message);
            throw new ApplicationException(message);
        }
        
        if (InputHasFilters(input))
            usersRecod = await _batchService.ApplyFiltersAsync(usersRecod, input.OrderId, input.StartDate, input.EndDate,
                cancellationToken);
        
        return new GetFileOutput { Users = usersRecod };
    }
    
    private static bool InputHasFilters(GetFileInput input)
    {
        return input.OrderId.HasValue || input.StartDate.HasValue || input.EndDate.HasValue;
    }
}