using Carter;
using Serilog;
using Softdesign.CoP.Observability.Basket.Service;
using System.Diagnostics;
using System.Text.Json;
using Softdesign.CoP.Observability.Basket.Helpers;

namespace Softdesign.CoP.Observability.Basket.Endpoints
{
    public class BasketEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/basket", async (Domain.Basket basket, BasketService service) =>
            {
                var activity = Activity.Current;
                activity.SetTagSafe("request.body", JsonSerializer.Serialize(basket));
                basket.Id = basket.Id == Guid.Empty ? Guid.NewGuid() : basket.Id;
                await service.InsertOrUpdateAsync(basket);
                activity.SetTagSafe("response.body", JsonSerializer.Serialize(basket));
                return Results.Ok(basket);
            })
            .WithName("CreateBasket")
            .WithSummary("Cria um novo basket.")
            .WithDescription("Cria um novo basket com uma lista de itens.")
            .Produces(StatusCodes.Status200OK)
            .Accepts<Domain.Basket>("application/json");

            app.MapPut("/basket", async (Domain.Basket basket, BasketService service) =>
            {
                var activity = Activity.Current;
                activity.SetTagSafe("request.body", JsonSerializer.Serialize(basket));
                await service.InsertOrUpdateAsync(basket);
                activity.SetTagSafe("response.status", "200");
                return Results.Ok();
            })
            .WithName("UpdateBasket")
            .WithSummary("Atualiza um basket existente.")
            .WithDescription("Atualiza um basket existente pelo Id.")
            .Produces(StatusCodes.Status200OK)
            .Accepts<Domain.Basket>("application/json");

            app.MapGet("/basket/{id}", async (Guid id, BasketService service) =>
            {
                var activity = Activity.Current;
                activity.SetTagSafe("request.id", id.ToString());
                Log.Information("API Basket iniciada e Serilog configurado para Loki.");
                var basket = await service.GetBasketAsync(id);
                activity.SetTagSafe("response.body", JsonSerializer.Serialize(basket));
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
                var activity = Activity.Current;
                activity.SetTagSafe("request.id", id.ToString());
                await service.DeleteAsync(id);
                activity.SetTagSafe("response.status", "204");
                return Results.NoContent();
            })
            .WithName("DeleteBasket")
            .WithSummary("Remove um basket pelo Id.")
            .WithDescription("Remove um basket do sistema pelo seu identificador.")
            .Produces(StatusCodes.Status204NoContent);
        }
    }
}
