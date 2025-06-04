using Carter;
using Refit;
using Softdesign.CoP.Observability.Bff.Contracts.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCarter();

builder.Services.AddRefitClient<IBasketApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5027")); // ajuste a porta conforme necessário

builder.Services.AddRefitClient<IOrderApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5135")); // ajuste a porta conforme necessário

builder.Services.AddScoped<Softdesign.CoP.Observability.Bff.Services.IPurchaseService, Softdesign.CoP.Observability.Bff.Services.PurchaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "BFF API");
    });
}

app.MapCarter();

app.Run();
