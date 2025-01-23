using FastEndpoints;
using FluentValidation;
using OrderSyncApi.Core.Application.UseCases.GetFile;

namespace OrderSyncApi._Endpoints_.Order.Get;

internal sealed class Request
{
    [QueryParam]
    public string FileName { get; init; }
    
    [QueryParam] 
    public int? OrderId { get; init; } = null;
    
    [QueryParam]
    public string? StartDate { get; init; } = null!;
    
    [QueryParam]
    public string? EndDate { get; init; } = null!;
    
    internal sealed class Validator : Validator<Get.Request>
    {
        public Validator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("O nome do arquivo é obrigatório.");
            
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
    public GetFileOutput GetFileOutput { get; init; }
}