using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public DateTime? DataEntregaPrevista { get; set; }
    public StatusPedido Status { get; set; } = StatusPedido.Aguardando;
    public decimal Total { get; set; }
    public string? Observacoes { get; set; }
    public bool Entrega { get; set; }
    public string? EnderecoEntrega { get; set; }

    /// <summary>
    /// Forma de pagamento escolhida. Nullable: pedidos criados pelo painel administrativo
    /// (encomenda por telefone/balcão) podem não ter esse dado; o checkout do cardápio
    /// público EXIGE o preenchimento (validado em LojaPublicaService.CriarPedidoVisitanteAsync,
    /// não no banco, para não quebrar o fluxo administrativo existente).
    /// </summary>
    public FormaPagamento? FormaPagamento { get; set; }

    /// <summary>
    /// Status do pagamento Pix manual (PIX-MANUAL-001). Só tem sentido quando
    /// <see cref="FormaPagamento"/> é <see cref="Enums.FormaPagamento.Pix"/>. Nulo em pedidos Pix
    /// antigos (anteriores a esta funcionalidade) é interpretado como Aguardando pelo serviço —
    /// não é reescrito em massa aqui para não sair do escopo (sem migration de dados).
    /// </summary>
    public StatusPagamento? StatusPagamento { get; set; }

    /// <summary>UTC. Preenchido quando o comerciante confirma o recebimento do Pix.</summary>
    public DateTime? PagamentoConfirmadoEm { get; set; }

    public ICollection<PedidoItem> Itens { get; set; } = [];
}
