using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Softdesign.CoP.Observability.Order.Domain;
using Softdesign.CoP.Observability.Order.Service;

namespace Softdesign.CoP.Observability.Order.Endpoints
{
    public static class VoucherEndpoints
    {
        public static void MapVoucherEndpoints(this WebApplication app)
        {
            app.MapGet("/vouchers", async (VoucherService service) => Results.Ok(await service.GetAllAsync()))
                .WithName("ListVouchers").WithSummary("Lista todos os vouchers");

            app.MapGet("/vouchers/{id}", async (Guid id, VoucherService service) =>
            {
                var voucher = await service.GetByIdAsync(id);
                return voucher is not null ? Results.Ok(voucher) : Results.NotFound();
            }).WithName("GetVoucher").WithSummary("Busca voucher por Id");

            app.MapPost("/vouchers", async (Voucher voucher, VoucherService service) =>
            {
                voucher.Id = Guid.NewGuid();
                await service.AddAsync(voucher);
                return Results.Created($"/vouchers/{voucher.Id}", voucher);
            }).WithName("CreateVoucher").WithSummary("Cria um novo voucher");

            app.MapPut("/vouchers/{id}", async (Guid id, Voucher voucher, VoucherService service) =>
            {
                voucher.Id = id;
                await service.UpdateAsync(voucher);
                return Results.Ok(voucher);
            }).WithName("UpdateVoucher").WithSummary("Atualiza um voucher");

            app.MapDelete("/vouchers/{id}", async (Guid id, VoucherService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }).WithName("DeleteVoucher").WithSummary("Remove um voucher");
        }
    }
}
