using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrderSyncApi.Core.Application.UseCases.GetFile;
using OrderSyncApi.Core.Domain.Entity;
using OrderSyncApi.Infrastructure.Gateways.Redis;
using OrderSyncApi.Infrastructure.Services;

namespace OrderSyncApi.Tests.UseCases;

public class GetFileUseCaseTests
{
    private readonly IFileRecodGateway _fileRecordGateway;
    private readonly ILogger<GetFileUseCase> _logger;
    private readonly IBatchService _batchService;
    private readonly GetFileUseCase _getFileUseCase;

    public GetFileUseCaseTests()
    {
        _fileRecordGateway = A.Fake<IFileRecodGateway>();
        _logger = A.Fake<ILogger<GetFileUseCase>>();
        _batchService = A.Fake<IBatchService>();
        _getFileUseCase = new GetFileUseCase(_fileRecordGateway, _logger, _batchService);
    }

    [Fact]
    public async Task HandleAsync_FileNotFound_ThrowsApplicationException()
    {
        // Arrange
        var input = new GetFileInput("test.txt", null, null, null);
        A.CallTo(() => _fileRecordGateway.GetFileAsync("test.txt")).Returns((null, null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() => _getFileUseCase.HandleAsync(input, CancellationToken.None));
        Assert.Equal("Not found file record for test.txt", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_FileFoundWithoutFilters_ReturnsUsers()
    {
        // Arrange
        var input = new GetFileInput("test.txt", null, null, null);
        var users = new List<User> { new User(1, "John Doe", []) };
        A.CallTo(() => _fileRecordGateway.GetFileAsync("test.txt")).Returns(("test.txt", users));

        // Act
        var result = await _getFileUseCase.HandleAsync(input, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(users, result.Users);
    }

    [Fact]
    public async Task HandleAsync_FileFoundWithFilters_AppliesFiltersAndReturnsUsers()
    {
        // Arrange
        var input = new GetFileInput("test.txt", 1, DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), DateOnly.FromDateTime(DateTime.Now));
        var users = new List<User> { new User(1, "John Doe", []) };
        A.CallTo(() => _fileRecordGateway.GetFileAsync("test.txt")).Returns(("test.txt", users));
        A.CallTo(() => _batchService.ApplyFiltersAsync(users, 1, input.StartDate, input.EndDate, A<CancellationToken>.Ignored)).Returns(users);

        // Act
        var result = await _getFileUseCase.HandleAsync(input, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(users, result.Users);
    }
}