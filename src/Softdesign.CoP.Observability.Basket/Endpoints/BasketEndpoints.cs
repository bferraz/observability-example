using Carter;
using Softdesign.CoP.Observability.Basket.Domain;
using Softdesign.CoP.Observability.Basket.Service;

namespace Softdesign.CoP.Observability.Basket.Endpoints
{
    public class BasketEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/basket", async (BasketItem item, BasketService service) =>
            {
                item.ProductId = Guid.NewGuid();
                await service.InsertOrUpdateAsync(item);
                return Results.Ok(item);
            })
            .WithName("CreateBasketItem")
            .WithSummary("Cria um novo item no basket.")
            .WithDescription("Cria um novo item no basket. O ProductId será gerado automaticamente pelo backend.")
            .Produces(StatusCodes.Status200OK)
            .Accepts<BasketItem>("application/json");

            app.MapPut("/basket", async (BasketItem item, BasketService service) =>
            {
                await service.InsertOrUpdateAsync(item);
                return Results.Ok();
            })
            .WithName("UpdateBasketItem")
            .WithSummary("Atualiza um item existente no basket.")
            .WithDescription("Atualiza um item existente no basket pelo ProductId.")
            .Produces(StatusCodes.Status200OK)
            .Accepts<BasketItem>("application/json");

            app.MapGet("/basket", async (BasketService service) =>
            {
                var items = await service.ListAsync();
                return Results.Ok(items);
            })
            .WithName("ListBasketItems")
            .WithSummary("Lista todos os itens do basket.")
            .WithDescription("Retorna todos os itens cadastrados no basket.")
            .Produces<List<BasketItem>>(StatusCodes.Status200OK, "application/json");
        }
    }
}
