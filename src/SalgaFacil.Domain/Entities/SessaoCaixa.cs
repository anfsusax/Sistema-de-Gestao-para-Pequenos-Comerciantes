using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

/// <summary>
/// Sessão de caixa (abertura/fechamento de turno). Uma <see cref="Venda"/> só pode ser
/// registrada com uma sessão Aberta vinculada — ver _ia/DECISOES.md (2026-07-01,
/// "Caixa obrigatório para vender"). Modelo assume 1 sessão aberta por vez no sistema
/// inteiro (não por usuário/terminal) — adequado para um único ponto de venda físico.
/// </summary>
public class SessaoCaixa
{
    public int Id { get; set; }
    public int UsuarioAberturaId { get; set; }
    public Usuario UsuarioAbertura { get; set; } = null!;
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public decimal ValorAbertura { get; set; }
    public int? UsuarioFechamentoId { get; set; }
    public Usuario? UsuarioFechamento { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal? ValorContado { get; set; }
    public decimal? ValorEsperado { get; set; }
    public decimal? Diferenca { get; set; }
    public StatusSessaoCaixa Status { get; set; } = StatusSessaoCaixa.Aberta;
    public ICollection<MovimentoCaixa> Movimentos { get; set; } = [];
    public ICollection<Venda> Vendas { get; set; } = [];
}
