namespace OrderSyncApi.Core.Application.UseCases.FileSync;

public record FileSyncOutput(bool Success, string? Status = null);