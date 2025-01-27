using System.Net;
using FastEndpoints;
using FluentValidation;
using OrderSyncApi.Core.Application.UseCases.FileSync;

namespace OrderSyncApi._Endpoints_.Order.Sync;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    public IFileSyncUseCase _FileSyncUseCase { get; set; }

    public override void Configure()
    {
        Post("/file/sync");
        Options(o => o.WithName("FileSyncEndpoint"));
        AllowFileUploads();
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        try
        {
            var response = await _FileSyncUseCase.HandleAsync
            (
                new FileSyncInput(r.File!),
                c);
            await SendOkAsync(new Response { FileSyncOutput = response });
        }
        catch (ValidationException vx)
        {
            AddError(vx.Message);
            await SendErrorsAsync((int)HttpStatusCode.BadRequest, c);
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            };
            await SendResultAsync(problemDetails);
        }
    }
}