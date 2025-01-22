using FakeItEasy;
using Microsoft.Extensions.Logging;
using OrderSyncApi.Infrastructure.Services;

namespace OrderSyncApi.Tests.Services;

public class BatchServiceTests
{
    private readonly ILineParser _lineParserFake;
    private readonly ILogger<BatchService> _loggerFake;
    private readonly IBatchService _batchService;

    public BatchServiceTests()
    {
        _lineParserFake = A.Fake<ILineParser>();
        _loggerFake = A.Fake<ILogger<BatchService>>();
        _batchService = new BatchService(_lineParserFake, _loggerFake);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldReturnProcessedUser_WhenValidDataIsProvided()
    {
        const string content = @"
1,User1,100,10,20,2025-01-01
        ";
        var batchSize = 2;
        var cancellationToken = CancellationToken.None;

        var filePath = Path.Combine(Path.GetTempPath(), "test.txt");

        await File.WriteAllTextAsync(filePath, content.Trim());

        A.CallTo(() => _lineParserFake.Parse(A<string>.Ignored))
            .ReturnsLazily((string line) =>
                (true, new ParsedLine(1, "User1", 100, 10, 20m, new DateTime(2025, 01, 01)), string.Empty));

        var result = await _batchService.ProcessFileAsync(filePath, batchSize, cancellationToken);

        Assert.Single(result);
        var user = result.First();
        Assert.Equal(1, user.UserId);
        Assert.Single(user.Orders);
        Assert.Single(user.Orders.First().Products);

        File.Delete(filePath);
    }
    
    [Fact]
    public async Task ProcessFileAsync_ShouldProcessMultipleUsersAndOrders_WhenValidDataIsProvided()
    {
        // Arrange
        const string content = @"
1,User1,100,10,20,2025-01-01
2,User2,200,20,40,2025-01-02
1,User1,100,30,60,2025-01-01
        ";
        var batchSize = 2;
        var cancellationToken = CancellationToken.None;
        var filePath = Path.Combine(Path.GetTempPath(), "test_multiple.txt");
        await File.WriteAllTextAsync(filePath, content.Trim());

        A.CallTo(() => _lineParserFake.Parse(A<string>.Ignored))
            .ReturnsLazily((string line) =>
            {
                var parts = line.Split(',');
                return (true, new ParsedLine(
                    int.Parse(parts[0]),
                    parts[1],
                    int.Parse(parts[2]),
                    int.Parse(parts[3]),
                    decimal.Parse(parts[4]),
                    DateTime.Parse(parts[5])
                ), string.Empty);
            });

        // Act
        var result = await _batchService.ProcessFileAsync(filePath, batchSize, cancellationToken);

        // Assert
        Assert.Equal(2, result.Count);

        var user1 = result.First(u => u.UserId == 1);
        Assert.Equal(1, user1.UserId);
        Assert.Equal(2, user1.Orders.First().Products.Count);

        var user2 = result.First(u => u.UserId == 2);
        Assert.Equal(1, user2.Orders.Count);
        Assert.Single(user2.Orders.First().Products);

        File.Delete(filePath);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleInvalidLineGracefully()
    {
        var content = @"
invalid,data,here";
        var batchSize = 2;
        var cancellationToken = CancellationToken.None;

        var filePath = Path.Combine(Path.GetTempPath(), "test.txt");

        await File.WriteAllTextAsync(filePath, content.Trim());

        A.CallTo(() => _lineParserFake.Parse(A<string>.Ignored))
            .ReturnsLazily((string line) =>
                (false, null, "Invalid data format"));

        var result = await _batchService.ProcessFileAsync(filePath, batchSize, cancellationToken);

        Assert.Empty(result); 

        File.Delete(filePath);
    }

    [Fact]
    public async Task ProcessFileAsync_ShouldHandleExceptionGracefully_WhenParsingFails()
    {
        var content = @"
1,User1,100,10,20,2025-01-01";
        var batchSize = 2;
        var cancellationToken = CancellationToken.None;

        var filePath = Path.Combine(Path.GetTempPath(), "test.txt");

        await File.WriteAllTextAsync(filePath, content.Trim());

        A.CallTo(() => _lineParserFake.Parse(A<string>.Ignored))
            .Throws(new System.Exception("Unexpected error"));

        var result = await _batchService.ProcessFileAsync(filePath, batchSize, cancellationToken);

        Assert.Empty(result); 

        File.Delete(filePath);
    }

}