using Carter;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Service;

namespace Softdesign.CoP.Observability.Order.Endpoints
{
    public class ProductEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", async (ProductService service) => Results.Ok(await service.GetAllAsync()))
                .WithName("ListProducts")
                .WithSummary("Lista todos os produtos.")
                .WithDescription("Retorna todos os produtos cadastrados no sistema.")
                .Produces<List<Product>>(StatusCodes.Status200OK, "application/json");

            app.MapGet("/products/{id}", async (Guid id, ProductService service) =>
            {
                var product = await service.GetByIdAsync(id);
                return product is not null ? Results.Ok(product) : Results.NotFound();
            })
            .WithName("GetProduct")
            .WithSummary("Busca produto por Id.")
            .WithDescription("Retorna um produto específico pelo seu identificador.")
            .Produces<Product>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound);

            app.MapPost("/products", async (Product product, ProductService service) =>
            {
                if (product.Id == Guid.Empty)
                    product.Id = Guid.NewGuid();
                await service.AddAsync(product);
                return Results.Created($"/products/{product.Id}", product);
            })
            .WithName("CreateProduct")
            .WithSummary("Cria um novo produto.")
            .WithDescription("Cria um novo produto no sistema. O Id pode ser informado para integração com outros sistemas, ou será gerado automaticamente.")
            .Accepts<Product>("application/json")
            .Produces<Product>(StatusCodes.Status201Created, "application/json");

            app.MapPut("/products/{id}", async (Guid id, Product product, ProductService service) =>
            {
                product.Id = id;
                await service.UpdateAsync(product);
                return Results.Ok(product);
            })
            .WithName("UpdateProduct")
            .WithSummary("Atualiza um produto.")
            .WithDescription("Atualiza os dados de um produto existente.")
            .Accepts<Product>("application/json")
            .Produces<Product>(StatusCodes.Status200OK, "application/json");

            app.MapDelete("/products/{id}", async (Guid id, ProductService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteProduct")
            .WithSummary("Remove um produto.")
            .WithDescription("Remove um produto do sistema pelo seu identificador.")
            .Produces(StatusCodes.Status204NoContent);
        }
    }
}
