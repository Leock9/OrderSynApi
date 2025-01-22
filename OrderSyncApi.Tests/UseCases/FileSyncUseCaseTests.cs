using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderSyncApi.Core.Application.UseCases.FileSync;
using OrderSyncApi.Core.Domain.Entity;
using OrderSyncApi.Infrastructure.Gateways.Redis;
using OrderSyncApi.Infrastructure.Services;

namespace OrderSyncApi.Tests.UseCases;

public class FileSyncUseCaseTests
{
    private readonly IFormFile _fakeFile;
    private readonly FileSyncUseCase _fileSyncUseCase;
    private readonly IBatchService _mockBatchService;
    private readonly IFileRecodGateway _fileRecodGateway;
    private readonly ILogger<FileSyncUseCase> _logger;

    public FileSyncUseCaseTests()
    {
        _fakeFile = A.Fake<IFormFile>();
        _mockBatchService = A.Fake<IBatchService>();
        _fileRecodGateway = A.Fake<IFileRecodGateway>();
        _logger = A.Fake<ILogger<FileSyncUseCase>>();
        _fileSyncUseCase = new FileSyncUseCase(_mockBatchService, _fileRecodGateway, _logger);
    }

    private void SetupFileMock(string content, string fileName)
    {
        var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, leaveOpen: true))
        {
            writer.Write(content);
            writer.Flush();
        }

        ms.Position = 0;

        A.CallTo(() => _fakeFile.OpenReadStream()).Returns(ms);
        A.CallTo(() => _fakeFile.FileName).Returns(fileName);
        A.CallTo(() => _fakeFile.Length).Returns(ms.Length);
    }

    [Fact]
    public async Task HandleAsync_FileAlreadyProcessed_ReturnsFileAlreadyProcessedMessage()
    {
        SetupFileMock("file content", "test.txt");
        var input = new FileSyncInput(_fakeFile);
        A.CallTo(() => _fileRecodGateway.GetFileAsync(A<string>.Ignored)).Returns(("test.txt", new List<User>()));

        var result = await _fileSyncUseCase.HandleAsync(input, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("File already processed", result.Status);
    }

    [Fact]
    public async Task HandleAsync_ProcessFileSuccessfully_ReturnsSuccess()
    {
        SetupFileMock("file content", "test.txt");
        var input = new FileSyncInput(_fakeFile);
        A.CallTo(() => _fileRecodGateway.GetFileAsync(A<string>.Ignored)).Returns((null, null));
        A.CallTo(() => _mockBatchService.ProcessFileAsync(A<string>.Ignored, A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<User>());

        var result = await _fileSyncUseCase.HandleAsync(input, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.Status);
        A.CallTo(() => _fileRecodGateway.AddFileAsync("test", A<IList<User>>.Ignored)).MustHaveHappened();
    }

    [Fact]
    public async Task HandleAsync_ErrorProcessingFile_ThrowsException()
    {
        SetupFileMock("file content", "test.txt");
        var input = new FileSyncInput(_fakeFile);
        A.CallTo(() => _fileRecodGateway.GetFileAsync(A<string>.Ignored)).Returns((null, null));
        A.CallTo(() => _mockBatchService.ProcessFileAsync(A<string>.Ignored, A<int>.Ignored, A<CancellationToken>.Ignored))
            .Throws<Exception>();

        await Assert.ThrowsAsync<Exception>(() => _fileSyncUseCase.HandleAsync(input, CancellationToken.None));
    }
}