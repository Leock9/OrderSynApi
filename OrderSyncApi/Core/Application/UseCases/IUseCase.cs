using FluentValidation.Results;

namespace OrderSyncApi.Core.Application.UseCases;

public interface IUseCase<TInput, TOutput>
{
    Task<TOutput> HandleAsync(TInput input, CancellationToken cancellationToken);
}