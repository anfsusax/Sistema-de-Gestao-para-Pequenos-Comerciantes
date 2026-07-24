using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;

namespace SalgaFacil.Infrastructure.Data;

public class SalgaFacilDbContext : DbContext
{
    public SalgaFacilDbContext(DbContextOptions<SalgaFacilDbContext> options) : base(options) { }

    public DbSet<CategoriaProduto> CategoriasProduto => Set<CategoriaProduto>();
    public DbSet<UnidadeMedida> UnidadesMedida => Set<UnidadeMedida>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<EnderecoCliente> EnderecosCliente => Set<EnderecoCliente>();
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
        modelBuilder.Entity<Empresa>(e =>
        {
            e.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.NomeFantasia).HasMaxLength(200);
            e.Property(x => x.RazaoSocial).HasMaxLength(200);
            e.Property(x => x.Cnpj).HasMaxLength(18);
            e.Property(x => x.Telefone).HasMaxLength(30);
            e.Property(x => x.WhatsApp).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Endereco).HasMaxLength(300);
            e.Property(x => x.Descricao).HasMaxLength(1000);
            e.Property(x => x.LogoUrl).HasMaxLength(500);
            e.Property(x => x.BannerUrl).HasMaxLength(500);
            e.Property(x => x.HorarioFuncionamento).HasMaxLength(300);
            e.Property(x => x.Instagram).HasMaxLength(120);
            e.Property(x => x.Facebook).HasMaxLength(120);
            e.Property(x => x.PixChave).HasMaxLength(140);
            e.Property(x => x.PixNomeBeneficiario).HasMaxLength(200);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.Nome).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Empresa).WithMany(em => em.Usuarios).HasForeignKey(u => u.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(u => u.EmpresaId);
        });

        modelBuilder.Entity<CategoriaProduto>(e =>
        {
            e.Property(c => c.Nome).HasMaxLength(100).IsRequired();
            e.Property(c => c.Descricao).HasMaxLength(500);
            e.HasIndex(c => new { c.EmpresaId, c.Nome }).IsUnique();
            e.HasIndex(c => c.Ordem);
            e.HasOne(c => c.Empresa).WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UnidadeMedida>(e =>
        {
            e.Property(u => u.Sigla).HasMaxLength(10).IsRequired();
            e.Property(u => u.Nome).HasMaxLength(80).IsRequired();
            e.HasIndex(u => u.Sigla).IsUnique();
        });

        modelBuilder.Entity<Produto>(e =>
        {
            e.Property(p => p.Codigo).HasMaxLength(40);
            e.Property(p => p.Nome).HasMaxLength(200).IsRequired();
            e.Property(p => p.Descricao).HasMaxLength(1000);
            e.Property(p => p.FotoUrl).HasMaxLength(500);
            e.Property(p => p.CodigoBarras).HasMaxLength(64);
            e.Property(p => p.PrecoVenda).HasPrecision(18, 2);
            e.Property(p => p.CustoEstimado).HasPrecision(18, 2);
            e.Property(p => p.EstoqueAtual).HasPrecision(18, 3);
            e.Property(p => p.EstoqueMinimo).HasPrecision(18, 3);
            e.HasIndex(p => new { p.EmpresaId, p.Codigo });
            e.HasIndex(p => new { p.EmpresaId, p.Nome });
            e.HasIndex(p => p.CategoriaId);
            e.HasIndex(p => new { p.EmpresaId, p.CodigoBarras }).IsUnique();
            e.HasOne(p => p.Empresa).WithMany().HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Categoria).WithMany(c => c.Produtos).HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.UnidadeMedida).WithMany(u => u.Produtos).HasForeignKey(p => p.UnidadeMedidaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.Nome).HasMaxLength(200).IsRequired();
            e.Property(c => c.Telefone).HasMaxLength(30);
            e.Property(c => c.WhatsApp).HasMaxLength(30);
            e.Property(c => c.TelefoneNormalizado).HasMaxLength(20);
            e.Property(c => c.Cpf).HasMaxLength(14);
            e.Property(c => c.Cnpj).HasMaxLength(18);
            e.Property(c => c.Email).HasMaxLength(200);
            e.Property(c => c.Observacoes).HasMaxLength(1000);
            e.HasIndex(c => new { c.EmpresaId, c.Nome });
            e.HasIndex(c => new { c.EmpresaId, c.Telefone });
            e.HasIndex(c => new { c.EmpresaId, c.WhatsApp });
            // Índice NÃO único por enquanto — existem cadastros anteriores à normalização que
            // podem colidir. A restrição UNIQUE (impede duplicidade a nível de banco, inclusive
            // sob concorrência) é o passo seguinte, condicionado à checagem/consolidação de
            // duplicados reais no banco do usuário. Ver _ia/RISCOS.md e _ia/DECISOES.md 2026-07-24.
            e.HasIndex(c => new { c.EmpresaId, c.TelefoneNormalizado });
            e.HasOne(c => c.Empresa).WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EnderecoCliente>(e =>
        {
            e.Property(a => a.Cep).HasMaxLength(10);
            e.Property(a => a.Logradouro).HasMaxLength(200).IsRequired();
            e.Property(a => a.Numero).HasMaxLength(20);
            e.Property(a => a.Complemento).HasMaxLength(100);
            e.Property(a => a.Bairro).HasMaxLength(100);
            e.Property(a => a.Cidade).HasMaxLength(100).IsRequired();
            e.Property(a => a.Estado).HasMaxLength(2).IsRequired();
            e.Property(a => a.Referencia).HasMaxLength(200);
            e.HasOne(a => a.Cliente).WithMany(c => c.Enderecos).HasForeignKey(a => a.ClienteId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Pacote>(e =>
        {
            e.Property(p => p.Preco).HasPrecision(18, 2);
            e.Property(p => p.Nome).HasMaxLength(200);
            e.HasOne(p => p.Empresa).WithMany().HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pedido>(e =>
        {
            e.Property(p => p.Total).HasPrecision(18, 2);
            e.Property(p => p.Observacoes).HasMaxLength(500);
            e.Property(p => p.EnderecoEntrega).HasMaxLength(400);
            e.HasOne(p => p.Empresa).WithMany().HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => p.EmpresaId);
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
            e.HasOne(v => v.Empresa).WithMany().HasForeignKey(v => v.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(v => v.Usuario).WithMany().HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.SessaoCaixa).WithMany(s => s.Vendas).HasForeignKey(v => v.SessaoCaixaId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(v => v.EmpresaId);
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
            e.HasOne(s => s.Empresa).WithMany().HasForeignKey(s => s.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.UsuarioAbertura).WithMany().HasForeignKey(s => s.UsuarioAberturaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.UsuarioFechamento).WithMany().HasForeignKey(s => s.UsuarioFechamentoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => s.EmpresaId);
        });

        modelBuilder.Entity<MovimentoCaixa>(e =>
        {
            e.Property(m => m.Valor).HasPrecision(18, 2);
            e.Property(m => m.Descricao).HasMaxLength(300);
            e.HasOne(m => m.SessaoCaixa).WithMany(s => s.Movimentos).HasForeignKey(m => m.SessaoCaixaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Usuario).WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
