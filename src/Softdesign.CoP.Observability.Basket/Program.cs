using Carter;
using Softdesign.CoP.Observability.Basket.Domain;
using Softdesign.CoP.Observability.Basket.Infrastructure;
using Softdesign.CoP.Observability.Basket.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCarter();

// Configuração do Redis
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
builder.Services.AddSingleton<IRedisConnectionFactory>(sp => new RedisConnectionFactory(redisConnectionString));
builder.Services.AddScoped<BasketRepository>();
builder.Services.AddScoped<BasketService>();

var app = builder.Build();

// Pré-cadastro de itens no Redis com os mesmos Ids dos produtos de Order
using (var scope = app.Services.CreateScope())
{
    var basketService = scope.ServiceProvider.GetRequiredService<BasketService>();
    var existing = await basketService.ListAsync();
    if (existing == null || !existing.Any())
    {
        await basketService.InsertOrUpdateAsync(new BasketItem { ProductId = Guid.Parse("3ef6f085-d567-4ba4-9368-e320a2b923a7"), ProductName = "Mouse", Quantity = 1, Value = 50 });
        await basketService.InsertOrUpdateAsync(new BasketItem { ProductId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), ProductName = "Monitor", Quantity = 1, Value = 1250 });
        await basketService.InsertOrUpdateAsync(new BasketItem { ProductId = Guid.Parse("eef8e519-7b44-49fc-bf79-30729ce1fa1e"), ProductName = "Pentes de Memória", Quantity = 2, Value = 870 });
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Basket API");
    });
}

app.MapCarter();

app.Run();
