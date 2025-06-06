using Carter;
using OpenTelemetry.Trace;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Service;
using System.Text.Json;
using System.Diagnostics;
using Softdesign.CoP.Observability.Order.Helpers;

namespace Softdesign.CoP.Observability.Order.Endpoints
{
    public class ProductEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", async (ProductService service) =>
            {
                var activity = Activity.Current;
                var result = await service.GetAllAsync();
                activity.SetTagSafe("response.body", JsonSerializer.Serialize(result));
                return Results.Ok(result);
            })
            .WithName("ListProducts")
            .WithSummary("Lista todos os produtos.")
            .WithDescription("Retorna todos os produtos cadastrados no sistema.")
            .Produces<List<Product>>(StatusCodes.Status200OK, "application/json")
            .WithTags("Products");

            app.MapGet("/products/{id}", async (Guid id, ProductService service) =>
            {
                var activity = Activity.Current;
                activity.SetTagSafe("request.id", id.ToString());
                var product = await service.GetByIdAsync(id);
                activity.SetTagSafe("response.body", JsonSerializer.Serialize(product));
                return product is not null ? Results.Ok(product) : Results.NotFound();
            })
            .WithName("GetProduct")
            .WithSummary("Busca produto por Id.")
            .WithDescription("Retorna um produto específico pelo seu identificador.")
            .Produces<Product>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Products");

            app.MapPost("/products", async (Product product, ProductService service) =>
            {
                var activity = Activity.Current;
                activity.SetTagSafe("request.body", JsonSerializer.Serialize(product));
                if (product.Id == Guid.Empty)
                    product.Id = Guid.NewGuid();
                await service.AddAsync(product);
                activity.SetTagSafe("response.body", JsonSerializer.Serialize(product));
                return Results.Created($"/products/{product.Id}", product);
            })
            .WithName("CreateProduct")
            .WithSummary("Cria um novo produto.")
            .WithDescription("Cria um novo produto no sistema. O Id pode ser informado para integração com outros sistemas, ou será gerado automaticamente.")
            .Accepts<Product>("application/json")
            .Produces<Product>(StatusCodes.Status201Created, "application/json")
            .WithTags("Products");

            app.MapPut("/products/{id}", async (Guid id, Product product, ProductService service) =>
            {
                var activity = Activity.Current;
                activity.SetTagSafe("request.id", id.ToString());
                activity.SetTagSafe("request.body", JsonSerializer.Serialize(product));
                product.Id = id;
                await service.UpdateAsync(product);
                activity.SetTagSafe("response.body", JsonSerializer.Serialize(product));
                return Results.Ok(product);
            })
            .WithName("UpdateProduct")
            .WithSummary("Atualiza um produto.")
            .WithDescription("Atualiza os dados de um produto existente.")
            .Accepts<Product>("application/json")
            .Produces<Product>(StatusCodes.Status200OK, "application/json")
            .WithTags("Products");

            app.MapDelete("/products/{id}", async (Guid id, ProductService service) =>
            {
                var activity = Activity.Current;
                activity.SetTagSafe("request.id", id.ToString());
                await service.DeleteAsync(id);
                activity.SetTagSafe("response.status", "204");
                return Results.NoContent();
            })
            .WithName("DeleteProduct")
            .WithSummary("Remove um produto.")
            .WithDescription("Remove um produto existente pelo Id.")
            .Produces(StatusCodes.Status204NoContent)
            .WithTags("Products");
        }
    }
}
