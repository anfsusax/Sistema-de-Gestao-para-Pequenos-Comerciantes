using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Domain.Services;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

/// <summary>Serviços públicos da loja (sem login administrativo).</summary>
public class LojaPublicaService(SalgaFacilDbContext db, ClienteService clienteService)
{
    public Task<Empresa?> ObterEmpresaPorSlugAsync(string slug) =>
        db.Empresas.FirstOrDefaultAsync(e => e.Slug == slug.Trim().ToLowerInvariant() && e.Ativo);

    public async Task<List<CategoriaProduto>> ListarCategoriasAsync(int empresaId) =>
        await db.CategoriasProduto
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .OrderBy(c => c.Ordem).ThenBy(c => c.Nome)
            .ToListAsync();

    public async Task<List<Produto>> ListarProdutosAsync(int empresaId, int? categoriaId = null) =>
        await db.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.UnidadeMedida)
            .Where(p => p.EmpresaId == empresaId && p.Ativo)
            .Where(p => !categoriaId.HasValue || p.CategoriaId == categoriaId.Value)
            .OrderBy(p => p.Categoria.Ordem).ThenBy(p => p.Nome)
            .ToListAsync();

    public async Task<PedidoVisitanteResultado> CriarPedidoVisitanteAsync(int empresaId, PedidoVisitanteDto dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new InvalidOperationException("Informe seu nome.");
        if (string.IsNullOrWhiteSpace(dados.Telefone) && string.IsNullOrWhiteSpace(dados.WhatsApp))
            throw new InvalidOperationException("Informe telefone ou WhatsApp.");
        if (dados.Itens.Count == 0)
            throw new InvalidOperationException("Carrinho vazio.");
        if (dados.Itens.Any(i => i.Quantidade < PrecificacaoProduto.QuantidadeMinima))
            throw new InvalidOperationException("Quantidade inválida em um ou mais itens do carrinho.");
        if (dados.FormaPagamento is null)
            throw new InvalidOperationException("Selecione a forma de pagamento.");
        // E-mail é opcional no checkout público (mesma validação de formato usada no
        // cadastro administrativo — ClienteService.SalvarAsync), só recusa se preenchido errado.
        if (!string.IsNullOrWhiteSpace(dados.Email) && !dados.Email.Contains('@'))
            throw new InvalidOperationException("E-mail inválido.");

        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId && e.Ativo)
            ?? throw new InvalidOperationException("Loja não encontrada.");

        var produtoIds = dados.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos
            .Where(p => p.EmpresaId == empresaId && p.Ativo && produtoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (produtos.Count != produtoIds.Count)
            throw new InvalidOperationException("Um ou mais produtos não estão disponíveis.");

        // Ponto único de "achar ou criar cliente" (ClienteService.ObterOuCriarPorTelefoneAsync) —
        // evita criar um cliente novo a cada pedido quando o telefone já está cadastrado.
        // Ver _ia/DECISOES.md 2026-07-24.
        var (cliente, clienteJaExistia) = await clienteService.ObterOuCriarPorTelefoneAsync(
            empresaId, dados.Nome, dados.Telefone, dados.WhatsApp, dados.Email);

        if (!clienteJaExistia && dados.Entrega && !string.IsNullOrWhiteSpace(dados.EnderecoEntrega))
        {
            db.EnderecosCliente.Add(new EnderecoCliente
            {
                ClienteId = cliente.Id,
                Logradouro = dados.EnderecoEntrega.Trim(),
                Cidade = "—",
                Estado = "SP",
                Principal = true
            });
            await db.SaveChangesAsync();
        }

        var pedido = new Pedido
        {
            EmpresaId = empresaId,
            ClienteId = cliente.Id,
            Data = DateTime.UtcNow,
            Status = StatusPedido.Aguardando,
            Entrega = dados.Entrega,
            EnderecoEntrega = dados.EnderecoEntrega,
            Observacoes = dados.Observacoes,
            FormaPagamento = dados.FormaPagamento,
            // Pedido Pix novo começa em Aguardando (PIX-MANUAL-001). Demais formas de pagamento
            // não usam este campo — StatusPagamento fica nulo, como já era antes desta tarefa.
            StatusPagamento = dados.FormaPagamento == FormaPagamento.Pix ? StatusPagamento.Aguardando : null
        };

        foreach (var item in dados.Itens)
        {
            var produto = produtos[item.ProdutoId];
            pedido.Itens.Add(new PedidoItem
            {
                ProdutoId = produto.Id,
                Descricao = produto.Nome,
                Quantidade = item.Quantidade,
                ValorUnitario = PrecificacaoProduto.PrecoUnitario(produto, item.Quantidade),
                Total = PrecificacaoProduto.CalcularTotal(produto, item.Quantidade)
            });
        }
        pedido.Total = pedido.Itens.Sum(i => i.Total);
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();

        return new PedidoVisitanteResultado { PedidoId = pedido.Id, ClienteJaExistia = clienteJaExistia };
    }

    public async Task<int> CriarPedidoClienteAsync(int empresaId, int clienteId, PedidoClienteDto dados)
    {
        if (dados.Itens.Count == 0)
            throw new InvalidOperationException("Carrinho vazio.");
        if (dados.Itens.Any(i => i.Quantidade < PrecificacaoProduto.QuantidadeMinima))
            throw new InvalidOperationException("Quantidade inválida em um ou mais itens do carrinho.");
        if (dados.FormaPagamento is null)
            throw new InvalidOperationException("Selecione a forma de pagamento.");
        if (dados.Entrega && string.IsNullOrWhiteSpace(dados.EnderecoEntrega))
            throw new InvalidOperationException("Informe o endereço de entrega.");

        _ = await db.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId && e.Ativo)
            ?? throw new InvalidOperationException("Loja não encontrada.");

        var cliente = await db.Clientes
            .FirstOrDefaultAsync(c => c.Id == clienteId && c.EmpresaId == empresaId && c.Ativo)
            ?? throw new InvalidOperationException("Faça login novamente para continuar.");

        var produtoIds = dados.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos
            .Where(p => p.EmpresaId == empresaId && p.Ativo && produtoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (produtos.Count != produtoIds.Count)
            throw new InvalidOperationException("Um ou mais produtos não estão disponíveis.");

        if (dados.Entrega && !string.IsNullOrWhiteSpace(dados.EnderecoEntrega))
        {
            var endereco = dados.EnderecoEntrega.Trim();
            var jaExiste = await db.EnderecosCliente
                .AnyAsync(e => e.ClienteId == cliente.Id && e.Logradouro == endereco);
            if (!jaExiste)
            {
                db.EnderecosCliente.Add(new EnderecoCliente
                {
                    ClienteId = cliente.Id,
                    Logradouro = endereco,
                    Cidade = "—",
                    Estado = "SP",
                    Principal = !await db.EnderecosCliente.AnyAsync(e => e.ClienteId == cliente.Id)
                });
            }
        }

        var pedido = new Pedido
        {
            EmpresaId = empresaId,
            ClienteId = cliente.Id,
            Data = DateTime.UtcNow,
            Status = StatusPedido.Aguardando,
            Entrega = dados.Entrega,
            EnderecoEntrega = dados.Entrega ? dados.EnderecoEntrega?.Trim() : null,
            Observacoes = dados.Observacoes?.Trim(),
            FormaPagamento = dados.FormaPagamento,
            StatusPagamento = dados.FormaPagamento == FormaPagamento.Pix
                ? StatusPagamento.Aguardando
                : null
        };

        foreach (var item in dados.Itens)
        {
            var produto = produtos[item.ProdutoId];
            pedido.Itens.Add(new PedidoItem
            {
                ProdutoId = produto.Id,
                Descricao = produto.Nome,
                Quantidade = item.Quantidade,
                ValorUnitario = PrecificacaoProduto.PrecoUnitario(produto, item.Quantidade),
                Total = PrecificacaoProduto.CalcularTotal(produto, item.Quantidade)
            });
        }

        pedido.Total = pedido.Itens.Sum(i => i.Total);
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        return pedido.Id;
    }
    /// <summary>
    /// Lista os pedidos (com itens) do cliente identificado pelo telefone informado, do mais
    /// recente para o mais antigo. Não há login no cardápio público — o telefone é o único
    /// identificador, por isso reaproveita <see cref="ClienteService.BuscarPorTelefoneAsync"/>
    /// (mesma normalização e mesmo fallback de auto-cura usados no checkout). Retorna lista
    /// vazia se o telefone não corresponder a nenhum cliente — nunca lança erro nesse caso,
    /// para não expor se um telefone está ou não cadastrado.
    /// </summary>
    public async Task<List<Pedido>> ListarPedidosPorTelefoneAsync(int empresaId, string? telefone)
    {
        var cliente = await clienteService.BuscarPorTelefoneAsync(empresaId, telefone);
        if (cliente is null)
            return [];

        return await db.Pedidos
            .Include(p => p.Itens)
            .Where(p => p.ClienteId == cliente.Id && p.EmpresaId == empresaId)
            .OrderByDescending(p => p.Data)
            .ToListAsync();
    }
}

public class PedidoVisitanteDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public bool Entrega { get; set; }
    public string? EnderecoEntrega { get; set; }
    public string? Observacoes { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
    public List<ItemCarrinhoDto> Itens { get; set; } = [];
}

public class PedidoClienteDto
{
    public bool Entrega { get; set; }
    public string? EnderecoEntrega { get; set; }
    public string? Observacoes { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
    public List<ItemCarrinhoDto> Itens { get; set; } = [];
}
public class ItemCarrinhoDto
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}

public class PedidoVisitanteResultado
{
    public int PedidoId { get; set; }

    /// <summary>True quando o pedido foi vinculado a um cliente já cadastrado (mesmo telefone
    /// normalizado), em vez de um cliente novo. Usado pela UI para a mensagem amigável.</summary>
    public bool ClienteJaExistia { get; set; }
}
