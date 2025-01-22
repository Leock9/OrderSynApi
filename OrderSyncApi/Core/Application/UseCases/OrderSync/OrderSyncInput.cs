namespace OrderSyncApi.Core.Application.UseCases.OrderSync;

public record OrderSyncInput(IFormFile File, int? OrderId = null, DateOnly? StartDate = null, DateOnly? EndDate = null);