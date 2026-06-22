using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class EmpresaService(SalgaFacilDbContext db)
{
    public Task<Empresa?> ObterAsync() => db.Empresas.FirstOrDefaultAsync();

    public async Task SalvarAsync(Empresa empresa)
    {
        if (empresa.Id == 0)
            db.Empresas.Add(empresa);
        else
            db.Empresas.Update(empresa);
        await db.SaveChangesAsync();
    }
}
