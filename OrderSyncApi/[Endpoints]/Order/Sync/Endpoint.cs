using System.Net;
using FastEndpoints;
using FluentValidation;
using OrderSyncApi.Core.Application.UseCases.OrderSync;

namespace OrderSyncApi._Endpoints_.Order.Sync;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    public IOrderSyncUseCase _orderSyncUseCase { get; set; }

    public override void Configure()
    {
        Post("/order/sync");
        Options(o => o.WithName("OrderSyncEndpoint"));
        AllowFileUploads();
        
        Throttle(
            hitLimit: 120,
            durationSeconds: 60
        );
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        try
        {
            DateOnly? startDate = DateOnly.TryParse(r.StartDate, out var parsedStartDate) ? parsedStartDate : null;
            DateOnly? endDate = DateOnly.TryParse(r.EndDate, out var parsedEndDate) ? parsedEndDate : null;

            var response = await _orderSyncUseCase.HandleAsync
            (
                new OrderSyncInput(r.File!, r.OrderId, startDate, endDate),
                c);
            await SendOkAsync(new Response { OrderSyncOutput = response });
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