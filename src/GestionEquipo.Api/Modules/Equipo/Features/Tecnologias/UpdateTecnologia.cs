using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Tecnologias;

public static class UpdateTecnologia
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var repo = new Repository<Tecnologia>(db);
        var tecnologia = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        var categoriaId = body["categoria_id"]?.GetValue<int>() ?? tecnologia.CategoriaId;
        var nombre = body["nombre"]?.GetValue<string>() ?? tecnologia.Nombre;
        tecnologia.Actualizar(categoriaId, nombre);
        if (body["activo"] is { } activo) { if (activo.GetValue<bool>()) tecnologia.Activar(); else tecnologia.Desactivar(); }

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { tecnologia.Id, message = "Guardado correctamente" });
    }
}
