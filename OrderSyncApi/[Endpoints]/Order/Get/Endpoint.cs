using System.Net;
using FastEndpoints;
using FluentValidation;
using OrderSyncApi._Endpoints_.Order.Sync;
using OrderSyncApi.Core.Application.UseCases.GetFile;

namespace OrderSyncApi._Endpoints_.Order.Get;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    public IGetFileUseCase _getFileUseCase { get; set; }

    public override void Configure()
    {
        Get("/file");
        Options(o => o.WithName("GetFileEndpoint"));
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        try
        {
            DateOnly? startDate = DateOnly.TryParse(r.StartDate, out var parsedStartDate) ? parsedStartDate : null;
            DateOnly? endDate = DateOnly.TryParse(r.EndDate, out var parsedEndDate) ? parsedEndDate : null;

            var response = await _getFileUseCase.HandleAsync
            (
                new GetFileInput(r.FileName!, r.OrderId, startDate, endDate),
                c);
            await SendOkAsync(new Response { GetFileOutput = response });
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