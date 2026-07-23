using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

/// <remarks>
/// Modelo assume 1 sessão de caixa aberta por vez no sistema inteiro (não por usuário/terminal) —
/// adequado para um único ponto de venda físico. Ver _ia/DECISOES.md.
/// </remarks>
public class CaixaService(SalgaFacilDbContext db)
{
    public Task<SessaoCaixa?> ObterSessaoAbertaAsync() =>
        db.SessoesCaixa
            .Include(s => s.UsuarioAbertura)
            .FirstOrDefaultAsync(s => s.Status == StatusSessaoCaixa.Aberta);

    public Task<SessaoCaixa?> ObterAsync(int id) =>
        db.SessoesCaixa
            .Include(s => s.UsuarioAbertura)
            .Include(s => s.UsuarioFechamento)
            .Include(s => s.Movimentos)
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<SessaoCaixa>> ListarAsync() =>
        db.SessoesCaixa
            .Include(s => s.UsuarioAbertura)
            .OrderByDescending(s => s.DataAbertura)
            .ToListAsync();

    public async Task<int> AbrirAsync(int usuarioId, decimal valorAbertura)
    {
        if (await ObterSessaoAbertaAsync() != null)
            throw new InvalidOperationException("Já existe uma sessão de caixa aberta.");
        if (valorAbertura < 0)
            throw new InvalidOperationException("Valor de abertura não pode ser negativo.");

        var sessao = new SessaoCaixa { UsuarioAberturaId = usuarioId, ValorAbertura = valorAbertura };
        db.SessoesCaixa.Add(sessao);
        await db.SaveChangesAsync();
        return sessao.Id;
    }

    public async Task RegistrarMovimentoAsync(int sessaoId, TipoMovimentoCaixa tipo, decimal valor, string descricao, int usuarioId)
    {
        if (valor <= 0)
            throw new InvalidOperationException("Valor do movimento deve ser maior que zero.");

        var sessao = await db.SessoesCaixa.FindAsync(sessaoId);
        if (sessao == null || sessao.Status != StatusSessaoCaixa.Aberta)
            throw new InvalidOperationException("Sessão de caixa não está aberta.");

        db.MovimentosCaixa.Add(new MovimentoCaixa
        {
            SessaoCaixaId = sessaoId,
            Tipo = tipo,
            Valor = valor,
            Descricao = descricao,
            UsuarioId = usuarioId
        });
        await db.SaveChangesAsync();
    }

    public async Task<ResumoSessaoCaixa> ObterResumoAsync(int sessaoId)
    {
        var sessao = await db.SessoesCaixa.FindAsync(sessaoId)
            ?? throw new InvalidOperationException("Sessão de caixa não encontrada.");

        var vendasDinheiro = await db.Vendas
            .Where(v => v.SessaoCaixaId == sessaoId && v.Status == StatusVenda.Finalizada && v.FormaPagamento == FormaPagamento.Dinheiro)
            .SumAsync(v => (decimal?)v.Total) ?? 0m;
        var vendasOutras = await db.Vendas
            .Where(v => v.SessaoCaixaId == sessaoId && v.Status == StatusVenda.Finalizada && v.FormaPagamento != FormaPagamento.Dinheiro)
            .SumAsync(v => (decimal?)v.Total) ?? 0m;
        var sangrias = await db.MovimentosCaixa
            .Where(m => m.SessaoCaixaId == sessaoId && m.Tipo == TipoMovimentoCaixa.Sangria)
            .SumAsync(m => (decimal?)m.Valor) ?? 0m;
        var suprimentos = await db.MovimentosCaixa
            .Where(m => m.SessaoCaixaId == sessaoId && m.Tipo == TipoMovimentoCaixa.Suprimento)
            .SumAsync(m => (decimal?)m.Valor) ?? 0m;

        return new ResumoSessaoCaixa
        {
            ValorAbertura = sessao.ValorAbertura,
            VendasDinheiro = vendasDinheiro,
            VendasOutrasFormas = vendasOutras,
            Sangrias = sangrias,
            Suprimentos = suprimentos,
            ValorEsperado = sessao.ValorAbertura + vendasDinheiro + suprimentos - sangrias
        };
    }

    public async Task FecharAsync(int sessaoId, decimal valorContado, int usuarioId)
    {
        var sessao = await db.SessoesCaixa.FindAsync(sessaoId)
            ?? throw new InvalidOperationException("Sessão de caixa não encontrada.");
        if (sessao.Status != StatusSessaoCaixa.Aberta)
            throw new InvalidOperationException("Sessão de caixa já está fechada.");

        var resumo = await ObterResumoAsync(sessaoId);
        sessao.ValorContado = valorContado;
        sessao.ValorEsperado = resumo.ValorEsperado;
        sessao.Diferenca = valorContado - resumo.ValorEsperado;
        sessao.DataFechamento = DateTime.UtcNow;
        sessao.UsuarioFechamentoId = usuarioId;
        sessao.Status = StatusSessaoCaixa.Fechada;
        await db.SaveChangesAsync();
    }
}

public class ResumoSessaoCaixa
{
    public decimal ValorAbertura { get; set; }
    public decimal VendasDinheiro { get; set; }
    public decimal VendasOutrasFormas { get; set; }
    public decimal Sangrias { get; set; }
    public decimal Suprimentos { get; set; }
    public decimal ValorEsperado { get; set; }
}
