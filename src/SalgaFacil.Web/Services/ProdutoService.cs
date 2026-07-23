using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class ProdutoService(SalgaFacilDbContext db)
{
    public Task<List<Produto>> ListarAsync(
        string? filtroTipo = null,
        bool? apenasAtivos = null,
        int? categoriaId = null,
        string? busca = null) =>
        db.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.UnidadeMedida)
            .Where(p => filtroTipo == null
                || (filtroTipo == "Frito" && p.Tipo == TipoProduto.Frito)
                || (filtroTipo == "Assado" && p.Tipo == TipoProduto.Assado))
            .Where(p => !apenasAtivos.HasValue || p.Ativo == apenasAtivos.Value)
            .Where(p => !categoriaId.HasValue || p.CategoriaId == categoriaId.Value)
            .Where(p => string.IsNullOrWhiteSpace(busca)
                || p.Nome.Contains(busca)
                || (p.Codigo != null && p.Codigo.Contains(busca))
                || (p.CodigoBarras != null && p.CodigoBarras.Contains(busca)))
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public Task<Produto?> ObterAsync(int id) =>
        db.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.UnidadeMedida)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task SalvarAsync(Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Nome))
            throw new InvalidOperationException("Nome do produto é obrigatório.");
        if (produto.CategoriaId <= 0)
            throw new InvalidOperationException("Categoria é obrigatória.");
        if (produto.UnidadeMedidaId <= 0)
            throw new InvalidOperationException("Unidade de medida é obrigatória.");
        if (produto.PrecoVenda < 0 || produto.CustoEstimado < 0)
            throw new InvalidOperationException("Preço e custo não podem ser negativos.");
        if (produto.EstoqueAtual < 0 || produto.EstoqueMinimo < 0)
            throw new InvalidOperationException("Estoques não podem ser negativos.");

        produto.Nome = produto.Nome.Trim();
        if (!string.IsNullOrWhiteSpace(produto.Codigo))
            produto.Codigo = produto.Codigo.Trim().ToUpperInvariant();

        if (produto.Id == 0)
        {
            var novo = new Produto
            {
                Codigo = produto.Codigo,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                CategoriaId = produto.CategoriaId,
                UnidadeMedidaId = produto.UnidadeMedidaId,
                Tipo = produto.Tipo,
                FotoUrl = produto.FotoUrl,
                CodigoBarras = produto.CodigoBarras,
                PrecoVenda = produto.PrecoVenda,
                CustoEstimado = produto.CustoEstimado,
                EstoqueAtual = produto.EstoqueAtual,
                EstoqueMinimo = produto.EstoqueMinimo,
                Ativo = produto.Ativo,
                CriadoEm = DateTime.UtcNow
            };
            db.Produtos.Add(novo);
        }
        else
        {
            var existente = await db.Produtos.FindAsync(produto.Id)
                ?? throw new InvalidOperationException("Produto não encontrado.");
            existente.Codigo = produto.Codigo;
            existente.Nome = produto.Nome;
            existente.Descricao = produto.Descricao;
            existente.CategoriaId = produto.CategoriaId;
            existente.UnidadeMedidaId = produto.UnidadeMedidaId;
            existente.Tipo = produto.Tipo;
            existente.FotoUrl = produto.FotoUrl;
            existente.CodigoBarras = produto.CodigoBarras;
            existente.PrecoVenda = produto.PrecoVenda;
            existente.CustoEstimado = produto.CustoEstimado;
            existente.EstoqueAtual = produto.EstoqueAtual;
            existente.EstoqueMinimo = produto.EstoqueMinimo;
            existente.Ativo = produto.Ativo;
            existente.AtualizadoEm = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task AlternarAtivoAsync(int id)
    {
        var produto = await db.Produtos.FindAsync(id)
            ?? throw new InvalidOperationException("Produto não encontrado.");
        produto.Ativo = !produto.Ativo;
        produto.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var produto = await db.Produtos.FindAsync(id);
        if (produto is null) return;

        var emVenda = await db.VendaItens.AnyAsync(i => i.ProdutoId == id);
        var emPedido = await db.PedidoItens.AnyAsync(i => i.ProdutoId == id);
        var emPacote = await db.PacoteItens.AnyAsync(i => i.ProdutoId == id);
        if (emVenda || emPedido || emPacote)
            throw new InvalidOperationException("Não é possível excluir: produto já usado em vendas/pedidos/pacotes. Inative-o.");

        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();
    }
}
