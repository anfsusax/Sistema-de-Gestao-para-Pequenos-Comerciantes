using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Services;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

/// <summary>
/// Ferramenta de manutenção para a deduplicação de cliente por telefone
/// (ver _ia/DECISOES.md 2026-07-24, "Deduplicação de Cliente Por Telefone").
/// Nunca exclui clientes — apenas reporta possíveis duplicados e, quando o operador
/// confirma explicitamente qual é o cliente principal, consolida (move Pedidos/Vendas
/// para o principal e inativa o duplicado).
/// </summary>
public class ClienteManutencaoService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    private int EmpresaId => empresa.RequireEmpresaId();

    /// <summary>
    /// Preenche TelefoneNormalizado para clientes cadastrados antes desta funcionalidade
    /// (coluna ainda nula). Idempotente — só toca linhas pendentes, pode ser executado
    /// quantas vezes for preciso sem efeito colateral. Reutiliza TelefoneNormalizador —
    /// nenhuma lógica de normalização é duplicada aqui.
    /// </summary>
    public async Task<int> NormalizarTelefonesExistentesAsync()
    {
        var pendentes = await db.Clientes
            .Where(c => c.EmpresaId == EmpresaId && c.TelefoneNormalizado == null)
            .ToListAsync();

        foreach (var c in pendentes)
        {
            c.TelefoneNormalizado = TelefoneNormalizador.Normalizar(c.Telefone) ?? TelefoneNormalizador.Normalizar(c.WhatsApp);
            c.AtualizadoEm = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return pendentes.Count;
    }

    /// <summary>Lista grupos de clientes ativos com o mesmo TelefoneNormalizado (possíveis duplicados). Somente leitura, não altera nada.</summary>
    public async Task<List<GrupoDuplicado>> IdentificarDuplicadosAsync()
    {
        var clientes = await db.Clientes
            .Where(c => c.EmpresaId == EmpresaId && c.Ativo && c.TelefoneNormalizado != null)
            .Select(c => new ClienteDuplicadoDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Telefone = c.Telefone,
                TelefoneNormalizado = c.TelefoneNormalizado!,
                CriadoEm = c.CriadoEm,
                TotalPedidos = c.Pedidos.Count
            })
            .ToListAsync();

        return clientes
            .GroupBy(c => c.TelefoneNormalizado)
            .Where(g => g.Count() > 1)
            .Select(g => new GrupoDuplicado { TelefoneNormalizado = g.Key, Clientes = g.OrderBy(c => c.CriadoEm).ToList() })
            .OrderBy(g => g.TelefoneNormalizado)
            .ToList();
    }

    /// <summary>
    /// Consolida um cliente duplicado no cliente principal: move todos os Pedidos e Vendas
    /// do duplicado para o principal e inativa o duplicado (Ativo = false). Nunca exclui
    /// (Remove) nenhum registro. Exige que os dois clientes pertençam à empresa atual e
    /// tenham o mesmo TelefoneNormalizado, para impedir consolidar clientes que não são
    /// de fato o mesmo.
    /// </summary>
    public async Task ConsolidarAsync(int clientePrincipalId, int clienteDuplicadoId)
    {
        if (clientePrincipalId == clienteDuplicadoId)
            throw new InvalidOperationException("Selecione dois clientes diferentes para consolidar.");

        var principal = await db.Clientes.FirstOrDefaultAsync(c => c.Id == clientePrincipalId && c.EmpresaId == EmpresaId)
            ?? throw new InvalidOperationException("Cliente principal não encontrado.");
        var duplicado = await db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteDuplicadoId && c.EmpresaId == EmpresaId)
            ?? throw new InvalidOperationException("Cliente duplicado não encontrado.");

        if (principal.TelefoneNormalizado is null || principal.TelefoneNormalizado != duplicado.TelefoneNormalizado)
            throw new InvalidOperationException("Os dois clientes precisam ter o mesmo telefone normalizado para serem consolidados.");

        var pedidos = await db.Pedidos.Where(p => p.ClienteId == clienteDuplicadoId && p.EmpresaId == EmpresaId).ToListAsync();
        foreach (var p in pedidos)
            p.ClienteId = clientePrincipalId;

        var vendas = await db.Vendas.Where(v => v.ClienteId == clienteDuplicadoId && v.EmpresaId == EmpresaId).ToListAsync();
        foreach (var v in vendas)
            v.ClienteId = clientePrincipalId;

        var nota = $"Consolidado em {DateTime.UtcNow:yyyy-MM-dd} no cliente #{clientePrincipalId} (duplicidade de telefone; {pedidos.Count} pedido(s) e {vendas.Count} venda(s) transferidos).";
        duplicado.Observacoes = string.IsNullOrWhiteSpace(duplicado.Observacoes) ? nota : $"{duplicado.Observacoes} | {nota}";
        duplicado.Ativo = false;
        duplicado.AtualizadoEm = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }
}

public class ClienteDuplicadoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string TelefoneNormalizado { get; set; } = "";
    public DateTime CriadoEm { get; set; }
    public int TotalPedidos { get; set; }
}

public class GrupoDuplicado
{
    public string TelefoneNormalizado { get; set; } = "";
    public List<ClienteDuplicadoDto> Clientes { get; set; } = [];
}
