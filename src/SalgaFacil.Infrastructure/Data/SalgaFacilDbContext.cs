using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;

namespace SalgaFacil.Infrastructure.Data;

public class SalgaFacilDbContext : DbContext
{
    public SalgaFacilDbContext(DbContextOptions<SalgaFacilDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pacote> Pacotes => Set<Pacote>();
    public DbSet<PacoteItem> PacoteItens => Set<PacoteItem>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<VendaItem> VendaItens => Set<VendaItem>();
    public DbSet<SessaoCaixa> SessoesCaixa => Set<SessaoCaixa>();
    public DbSet<MovimentoCaixa> MovimentosCaixa => Set<MovimentoCaixa>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(e =>
        {
            e.Property(p => p.PrecoVenda).HasPrecision(18, 2);
            e.Property(p => p.CustoEstimado).HasPrecision(18, 2);
            e.Property(p => p.Nome).HasMaxLength(200);
            e.Property(p => p.Categoria).HasMaxLength(100);
            e.Property(p => p.CodigoBarras).HasMaxLength(64);
            // Índice único parcial: SQLite permite múltiplos NULL em índice único (cada NULL
            // é distinto), então produtos sem código de barras cadastrado não colidem entre si.
            e.HasIndex(p => p.CodigoBarras).IsUnique();
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.Nome).HasMaxLength(200);
            e.Property(c => c.Telefone).HasMaxLength(30);
            e.Property(c => c.Cpf).HasMaxLength(14);
        });

        modelBuilder.Entity<Pacote>(e =>
        {
            e.Property(p => p.Preco).HasPrecision(18, 2);
            e.Property(p => p.Nome).HasMaxLength(200);
        });

        modelBuilder.Entity<Pedido>(e =>
        {
            e.Property(p => p.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PedidoItem>(e =>
        {
            e.Property(p => p.ValorUnitario).HasPrecision(18, 2);
            e.Property(p => p.Total).HasPrecision(18, 2);
            e.Property(p => p.Descricao).HasMaxLength(300);
        });

        modelBuilder.Entity<Venda>(e =>
        {
            e.Property(v => v.Subtotal).HasPrecision(18, 2);
            e.Property(v => v.Desconto).HasPrecision(18, 2);
            e.Property(v => v.Total).HasPrecision(18, 2);
            e.Property(v => v.ValorRecebido).HasPrecision(18, 2);
            e.Property(v => v.Troco).HasPrecision(18, 2);
            // Cliente opcional: ao excluir um cliente, a venda historica permanece (ClienteId vira null)
            // em vez de apagar o registro financeiro em cascata.
            e.HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(v => v.Usuario).WithMany().HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.SessaoCaixa).WithMany(s => s.Vendas).HasForeignKey(v => v.SessaoCaixaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendaItem>(e =>
        {
            e.Property(i => i.ValorUnitario).HasPrecision(18, 2);
            e.Property(i => i.Total).HasPrecision(18, 2);
            e.Property(i => i.Descricao).HasMaxLength(300);
            e.HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SessaoCaixa>(e =>
        {
            e.Property(s => s.ValorAbertura).HasPrecision(18, 2);
            e.Property(s => s.ValorContado).HasPrecision(18, 2);
            e.Property(s => s.ValorEsperado).HasPrecision(18, 2);
            e.Property(s => s.Diferenca).HasPrecision(18, 2);
            e.HasOne(s => s.UsuarioAbertura).WithMany().HasForeignKey(s => s.UsuarioAberturaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.UsuarioFechamento).WithMany().HasForeignKey(s => s.UsuarioFechamentoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimentoCaixa>(e =>
        {
            e.Property(m => m.Valor).HasPrecision(18, 2);
            e.Property(m => m.Descricao).HasMaxLength(300);
            e.HasOne(m => m.SessaoCaixa).WithMany(s => s.Movimentos).HasForeignKey(m => m.SessaoCaixaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Usuario).WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
        });
    }
}
