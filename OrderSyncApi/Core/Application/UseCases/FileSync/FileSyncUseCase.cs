using OrderSyncApi.Core.Domain.Entity;
using OrderSyncApi.Infrastructure.Gateways.Redis;
using OrderSyncApi.Infrastructure.Services;

namespace OrderSyncApi.Core.Application.UseCases.FileSync;

public interface IFileSyncUseCase : IUseCase<FileSyncInput, FileSyncOutput>
{
}

public class FileSyncUseCase : IFileSyncUseCase
{
    private readonly ILogger<FileSyncUseCase> _logger;
    private readonly IBatchService _batchService;
    private readonly IFileRecodGateway _fileRecordGateway;

    public FileSyncUseCase
    (
            IBatchService batchService, 
            IFileRecodGateway fileRecordGateway, 
            ILogger<FileSyncUseCase> logger)
    {
        _batchService = batchService;
        _fileRecordGateway = fileRecordGateway;
        _logger = logger;
    }

    public async Task<FileSyncOutput> HandleAsync(FileSyncInput input, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileNameWithoutExtension(input.File.FileName);
        var (fileRecord, usersRecod) = await _fileRecordGateway.GetFileAsync(fileName);

        if (fileRecord != null && usersRecod != null)
            return new FileSyncOutput(false, "File already processed");
        else
        {
            _logger.LogWarning($"Processing file {fileName}");
            var users = await ProcessFileAsync(input, cancellationToken);
            await _fileRecordGateway.AddFileAsync(fileName, users);
        }
        
        return new FileSyncOutput(true);
    }

    private async Task<IList<User>> ProcessFileAsync(FileSyncInput input, CancellationToken cancellationToken)
    {
        var filePath = Path.GetTempFileName()!;
        await using (var stream = File.Create(filePath))
        {
            await input.File.CopyToAsync(stream, cancellationToken);
        }

        return await _batchService.ProcessFileAsync(filePath, 100, cancellationToken);
    }
}