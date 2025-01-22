using FastEndpoints;
using FluentValidation;
using OrderSyncApi.Core.Application.UseCases.OrderSync;

namespace OrderSyncApi._Endpoints_.Order.Sync;

internal sealed class Request
{
    [QueryParam] 
    public int? OrderId { get; init; } = null;
    
    [QueryParam]
    public string? StartDate { get; init; } = null!;
    
    [QueryParam]
    public string? EndDate { get; init; } = null!;
    
    public IFormFile? File { get; init; }
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.File)
                .NotEmpty()
                .WithMessage("O arquivo é obrigatório.");

            RuleFor(x => x.StartDate)
                .Custom((value, context) =>
                {
                    if (!string.IsNullOrEmpty(value) && !DateOnly.TryParse(value, out _))
                    {
                        context.AddFailure("StartDate", "A data inicial fornecida é inválida.");
                    }
                });

            RuleFor(x => x.EndDate)
                .Custom((value, context) =>
                {
                    if (!string.IsNullOrEmpty(value) && !DateOnly.TryParse(value, out _))
                    {
                        context.AddFailure("EndDate", "A data final fornecida é inválida.");
                    }
                });

            RuleFor(x => x)
                .Custom((req, context) =>
                {
                    if (DateOnly.TryParse(req.StartDate, out var startDate) &&
                        DateOnly.TryParse(req.EndDate, out var endDate) &&
                        startDate > endDate)
                    {
                        context.AddFailure("DateRange", "A data inicial não pode ser maior que a data final.");
                    }
                });
        }
    }
}

internal sealed class Response
{
    public OrderSyncOutput OrderSyncOutput { get; init; }
}