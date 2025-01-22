namespace OrderSyncApi.Core.Application.UseCases.GetFile;

public record GetFileInput(string FileName, int? OrderId = null, DateOnly? StartDate = null, DateOnly? EndDate = null);
