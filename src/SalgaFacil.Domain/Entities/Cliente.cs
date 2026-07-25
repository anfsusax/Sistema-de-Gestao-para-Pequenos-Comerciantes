namespace SalgaFacil.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Cnpj { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }

    /// <summary>
    /// Telefone em formato normalizado (somente dígitos, com DDI 55), calculado por
    /// <see cref="Services.TelefoneNormalizador"/> a partir de <see cref="Telefone"/>/<see cref="WhatsApp"/>.
    /// Usado como identificador de deduplicação de cliente (ver índice em SalgaFacilDbContext).
    /// Nullable por compatibilidade com cadastros antigos ainda não migrados.
    /// </summary>
    public string? TelefoneNormalizado { get; set; }
    public string? Email { get; set; }
    public string? SenhaHash { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = [];
    public ICollection<EnderecoCliente> Enderecos { get; set; } = [];
}
