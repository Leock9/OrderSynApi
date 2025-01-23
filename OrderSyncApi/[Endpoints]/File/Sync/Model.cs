using FastEndpoints;
using FluentValidation;
using OrderSyncApi.Core.Application.UseCases.FileSync;

namespace OrderSyncApi._Endpoints_.Order.Sync;

internal sealed class Request
{
    public IFormFile? File { get; init; }
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.File)
                .NotEmpty()
                .WithMessage("O arquivo é obrigatório.");
        }
    }
}

internal sealed class Response
{
    public FileSyncOutput FileSyncOutput { get; init; }   
}