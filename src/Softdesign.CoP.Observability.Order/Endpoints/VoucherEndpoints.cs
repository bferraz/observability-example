using Carter;
using Microsoft.AspNetCore.Http;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Service;

namespace Softdesign.CoP.Observability.Order.Endpoints
{
    public class VoucherEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/vouchers", async (VoucherService service) => Results.Ok(await service.GetAllAsync()))
                .WithName("ListVouchers")
                .WithSummary("Lista todos os vouchers.")
                .WithDescription("Retorna todos os vouchers cadastrados no sistema.")
                .Produces<List<Voucher>>(StatusCodes.Status200OK, "application/json")
                .WithTags("Vouchers");

            app.MapGet("/vouchers/{id}", async (Guid id, VoucherService service) =>
            {
                var voucher = await service.GetByIdAsync(id);
                return voucher is not null ? Results.Ok(voucher) : Results.NotFound();
            })
            .WithName("GetVoucher")
            .WithSummary("Busca voucher por Id.")
            .WithDescription("Retorna um voucher específico pelo seu identificador.")
            .Produces<Voucher>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Vouchers");

            app.MapPost("/vouchers", async (Voucher voucher, VoucherService service) =>
            {
                voucher.Id = Guid.NewGuid();
                await service.AddAsync(voucher);
                return Results.Created($"/vouchers/{voucher.Id}", voucher);
            })
            .WithName("CreateVoucher")
            .WithSummary("Cria um novo voucher.")
            .WithDescription("Cria um novo voucher no sistema. O Id é gerado automaticamente.")
            .Accepts<Voucher>("application/json")
            .Produces<Voucher>(StatusCodes.Status201Created, "application/json")
            .WithTags("Vouchers");

            app.MapPut("/vouchers/{id}", async (Guid id, Voucher voucher, VoucherService service) =>
            {
                voucher.Id = id;
                await service.UpdateAsync(voucher);
                return Results.Ok(voucher);
            })
            .WithName("UpdateVoucher")
            .WithSummary("Atualiza um voucher.")
            .WithDescription("Atualiza os dados de um voucher existente.")
            .Accepts<Voucher>("application/json")
            .Produces<Voucher>(StatusCodes.Status200OK, "application/json")
            .WithTags("Vouchers");

            app.MapDelete("/vouchers/{id}", async (Guid id, VoucherService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteVoucher")
            .WithSummary("Remove um voucher.")
            .WithDescription("Remove um voucher do sistema pelo seu identificador.")
            .Produces(StatusCodes.Status204NoContent)
            .WithTags("Vouchers");

            app.MapGet("/vouchers/code/{code}", async (string code, VoucherService service) =>
            {
                var voucher = await service.GetByCodeAsync(code);
                return voucher is not null ? Results.Ok(voucher) : Results.NotFound();
            })
            .WithName("GetVoucherByCode")
            .WithSummary("Busca voucher por código.")
            .WithDescription("Retorna um voucher específico pelo seu código.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Vouchers");
        }
    }
}
