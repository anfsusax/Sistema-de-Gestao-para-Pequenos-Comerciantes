using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace SalgaFacil.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalgaFacilDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (await db.Usuarios.AnyAsync()) return;

        db.Usuarios.Add(new Usuario
        {
            Nome = "Maria Barbosa",
            Email = "maria@salgadospro.com",
            SenhaHash = HashSenha("123456"),
            Ativo = true
        });

        db.Empresas.Add(new Empresa
        {
            Nome = "SalgadosPro",
            Telefone = "(11) 99999-0000",
            Email = "contato@salgadospro.com"
        });

        var produtos = new[]
        {
            new Produto { Nome = "Coxinha de Frango", Categoria = "Frango", Tipo = TipoProduto.Frito, PrecoVenda = 3.50m, CustoEstimado = 1.20m, Ativo = true },
            new Produto { Nome = "Bolinha de Queijo", Categoria = "Queijo", Tipo = TipoProduto.Frito, PrecoVenda = 2.80m, CustoEstimado = 0.90m, Ativo = true },
            new Produto { Nome = "Risole de Camarão", Categoria = "Camarão", Tipo = TipoProduto.Frito, PrecoVenda = 4.00m, CustoEstimado = 1.80m, Ativo = true },
            new Produto { Nome = "Pão de Queijo", Categoria = "Queijo", Tipo = TipoProduto.Assado, PrecoVenda = 2.00m, CustoEstimado = 0.70m, Ativo = true },
            new Produto { Nome = "Empada de Frango", Categoria = "Frango", Tipo = TipoProduto.Assado, PrecoVenda = 4.50m, CustoEstimado = 1.60m, Ativo = true },
            new Produto { Nome = "Kibe de Carne", Categoria = "Carne", Tipo = TipoProduto.Frito, PrecoVenda = 3.00m, CustoEstimado = 1.10m, Ativo = false }
        };
        db.Produtos.AddRange(produtos);
        await db.SaveChangesAsync();

        var coxinha = produtos[0];
        var bolinha = produtos[1];
        var risole = produtos[2];
        var pao = produtos[3];

        var maria = new Cliente { Nome = "Maria Silva", Telefone = "(11) 98888-1111", Endereco = "Rua A, 100" };
        var ana = new Cliente { Nome = "Ana Costa", Telefone = "(11) 98888-2222", Endereco = "Rua B, 200" };
        var fernanda = new Cliente { Nome = "Fernanda Lima", Telefone = "(11) 98888-3333", Endereco = "Rua C, 300" };
        db.Clientes.AddRange(maria, ana, fernanda);
        await db.SaveChangesAsync();

        var hoje = DateTime.UtcNow.Date;
        var pedidos = new List<Pedido>
        {
            CriarPedido(maria.Id, hoje, StatusPedido.EmProducao, 375m, new[] {
                (coxinha.Id, (int?)null, "Coxinha de Frango", 50, 3.50m),
                (risole.Id, (int?)null, "Risole de Camarão", 50, 3.75m)
            }),
            CriarPedido(ana.Id, hoje, StatusPedido.Pronto, 530m, new[] {
                (bolinha.Id, (int?)null, "Bolinha de Queijo", 100, 2.80m),
                (produtos[5].Id, (int?)null, "Kibe de Carne", 100, 2.50m)
            }),
            CriarPedido(fernanda.Id, hoje, StatusPedido.Aguardando, 275m, new[] {
                (coxinha.Id, (int?)null, "Coxinha de Frango", 50, 3.50m),
                (pao.Id, (int?)null, "Pão de Queijo", 50, 2.00m)
            }),
            CriarPedido(ana.Id, hoje.AddDays(-1), StatusPedido.Entregue, 450m, new[] {
                (coxinha.Id, (int?)null, "Coxinha de Frango", 100, 4.50m)
            }),
            CriarPedido(maria.Id, hoje.AddDays(-2), StatusPedido.Finalizado, 800m, new[] {
                (bolinha.Id, (int?)null, "Bolinha de Queijo", 200, 4.00m)
            })
        };

        foreach (var p in pedidos)
            p.DataEntregaPrevista = p.Data.AddHours(14);

        db.Pedidos.AddRange(pedidos);
        await db.SaveChangesAsync();
    }

    private static Pedido CriarPedido(int clienteId, DateTime data, StatusPedido status, decimal total, (int produtoId, int? pacoteId, string desc, int qtd, decimal valor)[] itens)
    {
        var pedido = new Pedido { ClienteId = clienteId, Data = data, Status = status, Total = total };
        foreach (var (prodId, pacoteId, desc, qtd, valor) in itens)
        {
            pedido.Itens.Add(new PedidoItem
            {
                ProdutoId = pacoteId.HasValue ? null : prodId,
                PacoteId = pacoteId,
                Descricao = desc,
                Quantidade = qtd,
                ValorUnitario = valor,
                Total = qtd * valor
            });
        }
        return pedido;
    }

    public static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(bytes);
    }
}
