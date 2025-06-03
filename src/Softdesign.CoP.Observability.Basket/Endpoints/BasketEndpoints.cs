using Carter;
using Softdesign.CoP.Observability.Basket.Service;

namespace Softdesign.CoP.Observability.Basket.Endpoints
{
    public class BasketEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/basket", async (Domain.Basket basket, BasketService service) =>
            {
                basket.Id = basket.Id == Guid.Empty ? Guid.NewGuid() : basket.Id;
                await service.InsertOrUpdateAsync(basket);
                return Results.Ok(basket);
            })
            .WithName("CreateBasket")
            .WithSummary("Cria um novo basket.")
            .WithDescription("Cria um novo basket com uma lista de itens.")
            .Produces(StatusCodes.Status200OK)
            .Accepts<Domain.Basket>("application/json");

            app.MapPut("/basket", async (Domain.Basket basket, BasketService service) =>
            {
                await service.InsertOrUpdateAsync(basket);
                return Results.Ok();
            })
            .WithName("UpdateBasket")
            .WithSummary("Atualiza um basket existente.")
            .WithDescription("Atualiza um basket existente pelo Id.")
            .Produces(StatusCodes.Status200OK)
            .Accepts<Domain.Basket>("application/json");

            app.MapGet("/basket/{id}", async (Guid id, BasketService service) =>
            {
                var basket = await service.GetBasketAsync(id);
                if (basket == null)
                    return Results.NotFound();
                return Results.Ok(basket);
            })
            .WithName("GetBasket")
            .WithSummary("Obtém o basket pelo Id.")
            .WithDescription("Retorna o basket cadastrado pelo Id.")
            .Produces<Domain.Basket>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound);

            app.MapDelete("/basket/{id}", async (Guid id, BasketService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteBasket")
            .WithSummary("Remove um basket pelo Id.")
            .WithDescription("Remove um basket do sistema pelo seu identificador.")
            .Produces(StatusCodes.Status204NoContent);
        }
    }
}
