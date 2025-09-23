using Carter;
using Softdesign.CoP.Observability.Catalog.Domain;
using Softdesign.CoP.Observability.Catalog.Service;
using System.Text.Json;
using System.Diagnostics;
using CorrelationId.Abstractions;

namespace Softdesign.CoP.Observability.Catalog.Endpoints
{
    public class UserEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users", async (UserService service, ICorrelationContextAccessor correlationContextAccessor) =>
            {
                // Adiciona Correlation ID ao tracing
                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
                Activity.Current?.SetTag("correlation_id", correlationId);

                var activity = Activity.Current;

                var result = await service.GetAllAsync();

                activity?.SetTag("response.body", JsonSerializer.Serialize(result));

                return Results.Ok(result);
            })
            .WithName("ListUsers")
            .WithSummary("Lista todos os usuários.")
            .WithDescription("Retorna todos os usuários cadastrados no sistema.")
            .Produces<List<User>>(StatusCodes.Status200OK, "application/json")
            .WithTags("Users");

            app.MapGet("/users/{id}", async (Guid id, UserService service, ICorrelationContextAccessor correlationContextAccessor) =>
            {
                // Adiciona Correlation ID ao tracing
                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
                Activity.Current?.SetTag("correlation_id", correlationId);

                var activity = Activity.Current;
                activity?.SetTag("request.id", id.ToString());

                var user = await service.GetByIdAsync(id);

                activity?.SetTag("response.body", JsonSerializer.Serialize(user));

                return user is not null ? Results.Ok(user) : Results.NotFound();
            })
            .WithName("GetUser")
            .WithSummary("Busca usuário por Id.")
            .WithDescription("Retorna um usuário específico pelo seu identificador.")
            .Produces<User>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");

            app.MapPost("/users", async (User user, UserService service, ICorrelationContextAccessor correlationContextAccessor) =>
            {
                // Adiciona Correlation ID ao tracing
                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
                Activity.Current?.SetTag("correlation_id", correlationId);

                var activity = Activity.Current;
                activity?.SetTag("request.body", JsonSerializer.Serialize(user));

                user.Id = Guid.NewGuid();
                await service.AddAsync(user);

                activity?.SetTag("response.body", JsonSerializer.Serialize(user));

                return Results.Created($"/users/{user.Id}", user);
            })
            .WithName("CreateUser")
            .WithSummary("Cria um novo usuário.")
            .WithDescription("Cria um novo usuário no sistema. O Id é gerado automaticamente.")
            .Accepts<User>("application/json")
            .Produces<User>(StatusCodes.Status201Created, "application/json")
            .WithTags("Users");

            app.MapPut("/users/{id}", async (Guid id, User user, UserService service, ICorrelationContextAccessor correlationContextAccessor) =>
            {
                // Adiciona Correlation ID ao tracing
                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
                Activity.Current?.SetTag("correlation_id", correlationId);

                var activity = Activity.Current;
                activity?.SetTag("request.id", id.ToString());
                activity?.SetTag("request.body", JsonSerializer.Serialize(user));

                user.Id = id;
                await service.UpdateAsync(user);

                activity?.SetTag("response.body", JsonSerializer.Serialize(user));

                return Results.Ok(user);
            })
            .WithName("UpdateUser")
            .WithSummary("Atualiza um usuário.")
            .WithDescription("Atualiza os dados de um usuário existente.")
            .Accepts<User>("application/json")
            .Produces<User>(StatusCodes.Status200OK, "application/json")
            .WithTags("Users");

            app.MapDelete("/users/{id}", async (Guid id, UserService service, ICorrelationContextAccessor correlationContextAccessor) =>
            {
                // Adiciona Correlation ID ao tracing
                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId ?? "unknown";
                Activity.Current?.SetTag("correlation_id", correlationId);

                var activity = Activity.Current;
                activity?.SetTag("request.id", id.ToString());

                await service.DeleteAsync(id);

                activity?.SetTag("response.status", "204");

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
