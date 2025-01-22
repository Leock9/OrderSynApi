using FastEndpoints;
using FastEndpoints.Swagger;
using OrderSyncApi.Core.Application.UseCases.FileSync;
using OrderSyncApi.Core.Application.UseCases.GetFile;
using OrderSyncApi.Infrastructure.Gateways.Redis;
using OrderSyncApi.Infrastructure.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFastEndpoints();

builder.Services.SwaggerDocument(o =>
{
    o.EnableJWTBearerAuth = false;
    o.DocumentSettings = s =>
    {
        s.Title = "Order Sync API";
        s.Version = "v1";
        s.Description = "Order Sync API Documentation";
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Register services
builder.Services.AddScoped<IFileSyncUseCase, FileSyncUseCase>();
builder.Services.AddScoped<IGetFileUseCase, GetFileUseCase>();

builder.Services.AddScoped<ILineParser, LineParser>();
builder.Services.AddScoped<IBatchService, BatchService>();

// Register gateways
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IFileRecodGateway, FileRecodGateway>();

var app = builder.Build();
app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep =>
    {
        ep.AllowAnonymous();
        ep.Summary(s =>
        {
            s.Responses[200] = "Success.";
            s.Responses[400] = "Validation error.";
            s.Responses[401] = "Unauthorized.";
            s.Responses[403] = "Forbidden.";
            s.Responses[404] = "Not found.";
            s.Responses[500] = "Internal server error.";
        });
    };
});

app.UseSwaggerGen();
app.Run();