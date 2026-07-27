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
    /// público EXIGE o preenchimento (validado em LojaPublicaService.CriarPedidoClienteAsync,
    /// não no banco, para não quebrar o fluxo administrativo existente).
    /// </summary>
    public FormaPagamento? FormaPagamento { get; set; }

    /// <summary>
    /// Status do pagamento Pix manual (PIX-MANUAL-001/002). Só tem sentido quando
    /// <see cref="FormaPagamento"/> é <see cref="Enums.FormaPagamento.Pix"/>. Nulo em pedidos Pix
    /// antigos (anteriores a esta funcionalidade) é interpretado como Aguardando pelo serviço —
    /// não é reescrito em massa aqui para não sair do escopo (sem migration de dados).
    /// </summary>
    public StatusPagamento? StatusPagamento { get; set; }

    /// <summary>UTC. Preenchido quando o comerciante confirma o recebimento do Pix.</summary>
    public DateTime? PagamentoConfirmadoEm { get; set; }

    /// <summary>
    /// Usuário (funcionário da loja) que confirmou o recebimento do Pix. Nulo até a confirmação
    /// acontecer. PIX-MANUAL-002: acompanha <see cref="PagamentoConfirmadoEm"/> para satisfazer
    /// "registrar data, hora e usuário responsável" na confirmação.
    /// </summary>
    public int? PagamentoConfirmadoPorUsuarioId { get; set; }
    public Usuario? PagamentoConfirmadoPorUsuario { get; set; }

    #region Comprovante Pix manual (PIX-MANUAL-002)

    /// <summary>
    /// Chave/identificador interno do arquivo no armazenamento privado (ver
    /// ComprovanteArmazenamentoService) — NUNCA é uma URL pública nem um caminho físico exposto
    /// ao cliente. Usado só server-side para localizar o arquivo em disco. Sobrescrito a cada
    /// novo envio (reenvio após rejeição substitui o arquivo anterior).
    /// </summary>
    public string? ComprovanteCaminho { get; set; }

    /// <summary>Nome original do arquivo enviado pelo cliente (só para exibição, ex.: "comprovante.pdf").</summary>
    public string? ComprovanteNomeOriginal { get; set; }

    /// <summary>Content-Type detectado no upload (ex.: "image/jpeg", "application/pdf").</summary>
    public string? ComprovanteContentType { get; set; }

    /// <summary>Tamanho em bytes do arquivo armazenado.</summary>
    public long? ComprovanteTamanhoBytes { get; set; }

    /// <summary>UTC. Quando o comprovante atualmente armazenado foi enviado (ou reenviado).</summary>
    public DateTime? ComprovanteEnviadoEm { get; set; }

    /// <summary>Motivo informado pela loja ao rejeitar o comprovante. Nulo fora do estado Rejeitado.</summary>
    public string? ComprovanteMotivoRejeicao { get; set; }

    /// <summary>Último usuário (funcionário) que revisou o comprovante — confirmou, rejeitou ou marcou em análise.</summary>
    public int? ComprovanteRevisadoPorUsuarioId { get; set; }
    public Usuario? ComprovanteRevisadoPorUsuario { get; set; }

    /// <summary>UTC da última revisão (confirmação, rejeição ou marcação "em análise").</summary>
    public DateTime? ComprovanteRevisadoEm { get; set; }

    #endregion

    public ICollection<PedidoItem> Itens { get; set; } = [];
}
