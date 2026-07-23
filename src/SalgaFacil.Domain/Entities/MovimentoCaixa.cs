using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

/// <summary>Sangria (retirada) ou suprimento (reforço) de dinheiro dentro de uma sessão de caixa aberta.</summary>
public class MovimentoCaixa
{
    public int Id { get; set; }
    public int SessaoCaixaId { get; set; }
    public SessaoCaixa SessaoCaixa { get; set; } = null!;
    public TipoMovimentoCaixa Tipo { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
