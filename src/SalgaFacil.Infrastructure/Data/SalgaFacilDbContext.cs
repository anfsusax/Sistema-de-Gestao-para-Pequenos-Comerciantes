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
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.Nome).HasMaxLength(200);
            e.Property(c => c.Telefone).HasMaxLength(30);
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

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
        });
    }
}
