using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderSyncApi.Core.Application.UseCases.OrderSync;
using OrderSyncApi.Core.Domain.Entity;
using OrderSyncApi.Infrastructure.Gateways.Redis;
using OrderSyncApi.Infrastructure.Services;

namespace OrderSyncApi.Tests.UseCases;

public class OrderSyncUseCaseTests
{
    private readonly IFormFile _fakeFile;
    private readonly OrderSyncUseCase _orderSyncUseCase;
    private readonly IBatchService _mockBatchService;
    private readonly IFileRecodGateway _fileRecodGateway;
    private readonly ILogger<OrderSyncUseCase> _logger;

    public OrderSyncUseCaseTests()
    {
        _fakeFile = A.Fake<IFormFile>();
        _mockBatchService = A.Fake<IBatchService>();
        _fileRecodGateway = A.Fake<IFileRecodGateway>();
        _logger = A.Fake<ILogger<OrderSyncUseCase>>();
        _orderSyncUseCase = new OrderSyncUseCase(_mockBatchService, _fileRecodGateway, _logger);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCachedDataFromRedis()
    {
        // Arrange
        var fileName = "test_cached.txt";
        var expectedUsers = new List<User>
        {
            new(70, "Cached User", new List<Order>
            {
                new(753, DateTime.Parse("2021-03-08"), new List<Product>
                {
                    new(3, 1836.74m)
                })
            })
        };

        A.CallTo(() => _fileRecodGateway.GetFileAsync(A<string>._))
            .Returns((fileName, expectedUsers));

        var input = new OrderSyncInput(_fakeFile);

        // Act
        var result = await _orderSyncUseCase.HandleAsync(input, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Users);
        Assert.Equal(70, result.Users[0].UserId);
        Assert.Equal("Cached User", result.Users[0].Name);
    }

    [Fact]
    public async Task HandleAsync_ShouldProcessFileWhenRedisHasNoData()
    {
        // Arrange
        var content = "0000000070                              Palmer Prosacco00000007530000000003     1836.7420210308";
        var fileName = "test_not_cached.txt";

        SetupFileMock(content, fileName);

        A.CallTo(() => _fileRecodGateway.GetFileAsync(A<string>._))
            .Returns((null, null));

        var expectedUsers = new List<User>
        {
            new(70, "Palmer Prosacco", new List<Order>
            {
                new(753, DateTime.Parse("2021-03-08"), new List<Product>
                {
                    new(3, 1836.74m)
                })
            })
        };

        A.CallTo(() => _mockBatchService.ProcessFileAsync(A<string>._, A<int>._, A<CancellationToken>._))
            .Returns(expectedUsers);

        var input = new OrderSyncInput(_fakeFile);

        // Act
        var result = await _orderSyncUseCase.HandleAsync(input, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Users);
        Assert.Equal(70, result.Users[0].UserId);
        Assert.Equal("Palmer Prosacco", result.Users[0].Name);
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyFiltersForRedisData()
    {
        // Arrange
        var fileName = "test_with_filters.txt";
        var cachedUsers = new List<User>
        {
            new(70, "Cached User", new List<Order>
            {
                new(753, DateTime.Parse("2021-03-08"), new List<Product>
                {
                    new(3, 1836.74m)
                })
            })
        };

        var filteredUsers = new List<User>
        {
            new(70, "Filtered User", new List<Order>
            {
                new(753, DateTime.Parse("2021-03-08"), new List<Product>
                {
                    new(3, 1000.00m)
                })
            })
        };

        A.CallTo(() => _fileRecodGateway.GetFileAsync(A<string>._))
            .Returns((fileName, cachedUsers));

        A.CallTo(() =>
                _mockBatchService.ApplyFiltersAsync(cachedUsers, A<int?>._, A<DateOnly?>._, A<DateOnly?>._,
                    A<CancellationToken>._))
            .Returns(filteredUsers);

        var input = new OrderSyncInput(_fakeFile)
        {
            OrderId = 753
        };

        // Act
        var result = await _orderSyncUseCase.HandleAsync(input, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Users);
        Assert.Equal("Filtered User", result.Users[0].Name);
    }

    // Helper method to set up IFormFile mocks
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
}