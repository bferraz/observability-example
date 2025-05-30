using Carter;
using Microsoft.EntityFrameworkCore;
using Softdesign.CoP.Observability.Order.Infrastructure;
using Softdesign.CoP.Observability.Order.Service;
using Softdesign.CoP.Observability.Order.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VoucherRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VoucherService>();
builder.Services.AddCarter();

var app = builder.Build();

// Pré-cadastro de produtos se não houver nenhum
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    if (!db.Products.Any())
    {
        var products = new List<Product>
        {
            new Product { Id = Guid.Parse("3ef6f085-d567-4ba4-9368-e320a2b923a7"), Name = "Mouse", Description = "Mouse óptico USB", Value = 50, QtdStock = 1 },
            new Product { Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), Name = "Monitor", Description = "Monitor 24'' Full HD", Value = 1250, QtdStock = 1 },
            new Product { Id = Guid.Parse("eef8e519-7b44-49fc-bf79-30729ce1fa1e"), Name = "Pentes de Memória", Description = "Kit 2x8GB DDR4", Value = 870, QtdStock = 2 }
        };
        db.Products.AddRange(products);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Order API");
    });
}

app.MapCarter();

app.Run();