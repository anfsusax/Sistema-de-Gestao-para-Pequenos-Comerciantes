using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class VendaService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    private int EmpresaId => empresa.RequireEmpresaId();

    public Task<List<Produto>> BuscarProdutosAsync(string termo) =>
        db.Produtos
            .Where(p => p.EmpresaId == EmpresaId && p.Ativo)
            .Where(p => p.Nome.Contains(termo) || (p.CodigoBarras != null && p.CodigoBarras == termo))
            .OrderBy(p => p.Nome)
            .Take(20)
            .ToListAsync();

    public Task<Produto?> BuscarPorCodigoBarrasAsync(string codigo) =>
        db.Produtos.FirstOrDefaultAsync(p => p.EmpresaId == EmpresaId && p.Ativo && p.CodigoBarras == codigo);

    public Task<List<Cliente>> BuscarClientesAsync(string termo) =>
        db.Clientes
            .Where(c => c.EmpresaId == EmpresaId)
            .Where(c => c.Nome.Contains(termo) || (c.Cpf != null && c.Cpf.Contains(termo)))
            .OrderBy(c => c.Nome)
            .Take(20)
            .ToListAsync();

    public Task<Venda?> ObterAsync(int id) =>
        db.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == EmpresaId);

    /// <remarks>
    /// BUG EVITADO: comparar "v.Data.Date == data.Value.Date" direto compararia o dia em UTC
    /// (como Data é gravada) com o dia local escolhido na tela — no fuso do Brasil (UTC-3),
    /// vendas feitas à noite (≈21h–23h59 local) já caem no dia seguinte em UTC e sumiriam do
    /// filtro "hoje". Em vez disso, convertemos o dia local escolhido para a janela [00h,24h)
    /// correspondente em UTC e comparamos por intervalo, não por igualdade de "Date".
    /// </remarks>
    public Task<List<Venda>> ListarAsync(DateTime? data = null, int? sessaoCaixaId = null)
    {
        DateTime? inicioUtc = data.HasValue ? data.Value.Date.ToUniversalTime() : null;
        DateTime? fimUtc = data.HasValue ? data.Value.Date.AddDays(1).ToUniversalTime() : null;

        return db.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
            .Where(v => v.EmpresaId == EmpresaId)
            .Where(v => !inicioUtc.HasValue || (v.Data >= inicioUtc.Value && v.Data < fimUtc!.Value))
            .Where(v => !sessaoCaixaId.HasValue || v.SessaoCaixaId == sessaoCaixaId.Value)
            .OrderByDescending(v => v.Data)
            .ToListAsync();
    }

    /// <remarks>
    /// Validações de invariante ficam aqui (não só na UI): mesmo que o botão "Finalizar Venda"
    /// esteja desabilitado no cliente, o service não deve confiar apenas nisso — qualquer outro
    /// consumidor futuro (API, outra tela) passaria por aqui e precisa das mesmas garantias.
    /// Estoque é validado ANTES de qualquer escrita (evita decrementar parcialmente e falhar no
    /// meio de uma venda com vários itens).
    /// </remarks>
    public async Task<int> CriarAsync(NovaVenda dados)
    {
        if (dados.Itens.Count == 0)
            throw new InvalidOperationException("Venda precisa ter ao menos um item.");

        var sessao = await db.SessoesCaixa.FirstOrDefaultAsync(s => s.Id == dados.SessaoCaixaId && s.EmpresaId == EmpresaId);
        if (sessao == null || sessao.Status != StatusSessaoCaixa.Aberta)
            throw new InvalidOperationException("Não há uma sessão de caixa aberta. Abra o caixa antes de vender.");

        var subtotal = dados.Itens.Sum(i => i.Quantidade * i.ValorUnitario);
        var total = subtotal - dados.Desconto;
        if (total < 0)
            throw new InvalidOperationException("Desconto não pode ser maior que o subtotal da venda.");

        var recebido = dados.FormaPagamento == FormaPagamento.Dinheiro ? dados.ValorRecebido : total;
        var troco = dados.FormaPagamento == FormaPagamento.Dinheiro ? dados.ValorRecebido - total : 0m;
        if (dados.FormaPagamento == FormaPagamento.Dinheiro && troco < 0)
            throw new InvalidOperationException("Valor recebido é menor que o total da venda.");

        var produtoIds = dados.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos.Where(p => p.EmpresaId == EmpresaId && produtoIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        var estoqueNecessario = new Dictionary<int, int>();
        foreach (var item in dados.Itens)
        {
            if (!produtos.TryGetValue(item.ProdutoId, out var produto))
                throw new InvalidOperationException("Um dos produtos da venda não foi encontrado.");
            estoqueNecessario[item.ProdutoId] = estoqueNecessario.GetValueOrDefault(item.ProdutoId) + item.Quantidade;
            if (produto.EstoqueAtual < estoqueNecessario[item.ProdutoId])
                throw new InvalidOperationException($"Estoque insuficiente de \"{produto.Nome}\" (disponível: {produto.EstoqueAtual}, necessário: {estoqueNecessario[item.ProdutoId]}).");
        }

        var venda = new Venda
        {
            EmpresaId = EmpresaId,
            ClienteId = dados.ClienteId,
            UsuarioId = dados.UsuarioId,
            SessaoCaixaId = dados.SessaoCaixaId,
            Data = DateTime.UtcNow,
            Subtotal = subtotal,
            Desconto = dados.Desconto,
            Total = total,
            FormaPagamento = dados.FormaPagamento,
            ValorRecebido = recebido,
            Troco = troco,
            Status = StatusVenda.Finalizada
        };

        foreach (var item in dados.Itens)
        {
            venda.Itens.Add(new VendaItem
            {
                ProdutoId = item.ProdutoId,
                Descricao = item.Descricao,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario,
                Total = item.Quantidade * item.ValorUnitario
            });
            produtos[item.ProdutoId].EstoqueAtual -= item.Quantidade;
        }

        db.Vendas.Add(venda);
        await db.SaveChangesAsync();
        return venda.Id;
    }

    /// <remarks>
    /// BUG EVITADO: a versão anterior desta função só marcava Status=Cancelada, sem devolver o
    /// estoque baixado na venda original — cancelar uma venda faria o estoque "sumir"
    /// permanentemente. Corrigido para repor o estoque de cada item ao cancelar.
    /// </remarks>
    public async Task CancelarAsync(int id)
    {
        var venda = await db.Vendas.Include(v => v.Itens).FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == EmpresaId);
        if (venda == null || venda.Status == StatusVenda.Cancelada) return;

        var produtoIds = venda.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos.Where(p => p.EmpresaId == EmpresaId && produtoIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        foreach (var item in venda.Itens)
            if (produtos.TryGetValue(item.ProdutoId, out var produto))
                produto.EstoqueAtual += item.Quantidade;

        venda.Status = StatusVenda.Cancelada;
        await db.SaveChangesAsync();
    }
}

public class NovaVenda
{
    public int? ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public int SessaoCaixaId { get; set; }
    public decimal Desconto { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public decimal ValorRecebido { get; set; }
    public List<NovoVendaItem> Itens { get; set; } = [];
}

public class NovoVendaItem
{
    public int ProdutoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}
