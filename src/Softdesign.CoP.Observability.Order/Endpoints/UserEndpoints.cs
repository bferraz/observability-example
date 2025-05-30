using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Service;

namespace Softdesign.CoP.Observability.Order.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            app.MapGet("/users", async (UserService service) => Results.Ok(await service.GetAllAsync()))
                .WithName("ListUsers").WithSummary("Lista todos os usuários");

            app.MapGet("/users/{id}", async (Guid id, UserService service) =>
            {
                var user = await service.GetByIdAsync(id);
                return user is not null ? Results.Ok(user) : Results.NotFound();
            }).WithName("GetUser").WithSummary("Busca usuário por Id");

            app.MapPost("/users", async (User user, UserService service) =>
            {
                user.Id = Guid.NewGuid();
                await service.AddAsync(user);
                return Results.Created($"/users/{user.Id}", user);
            }).WithName("CreateUser").WithSummary("Cria um novo usuário");

            app.MapPut("/users/{id}", async (Guid id, User user, UserService service) =>
            {
                user.Id = id;
                await service.UpdateAsync(user);
                return Results.Ok(user);
            }).WithName("UpdateUser").WithSummary("Atualiza um usuário");

            app.MapDelete("/users/{id}", async (Guid id, UserService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }).WithName("DeleteUser").WithSummary("Remove um usuário");
        }
    }
}
