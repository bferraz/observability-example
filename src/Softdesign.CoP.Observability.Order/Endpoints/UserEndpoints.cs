using Carter;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Service;

namespace Softdesign.CoP.Observability.Order.Endpoints
{
    public class UserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users", async (UserService service) => Results.Ok(await service.GetAllAsync()))
                .WithName("ListUsers")
                .WithSummary("Lista todos os usuários.")
                .WithDescription("Retorna todos os usuários cadastrados no sistema.")
                .Produces<List<User>>(StatusCodes.Status200OK, "application/json")
                .WithTags("Users");

            app.MapGet("/users/{id}", async (Guid id, UserService service) =>
            {
                var user = await service.GetByIdAsync(id);
                return user is not null ? Results.Ok(user) : Results.NotFound();
            })
            .WithName("GetUser")
            .WithSummary("Busca usuário por Id.")
            .WithDescription("Retorna um usuário específico pelo seu identificador.")
            .Produces<User>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");

            app.MapPost("/users", async (User user, UserService service) =>
            {
                user.Id = Guid.NewGuid();
                await service.AddAsync(user);
                return Results.Created($"/users/{user.Id}", user);
            })
            .WithName("CreateUser")
            .WithSummary("Cria um novo usuário.")
            .WithDescription("Cria um novo usuário no sistema. O Id é gerado automaticamente.")
            .Accepts<User>("application/json")
            .Produces<User>(StatusCodes.Status201Created, "application/json")
            .WithTags("Users");

            app.MapPut("/users/{id}", async (Guid id, User user, UserService service) =>
            {
                user.Id = id;
                await service.UpdateAsync(user);
                return Results.Ok(user);
            })
            .WithName("UpdateUser")
            .WithSummary("Atualiza um usuário.")
            .WithDescription("Atualiza os dados de um usuário existente.")
            .Accepts<User>("application/json")
            .Produces<User>(StatusCodes.Status200OK, "application/json")
            .WithTags("Users");

            app.MapDelete("/users/{id}", async (Guid id, UserService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteUser")
            .WithSummary("Remove um usuário.")
            .WithDescription("Remove um usuário do sistema pelo seu identificador.")
            .Produces(StatusCodes.Status204NoContent)
            .WithTags("Users");
        }
    }
}
